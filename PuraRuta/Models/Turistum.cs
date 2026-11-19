using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Turistum
{
    public int IdTurista { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public int IdDireccion { get; set; }

    public bool TieneVehiculo { get; set; }

    public virtual Direccion IdDireccionNavigation { get; set; } = null!;

    public virtual Usuario IdTuristaNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Rutum> Ruta { get; set; } = new List<Rutum>();

    public virtual ICollection<TuristaCategorium> TuristaCategoria { get; set; } = new List<TuristaCategorium>();
}
