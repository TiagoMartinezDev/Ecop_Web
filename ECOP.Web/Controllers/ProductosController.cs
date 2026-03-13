namespace ECOP.Web.Controllers;

public class ProductosController(IProductoRepositorio productoRepo, IUnidadMedidaRepositorio unidadRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        var productos = await productoRepo.ObtenerTodosAsync();
        var vm = productos.Select(p => new ProductoViewModels
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            UnidadMedida = p.UnidadMedida?.Descripcion ?? "-",
            PrecioUnitario = p.PrecioUnitario,
            Stock = p.Stock,
            Activo = p.Activo
        }).OrderByDescending(p => p.Activo)
          .ThenBy(p => p.Codigo);
        return View(vm);
    }

    public async Task<IActionResult> Crear()
    {
        var vm = new ProductoFormViewModel
        {
            UnidadesMedida = await GetUnidadesSelectAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.UnidadesMedida = await GetUnidadesSelectAsync();
            return View(vm);
        }

        var existe = await productoRepo.ExisteCodigoAsync(vm.Codigo);
        if (existe)
        {
            ModelState.AddModelError(nameof(vm.Codigo), "Ya existe un producto con ese código.");
            vm.UnidadesMedida = await GetUnidadesSelectAsync();
            return View(vm);
        }

        var producto = new Producto
        {
            Codigo = vm.Codigo.ToUpper(),
            Descripcion = vm.Descripcion,
            UnidadMedidaId = vm.UnidadMedidaId,
            PrecioUnitario = vm.PrecioUnitario,
            Stock = vm.Stock,
            Activo = true,
            FechaAlta = DateTime.UtcNow
        };

        await productoRepo.CrearAsync(producto);
        TempData["Exito"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var producto = await productoRepo.ObtenerPorIdConDetalleAsync(id);
        if (producto is null) return NotFound();

        var vm = new ProductoFormViewModel
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Descripcion = producto.Descripcion,
            UnidadMedidaId = producto.UnidadMedidaId,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            Activo = producto.Activo,
            UnidadesMedida = await GetUnidadesSelectAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            vm.UnidadesMedida = await GetUnidadesSelectAsync();
            return View(vm);
        }

        var existe = await productoRepo.ExisteCodigoAsync(vm.Codigo, id);
        if (existe)
        {
            ModelState.AddModelError(nameof(vm.Codigo), "Ya existe otro producto con ese código.");
            vm.UnidadesMedida = await GetUnidadesSelectAsync();
            return View(vm);
        }

        var producto = await productoRepo.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();

        producto.Descripcion = vm.Descripcion;
        producto.UnidadMedidaId = vm.UnidadMedidaId;
        producto.PrecioUnitario = vm.PrecioUnitario;
        producto.Stock = vm.Stock;
        producto.FechaModificacion = DateTime.UtcNow;

        await productoRepo.ActualizarAsync(producto);
        TempData["Exito"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await productoRepo.EliminarAsync(id);
        TempData["Exito"] = "Producto eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetUnidadesSelectAsync()
    {
        var unidades = await unidadRepo.ObtenerActivasAsync();
        return unidades.Select(u => new SelectListItem(u.Descripcion, u.Id.ToString()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarActivo(int id)
    {
        var cliente = await productoRepo.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();

        cliente.Activo = !cliente.Activo;
        cliente.FechaModificacion = DateTime.UtcNow;

        await productoRepo.ActualizarAsync(cliente);
        TempData["Exito"] = cliente.Activo ? "Producto activado." : "Cliente desactivado.";
        return RedirectToAction(nameof(Editar), new { id });
    }
}
