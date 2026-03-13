using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECOP.Web.Models.ViewModels
{
    public class PedidoListaViewModel
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal TotalMonto { get; set; }
        public string? Observaciones { get; set; }
    }

    public class PedidoDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<DetallePedidoViewModel> Detalles { get; set; } = new List<DetallePedidoViewModel>();
        public int TotalCantidad => Detalles.Sum(d => d.Cantidad);
        public decimal TotalMonto => Detalles.Sum(d => d.Subtotal);
    }

    public class DetallePedidoViewModel
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class PedidoFormViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        public List<DetallePedidoFormItem> Items { get; set; } = new List<DetallePedidoFormItem>();

        public IEnumerable<SelectListItem> Clientes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Productos { get; set; } = Enumerable.Empty<SelectListItem>();

        public int TotalCantidad => Items.Sum(i => i.Cantidad);
        public decimal TotalMonto => Items.Sum(i => i.Subtotal);
    }

    public class DetallePedidoFormItem
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class CambiarEstadoViewModel
    {
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public int EstadoId { get; set; }
        public IEnumerable<SelectListItem> Estados { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    public class PedidoEditarViewModel
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        public List<DetallePedidoFormItem> Items { get; set; } = [];

        public IEnumerable<SelectListItem> Clientes { get; set; } = [];
        public IEnumerable<SelectListItem> Productos { get; set; } = [];

        public int TotalCantidad => Items.Sum(i => i.Cantidad);
        public decimal TotalMonto => Items.Sum(i => i.Subtotal);
    }
}