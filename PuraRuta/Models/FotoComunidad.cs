using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class FotoComunidad
{
    public int IdFotoComunidad { get; set; }

    public int IdComunidad { get; set; }

    public string RutaImagen { get; set; } = null!;

    public string? DescripcionFotoComunidad { get; set; }

    public virtual Comunidad IdComunidadNavigation { get; set; } = null!;
    
}
