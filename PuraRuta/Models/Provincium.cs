using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Provincium
{
    public int IdProvincia { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Canton> Cantons { get; set; } = new List<Canton>();
}
