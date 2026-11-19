using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Direccion
{
    public int IdDireccion { get; set; }

    public int IdDistrito { get; set; }

    public string DireccionExacta { get; set; } = null!;

    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }

    public virtual ICollection<Comunidad> Comunidads { get; set; } = new List<Comunidad>();

    public virtual ICollection<Experiencium> Experiencia { get; set; } = new List<Experiencium>();

    public virtual Distrito IdDistritoNavigation { get; set; } = null!;

    public virtual ICollection<Turistum> Turista { get; set; } = new List<Turistum>();
}
