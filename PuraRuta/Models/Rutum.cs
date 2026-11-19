using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Rutum
{
    public int IdRuta { get; set; }

    public int IdTurista { get; set; }

    public string Nombre { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateOnly FechaVisita { get; set; }

    public decimal DistanciaTotalKm { get; set; }

    public virtual Turistum IdTuristaNavigation { get; set; } = null!;

    public virtual ICollection<RutaExperiencium> RutaExperiencia { get; set; } = new List<RutaExperiencium>();
}
