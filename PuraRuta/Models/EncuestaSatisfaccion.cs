using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class EncuestaSatisfaccion
{
    public int IdEncuesta { get; set; }

    public int IdReserva { get; set; }

    public int Calificacion { get; set; }

    public string? Comentario { get; set; }

    public bool Recomendaria { get; set; }

    public DateTime FechaEncuesta { get; set; }

    public virtual Reserva IdReservaNavigation { get; set; } = null!;
}
