
using System.ComponentModel.DataAnnotations;

namespace PuraRuta.ViewModels
{
    public class RutaViewModel
    {
        [Required(ErrorMessage = "El nombre de la ruta es obligatorio.")]
        [StringLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una fecha de visita.")]
        public DateOnly? FechaVisita { get; set; }

        [Required(ErrorMessage = "Selecciona al menos dos experiencias.")]
        [MinLength(2, ErrorMessage = "Debes seleccionar al menos 2 experiencias.")]
        [MaxLength(5, ErrorMessage = "Puedes seleccionar máximo 5 experiencias.")]
        public List<int> ExperienciasSeleccionadas { get; set; } = new();
    }
}
