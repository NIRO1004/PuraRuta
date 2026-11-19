using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdTurista { get; set; }

    public int IdExperiencia { get; set; }

    public int IdEstadoReserva { get; set; }

    public DateTime FechaReserva { get; set; }

    public DateOnly FechaVisita { get; set; }

    public int CantidadPersonas { get; set; }

    public decimal PrecioTotal { get; set; }

    public virtual EncuestaSatisfaccion? EncuestaSatisfaccion { get; set; }

    public virtual EstadoReserva IdEstadoReservaNavigation { get; set; } = null!;

    public virtual Experiencium IdExperienciaNavigation { get; set; } = null!;

    public virtual Turistum IdTuristaNavigation { get; set; } = null!;
}
