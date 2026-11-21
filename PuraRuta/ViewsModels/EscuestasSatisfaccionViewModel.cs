
using System.ComponentModel.DataAnnotations;

namespace PuraRuta.ViewModels
{
    public class EncuestaSatisfaccionViewModel
    {
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "Selecciona una calificación.")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "Escribe un comentario.")]
        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;

        public bool Recomendaria { get; set; }
    }
}
