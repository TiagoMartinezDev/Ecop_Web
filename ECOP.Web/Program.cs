using ECOP.AccesoDatos.Data.EF;
using ECOP.AccesoDatos.Data.Dapper;
using ECOP.AccesoDatos.Repositories.EF;
using ECOP.AccesoDatos.Repositories.Dapper;
using ECOP.AccesoDatos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var provider = builder.Configuration["DataProvider"] ?? "EntityFramework";
var connStr  = builder.Configuration.GetConnectionString("DefaultConnection")!;

if (provider == "EntityFramework")
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(connStr));

    builder.Services.AddScoped<IClienteRepositorio, EFClienteRepositorio>();
    builder.Services.AddScoped<ITipoDocumentoRepositorio, EFTipoDocumentoRepositorio>();
    builder.Services.AddScoped<IProductoRepositorio, EFProductoRepositorio>();
    builder.Services.AddScoped<IUnidadMedidaRepositorio, EFUnidadMedidaRepositorio>();
    builder.Services.AddScoped<IEstadoPedidoRepositorio, EFEstadoPedidoRepositorio>();
    builder.Services.AddScoped<IPedidoRepositorio, EFPedidoRepositorio>();
}
else
{
    builder.Services.AddSingleton<IDapperConnectionFactory>(_ =>
    new DapperConnectionFactory(builder.Configuration));

    builder.Services.AddScoped<IClienteRepositorio, DapperClienteRepositorio>();
    builder.Services.AddScoped<ITipoDocumentoRepositorio, DapperTipoDocumentoRepositorio>();
    builder.Services.AddScoped<IProductoRepositorio, DapperProductoRepositorio>();
    builder.Services.AddScoped<IUnidadMedidaRepositorio, DapperUnidadMedidaRepositorio>();
    builder.Services.AddScoped<IEstadoPedidoRepositorio, DapperEstadoPedidoRepositorio>();
    builder.Services.AddScoped<IPedidoRepositorio, DapperPedidoRepositorio>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
