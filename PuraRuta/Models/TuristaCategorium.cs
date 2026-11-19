using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class TuristaCategorium
{
    public int IdTuristaCategoria { get; set; }

    public int IdTurista { get; set; }

    public int IdCategoria { get; set; }

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Turistum IdTuristaNavigation { get; set; } = null!;
}
