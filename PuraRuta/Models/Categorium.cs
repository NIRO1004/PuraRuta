using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Categorium
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string DescripcionCategoria { get; set; } = null!;

    public virtual ICollection<Experiencium> Experiencia { get; set; } = new List<Experiencium>();

    public virtual ICollection<TuristaCategorium> TuristaCategoria { get; set; } = new List<TuristaCategorium>();
}
