# ECOP.Web — Sistema de Pedidos de Mercaderías (Web MVC)

Aplicación web ASP.NET Core 9 MVC para la administración completa de clientes, productos y pedidos de mercaderías.

---


## Requisitos

| Componente | Versión |
|---|---|
| .NET SDK | 9.0 |
| Visual Studio | 2022 v17.8+ |
| SQL Server Express | 2019 o 2022 |


---


## ⚙ Configuración inicial

### 1. Cadena de conexión

Editá `ECOP.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SERVIDOR\\INSTANCIA;Initial Catalog=ECOP_PedidosMercaderias;User ID=sa;Password=TU_PASSWORD;Encrypt=True;TrustServerCertificate=True"
  }
}
```

### 2. Elegir proveedor de datos

En `appsettings.json` cambiar esta línea dependiendo al proveedor de datos:

```json
"DataProvider": "EntityFramework"   
"DataProvider": "Dapper"            
```

Con un solo cambio el sistema cambia de implementación sin tocar código.

---

## 🗄 Creación de la base de datos

Elegir **una sola** de las opciones:

### Opción A — Migraciones de EF Core ✅ Recomendada

Abrir la **Package Manager Console** en Visual Studio (`Herramientas → NuGet → Package Manager Console`) y ejecutá:

```powershell
Add-Migration InitialMigration -Project ECOP.AccesoDatos -StartupProject ECOP.API
Update-Database -Project ECOP.AccesoDatos -StartupProject ECOP.API
```

EF Core creará la base de datos y todas las tablas automáticamente.


### Opción B — Scripts SQL manuales

Ejecutá en SSMS en este orden:

```
Database/01_CrearTablas.sql
Database/02_DatosPrueba.sql 
```

> **Si usás la Opción B y luego querés usar migraciones**, ejecutá esto para que EF no intente recrear las tablas:
> ```sql
> INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
> VALUES ('InitialMigration', '9.0.4');
> ```

---

## Funcionalidades

### Clientes
- Listado con nombre, documento, email, teléfono y estado
- Crear nuevo cliente con validación de documento duplicado
- Editar datos del cliente
- Ver detalle completo
- Activar / desactivar cliente (baja lógica)

### Productos
- Listado con código, descripción, unidad de medida, precio y stock
- Crear nuevo producto con validación de código duplicado
- Editar producto (el código no es modificable una vez creado)
- Ver detalle completo
- Activar / desactivar producto

### Pedidos
- Listado con número, cliente, fecha, estado y total
- Crear pedido con selección dinámica de productos via AJAX
  - Validación de stock en tiempo real
  - Cálculo automático de subtotales y total
  - Generación automática de número de pedido (`YYYYMM#####`)
- Editar pedido existente (ajuste de stock automático)
- Ver detalle completo del pedido
- Cambiar estado del pedido con las siguientes reglas:
  - `CANCELADO` y `ENTREGADO` no permiten más cambios de estado
  - Al cancelar un pedido se devuelve el stock automáticamente
  - Al reactivar desde `CANCELADO` se descuenta el stock nuevamente

---


## Estructura del proyecto

```
ECOP.Web/
├── Program.cs
├── appsettings.json
├── Controllers/
│   ├── HomeController.cs
│   ├── ClientesController.cs
│   ├── ProductosController.cs
│   └── PedidosController.cs
├── Models/
│   └── ViewModels/
│       └── ViewModels.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScripts.cshtml
│   ├── Clientes/
│   │   ├── Index.cshtml
│   │   ├── Crear.cshtml
│   │   ├── Editar.cshtml
│   │   ├── Detalle.cshtml
│   │   └── _Form.cshtml
│   ├── Productos/
│   │   ├── Index.cshtml
│   │   ├── Crear.cshtml
│   │   └── Editar.cshtml
│   └── Pedidos/
│       ├── Index.cshtml
│       ├── Crear.cshtml
│       ├── Editar.cshtml
│       ├── Detalle.cshtml
│       └── CambiarEstado.cshtml
└── wwwroot/
    └── css/
        └── site.css
```

---