using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Distrito
{
    public int IdDistrito { get; set; }

    public int IdCanton { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    public virtual Canton IdCantonNavigation { get; set; } = null!;
}
