
using System.ComponentModel.DataAnnotations;

namespace PuraRuta.ViewModels
{
    public class RegistroTuristaViewModel
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido1 { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido2 { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{8}$",
            ErrorMessage = "El teléfono debe tener 8 dígitos.")]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [Required]
        public DateOnly? FechaNacimiento { get; set; }

        [Required]
        public int? IdProvincia { get; set; }

        [Required]
        public int? IdCanton { get; set; }

        [Required]
        public int? IdDistrito { get; set; }

        [Required]
        [StringLength(150)]
        public string DireccionExacta { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public decimal? Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal? Longitud { get; set; }

        public bool TieneVehiculo { get; set; }
    }
}
