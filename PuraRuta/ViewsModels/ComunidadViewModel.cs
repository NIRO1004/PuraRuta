
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PuraRuta.ViewModels
{
    public class ComunidadViewModel
    {
        public int IdComunidad { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string DescripcionComunidad { get; set; } = string.Empty;

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

        public bool Estado { get; set; } = true;

        public List<IFormFile> Fotografias { get; set; } = new();

        public List<string> FotografiasExistentes { get; set; } = new();
    }
}
