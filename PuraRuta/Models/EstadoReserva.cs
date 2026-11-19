using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class EstadoReserva
{
    public int IdEstadoReserva { get; set; }

    public string Nombre { get; set; } = null!;

    public string DescripcionEstadoReserva { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
