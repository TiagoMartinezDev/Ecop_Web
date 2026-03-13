using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECOP.Web.Models.ViewModels
{
    public class ProductoViewModels
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
    }

    public class ProductoFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [Display(Name = "Unidad de Medida")]
        public int UnidadMedidaId { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 9999999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        public IEnumerable<SelectListItem> UnidadesMedida { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}