
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PuraRuta.ViewModels
{
    public class ExperienciaViewModel
    {
        public int IdExperiencia { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string DescripcionExperiencia { get; set; } = string.Empty;

        [Required]
        public int? IdComunidad { get; set; }

        [Required]
        public int? IdCategoria { get; set; }

        [Required]
        public int? IdProvincia { get; set; }

        [Required]
        public int? IdCanton { get; set; }

        [Required]
        public int? IdDistrito { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(150)]
        public string DireccionExacta { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public decimal? Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal? Longitud { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal Precio { get; set; }

        [Range(typeof(decimal), "0.1", "999999")]
        public decimal DuracionHoras { get; set; }

        [Range(0, 120)]
        public int EdadMinima { get; set; }

        [Required]
        public string Dificultad { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int CapacidadDiaria { get; set; }

        public bool TransportePublico { get; set; }

        public bool Estado { get; set; } = true;

        public List<IFormFile>? Fotografias { get; set; } = new();

        public List<string> FotografiasExistentes { get; set; } = new();
    }
}
