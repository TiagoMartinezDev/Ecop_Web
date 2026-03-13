namespace ECOP.Web.Controllers;

public class PedidosController(
    IPedidoRepositorio pedidoRepo,
    IClienteRepositorio clienteRepo,
    IProductoRepositorio productoRepo,
    IEstadoPedidoRepositorio estadoRepo) : Controller
{
    public async Task<IActionResult> Index(int? clienteId, string? estado)
    {
        var pedidos = await pedidoRepo.ObtenerTodosAsync(clienteId, estado);
        var vm = pedidos.Select(p => new PedidoListaViewModel
        {
            Id = p.Id,
            NumeroPedido = p.NumeroPedido,
            Cliente = p.Cliente is not null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "-",
            FechaPedido = p.FechaPedido,
            Estado = p.Estado?.Descripcion ?? "-",
            TotalMonto = p.TotalMonto,
            Observaciones = p.Observaciones
        });

        ViewBag.Clientes = new SelectList(await clienteRepo.ObtenerActivosAsync(), "Id", "NombreCompleto", clienteId);
        ViewBag.Estados = new SelectList(await estadoRepo.ObtenerTodosAsync(), "Codigo", "Descripcion", estado);
        ViewBag.ClienteId = clienteId;
        ViewBag.Estado = estado;

        return View(vm);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var pedido = await pedidoRepo.ObtenerPorIdAsync(id);
        if (pedido is null) return NotFound();

        var vm = new PedidoDetalleViewModel
        {
            Id = pedido.Id,
            NumeroPedido = pedido.NumeroPedido,
            Cliente = pedido.Cliente is not null ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}" : "-",
            FechaPedido = pedido.FechaPedido,
            Estado = pedido.Estado?.Descripcion ?? "-",
            Observaciones = pedido.Observaciones,
            Detalles = pedido.Detalles.Select(d => new DetallePedidoViewModel
            {
                ProductoId = d.ProductoId,
                Codigo = d.Producto?.Codigo ?? "-",
                Descripcion = d.Producto?.Descripcion ?? "-",
                UnidadMedida = d.Producto?.UnidadMedida?.Descripcion ?? "-",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Crear()
    {
        var vm = new PedidoFormViewModel
        {
            Clientes = await GetClientesSelectAsync(),
            Productos = await GetProductosSelectAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProducto(int id)
    {
        var producto = await productoRepo.ObtenerPorIdConDetalleAsync(id);
        if (producto is null) return NotFound();
        return Json(new
        {
            id = producto.Id,
            codigo = producto.Codigo,
            descripcion = producto.Descripcion,
            unidadMedida = producto.UnidadMedida?.Descripcion ?? "-",
            precioUnitario = producto.PrecioUnitario,
            stock = producto.Stock
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoFormViewModel vm)
    {
        if (vm.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto al pedido.");

        if (!ModelState.IsValid)
        {
            vm.Clientes  = await GetClientesSelectAsync();
            vm.Productos = await GetProductosSelectAsync();
            return View(vm);
        }

        // Generar número de pedido
        var ahora   = DateTime.Now;
        var prefijo = $"{ahora:yyyyMM}";
        var seq     = await pedidoRepo.PrefijoParaPedidoAsync(prefijo) + 1;
        var nroPedido = $"{prefijo}{seq:D5}";

        var estadoPendiente = await estadoRepo.ObtenerPorCodigoAsync("PENDIENTE");

        var pedido = new Pedido
        {
            NumeroPedido  = nroPedido,
            ClienteId     = vm.ClienteId,
            FechaPedido   = DateTime.UtcNow,
            EstadoId      = estadoPendiente!.Id,
            TotalMonto    = vm.TotalMonto,
            Observaciones = vm.Observaciones,
            Detalles      = vm.Items.Select(i => new DetallePedido
            {
                ProductoId     = i.ProductoId,
                Cantidad       = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList()
        };

        await pedidoRepo.CrearAsync(pedido);

        // Descontar stock de cada producto
        foreach (var item in vm.Items)
            await productoRepo.AjustarStockAsync(item.ProductoId, -item.Cantidad);

        TempData["Exito"] = $"Pedido {nroPedido} creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CambiarEstado(int id)
    {
        var pedido = await pedidoRepo.ObtenerPorIdAsync(id);
        if (pedido is null) return NotFound();

        var vm = new CambiarEstadoViewModel
        {
            PedidoId = pedido.Id,
            NumeroPedido = pedido.NumeroPedido,
            EstadoId = pedido.EstadoId,
            Estados = (await estadoRepo.ObtenerTodosAsync())
                           .Select(e => new SelectListItem(e.Descripcion, e.Id.ToString()))
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoViewModel vm)
    {
        var pedido = await pedidoRepo.ObtenerPorIdAsync(vm.PedidoId);
        if (pedido is null) return NotFound();

        var estadoNuevo = await estadoRepo.ObtenerPorIdAsync(vm.EstadoId);
        if (estadoNuevo is null) return BadRequest();

        var codigoAnterior = pedido.Estado?.Codigo;
        var codigoNuevo = estadoNuevo.Codigo;

        // Cancelando → devolver stock
        if (codigoNuevo == "CANCELADO" && codigoAnterior != "CANCELADO")
            foreach (var det in pedido.Detalles)
                await productoRepo.AjustarStockAsync(det.ProductoId, +det.Cantidad);

        // Reactivando desde Cancelado → descontar stock
        if (codigoAnterior == "CANCELADO" && codigoNuevo != "CANCELADO")
            foreach (var det in pedido.Detalles)
                await productoRepo.AjustarStockAsync(det.ProductoId, -det.Cantidad);

        await pedidoRepo.CambiarEstadoAsync(vm.PedidoId, vm.EstadoId);
        TempData["Exito"] = "Estado del pedido actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = vm.PedidoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var pedido = await pedidoRepo.ObtenerPorIdAsync(id);
        if (pedido is null) return NotFound();

        var estadoCancelado = await estadoRepo.ObtenerPorCodigoAsync("CANCELADO");
        if (estadoCancelado is null) return BadRequest();

        await pedidoRepo.CambiarEstadoAsync(id, estadoCancelado.Id);

        // Devolver stock de todos los productos del pedido
        foreach (var det in pedido.Detalles)
            await productoRepo.AjustarStockAsync(det.ProductoId, +det.Cantidad);

        TempData["Exito"] = "Pedido cancelado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var pedido = await pedidoRepo.ObtenerPorIdAsync(id);
        if (pedido is null) return NotFound();

        if (pedido.Estado?.Codigo is "CANCELADO" or "ENTREGADO")
        {
            TempData["Error"] = $"No se puede editar un pedido en estado {pedido.Estado.Descripcion}.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var vm = new PedidoEditarViewModel
        {
            Id = pedido.Id,
            NumeroPedido = pedido.NumeroPedido,
            EstadoActual = pedido.Estado?.Descripcion ?? "-",
            ClienteId = pedido.ClienteId,
            Observaciones = pedido.Observaciones,
            Clientes = await GetClientesSelectAsync(),
            Productos = await GetProductosSelectAsync(),
            Items = pedido.Detalles.Select(d => new DetallePedidoFormItem
            {
                ProductoId = d.ProductoId,
                Codigo = d.Producto?.Codigo ?? "-",
                Descripcion = d.Producto?.Descripcion ?? "-",
                UnidadMedida = d.Producto?.UnidadMedida?.Descripcion ?? "-",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PedidoEditarViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (vm.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto al pedido.");

        if (!ModelState.IsValid)
        {
            vm.Clientes = await GetClientesSelectAsync();
            vm.Productos = await GetProductosSelectAsync();
            return View(vm);
        }

        // Obtener detalles anteriores para devolver stock
        var pedidoAnterior = await pedidoRepo.ObtenerPorIdAsync(id);
        if (pedidoAnterior is null) return NotFound();

        var pedido = new Pedido
        {
            Id = vm.Id,
            NumeroPedido = vm.NumeroPedido,
            ClienteId = vm.ClienteId,
            TotalMonto = vm.TotalMonto,
            Observaciones = vm.Observaciones,
            Detalles = vm.Items.Select(i => new DetallePedido
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList()
        };

        // Devolver stock anterior
        foreach (var det in pedidoAnterior.Detalles)
            await productoRepo.AjustarStockAsync(det.ProductoId, +det.Cantidad);

        await pedidoRepo.ActualizarAsync(pedido);

        // Descontar stock nuevo
        foreach (var item in vm.Items)
            await productoRepo.AjustarStockAsync(item.ProductoId, -item.Cantidad);

        TempData["Exito"] = $"Pedido {vm.NumeroPedido} actualizado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }
    private async Task<IEnumerable<SelectListItem>> GetClientesSelectAsync()
    {
        var clientes = await clienteRepo.ObtenerActivosAsync();
        return clientes.Select(c => new SelectListItem($"{c.Nombre} {c.Apellido}", c.Id.ToString()));
    }

    private async Task<IEnumerable<SelectListItem>> GetProductosSelectAsync()
    {
        var productos = await productoRepo.ObtenerActivosAsync();
        return productos.Select(p => new SelectListItem($"[{p.Codigo}] {p.Descripcion}", p.Id.ToString()));
    }
}