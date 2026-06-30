namespace ECOP.Web.Controllers;

public class ClientesController( IClienteRepositorio clienteRepo, ITipoDocumentoRepositorio tipoDocRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clientes = await clienteRepo.ObtenerTodosAsync();
        var vm = clientes.Select(c => new ClienteViewModel
        {
            Id = c.Id,
            NombreCompleto = $"{c.Nombre} {c.Apellido}",
            TipoDocumento = c.TipoDocumento?.Descripcion ?? "-",
            NroDocumento = c.NroDocumento,
            Email = c.Email,
            Telefono = c.Telefono,
            Activo = c.Activo,
            FechaAlta = c.FechaAlta
        }).OrderByDescending(c => c.Activo)
          .ThenBy(c => c.NombreCompleto);
        return View(vm);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var cliente = await clienteRepo.ObtenerPorIdConDetalleAsync(id);
        if (cliente is null) 
            return NotFound();

        var vm = new ClienteViewModel
        {
            Id = cliente.Id,
            NombreCompleto = $"{cliente.Nombre} {cliente.Apellido}",
            TipoDocumento = cliente.TipoDocumento?.Descripcion ?? "-",
            NroDocumento = cliente.NroDocumento,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Activo = cliente.Activo,
            FechaAlta = cliente.FechaAlta
        };
        return View(vm);
    }

    public async Task<IActionResult> Crear()
    {
        var vm = new ClienteFormViewModel
        {
            TiposDocumento = await GetTiposDocumentoSelectAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.TiposDocumento = await GetTiposDocumentoSelectAsync();
            return View(vm);
        }

        var existe = await clienteRepo.ExisteDocumentoAsync(vm.TipoDocumentoId, vm.NroDocumento);
        if (existe)
        {
            ModelState.AddModelError(nameof(vm.NroDocumento), "Ya existe un cliente con ese documento.");
            vm.TiposDocumento = await GetTiposDocumentoSelectAsync();
            return View(vm);
        }

        var cliente = new Cliente
        {
            Nombre = vm.Nombre,
            Apellido = vm.Apellido,
            TipoDocumentoId = vm.TipoDocumentoId,
            NroDocumento = vm.NroDocumento,
            Email = vm.Email,
            Telefono = vm.Telefono,
            Activo = true,
            FechaAlta = DateTime.Now
        };

        await clienteRepo.CrearAsync(cliente);
        TempData["Exito"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await clienteRepo.ObtenerPorIdConDetalleAsync(id);
        if (cliente is null) return NotFound();

        var vm = new ClienteFormViewModel
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            TipoDocumentoId = cliente.TipoDocumentoId,
            NroDocumento = cliente.NroDocumento,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            TiposDocumento = await GetTiposDocumentoSelectAsync(),
            Activo = cliente.Activo
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ClienteFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            vm.TiposDocumento = await GetTiposDocumentoSelectAsync();
            return View(vm);
        }

        var existe = await clienteRepo.ExisteDocumentoAsync(vm.TipoDocumentoId, vm.NroDocumento, id);
        if (existe)
        {
            ModelState.AddModelError(nameof(vm.NroDocumento), "Ya existe otro cliente con ese documento.");
            vm.TiposDocumento = await GetTiposDocumentoSelectAsync();
            return View(vm);
        }

        var cliente = await clienteRepo.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();

        cliente.Nombre = vm.Nombre;
        cliente.Apellido = vm.Apellido;
        cliente.TipoDocumentoId = vm.TipoDocumentoId;
        cliente.NroDocumento = vm.NroDocumento;
        cliente.Email = vm.Email;
        cliente.Telefono = vm.Telefono;
        cliente.FechaModificacion = DateTime.Now;

        await clienteRepo.ActualizarAsync(cliente);
        TempData["Exito"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        /*await clienteRepo.EliminarAsync(id);
        TempData["Exito"] = "Cliente eliminado correctamente.";*/
        TempData["Error"] = "Esto es un mensaje de error de prueba.";
        return RedirectToAction(nameof(Index));
       // return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetTiposDocumentoSelectAsync()
    {
        var tipos = await tipoDocRepo.ObtenerActivosAsync();
        return tipos.Select(t => new SelectListItem(t.Descripcion, t.Id.ToString()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarActivo(int id)
    {
        var cliente = await clienteRepo.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();

        cliente.Activo = !cliente.Activo;
        cliente.FechaModificacion = DateTime.UtcNow;

        await clienteRepo.ActualizarAsync(cliente);
        TempData["Exito"] = cliente.Activo ? "Cliente activado." : "Cliente desactivado.";
        return RedirectToAction(nameof(Editar), new { id });
    }
}
