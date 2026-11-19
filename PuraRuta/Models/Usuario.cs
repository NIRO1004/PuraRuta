using System;
using System.Collections.Generic;

namespace PuraRuta.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido1 { get; set; } = null!;

    public string Apellido2 { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual Turistum? Turistum { get; set; }
}
