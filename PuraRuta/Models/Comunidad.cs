using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Comunidad
{
    public int IdComunidad { get; set; }

    public int IdDireccion { get; set; }

    public string Nombre { get; set; } = null!;

    public string DescripcionComunidad { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<Experiencium> Experiencia { get; set; } = new List<Experiencium>();

    public virtual ICollection<FotoComunidad> FotoComunidads { get; set; } = new List<FotoComunidad>();

    public virtual Direccion IdDireccionNavigation { get; set; } = null!;
}
