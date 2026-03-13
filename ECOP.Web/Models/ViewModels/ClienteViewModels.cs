using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECOP.Web.Models.ViewModels
{
    public class ClienteViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string NroDocumento { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public class ClienteFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [Display(Name = "Tipo de Documento")]
        public int TipoDocumentoId { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [MaxLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [Display(Name = "Nro. Documento")]
        public string NroDocumento { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        public IEnumerable<SelectListItem> TiposDocumento { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}