using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class RutaExperiencium
{
    public int IdRutaExperiencia { get; set; }

    public int IdRuta { get; set; }

    public int IdExperiencia { get; set; }

    public int OrdenVisita { get; set; }

    public decimal DistanciaKm { get; set; }

    public virtual Experiencium IdExperienciaNavigation { get; set; } = null!;

    public virtual Rutum IdRutaNavigation { get; set; } = null!;
}
