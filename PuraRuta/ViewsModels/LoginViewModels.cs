
using System.ComponentModel.DataAnnotations;

namespace PuraRuta.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa tu contraseña.")]
        public string Contrasena { get; set; } = string.Empty;
    }
}
