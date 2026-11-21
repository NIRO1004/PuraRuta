
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PuraRuta.ViewModels
{
    public class ReservaViewModel
    {
        public int IdExperiencia { get; set; }

        [Required(ErrorMessage = "Selecciona una fecha de visita.")]
        public DateOnly? FechaVisita { get; set; }

        [Required(ErrorMessage = "Indica la cantidad de personas.")]
        [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20 personas.")]
        public int CantidadPersonas { get; set; } = 1;

        public List<int> EdadesAcompanantes { get; set; } = new();
    }
}
