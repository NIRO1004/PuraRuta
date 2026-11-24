using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class FotoExperiencium
{
    public int IdFotoExperiencia { get; set; }

    public int IdExperiencia { get; set; }

    public string RutaImagen { get; set; } = null!;

    public string? DescripcionFotoExperiencia { get; set; }

    public virtual Experiencium IdExperienciaNavigation { get; set; } = null!;
}
