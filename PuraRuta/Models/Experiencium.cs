using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Experiencium
{
    public int IdExperiencia { get; set; }

    public int IdComunidad { get; set; }

    public int IdCategoria { get; set; }

    public int IdDireccion { get; set; }

    public string Nombre { get; set; } = null!;

    public string DescripcionExperiencia { get; set; } = null!;

    public decimal Precio { get; set; }

    public decimal DuracionHoras { get; set; }

    public int EdadMinima { get; set; }

    public string Dificultad { get; set; } = null!;

    public int CapacidadDiaria { get; set; }

    public bool TransportePublico { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<FotoExperiencium> FotoExperiencia { get; set; } = new List<FotoExperiencium>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Comunidad IdComunidadNavigation { get; set; } = null!;

    public virtual Direccion IdDireccionNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<RutaExperiencium> RutaExperiencia { get; set; } = new List<RutaExperiencium>();
}
