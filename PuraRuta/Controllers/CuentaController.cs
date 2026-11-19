
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.Models;
using PuraRuta.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PuraRuta.Controllers
{
    public class CuentaController : Controller
    {
        private readonly PuraRutaContext _context;

        public CuentaController(PuraRutaContext context)
        {
            _context = context;
        }

        // Mostrar formulario de registro
        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            await CargarProvincias();
            return View();
        }

        // Registrar un nuevo turista
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroTuristaViewModel model)
        {
            // Validar fecha de nacimiento
            if (model.FechaNacimiento.HasValue &&
                model.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    "FechaNacimiento",
                    "La fecha de nacimiento no puede ser futura."
                );
            }

            // Comprobar si el correo ya existe
            if (!string.IsNullOrWhiteSpace(model.Correo))
            {
                bool correoExiste = await _context.Usuarios
                    .AnyAsync(u => u.Correo == model.Correo);

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        "Correo",
                        "Este correo electrónico ya está registrado."
                    );
                }
            }

            // Validar relación entre provincia, cantón y distrito
            if (model.IdProvincia.HasValue &&
                model.IdCanton.HasValue &&
                model.IdDistrito.HasValue)
            {
                bool cantonValido = await _context.Cantons.AnyAsync(c =>
                    c.IdCanton == model.IdCanton.Value &&
                    c.IdProvincia == model.IdProvincia.Value
                );

                bool distritoValido = await _context.Distritos.AnyAsync(d =>
                    d.IdDistrito == model.IdDistrito.Value &&
                    d.IdCanton == model.IdCanton.Value
                );

                if (!cantonValido || !distritoValido)
                {
                    ModelState.AddModelError(
                        "IdDistrito",
                        "La ubicación seleccionada no es válida."
                    );
                }
            }

            // Si hay errores, regresar al formulario
            if (!ModelState.IsValid)
            {
                await CargarProvincias();
                return View(model);
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Crear usuario
                var usuario = new Usuario
                {
                    IdRol = 2,
                    Nombre = model.Nombre.Trim(),
                    Apellido1 = model.Apellido1.Trim(),
                    Apellido2 = model.Apellido2.Trim(),
                    Correo = model.Correo.Trim(),
                    Telefono = model.Telefono.Trim(),
                    Estado = true
                };

                // Generar hash seguro de la contraseña
                var passwordHasher = new PasswordHasher<Usuario>();

                usuario.ContrasenaHash = passwordHasher.HashPassword(
                    usuario,
                    model.Contrasena
                );

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();

                // 2. Crear dirección
                var direccion = new Direccion
                {
                    IdDistrito = model.IdDistrito!.Value,
                    DireccionExacta = model.DireccionExacta.Trim(),
                    Latitud = model.Latitud!.Value,
                    Longitud = model.Longitud!.Value
                };

                _context.Direccions.Add(direccion);

                await _context.SaveChangesAsync();

                // 3. Crear registro de turista
                var turista = new Turistum
                {
                    IdTurista = usuario.IdUsuario,
                    FechaNacimiento = model.FechaNacimiento!.Value,
                    IdDireccion = direccion.IdDireccion,
                    TieneVehiculo = model.TieneVehiculo
                };

                _context.Turista.Add(turista);

                await _context.SaveChangesAsync();

                // Confirmar las tres operaciones
                await transaccion.CommitAsync();

                TempData["RegistroExitoso"] =
                    "¡Tu cuenta se creó correctamente! Ya puedes iniciar sesión.";

                return RedirectToAction(nameof(Registro));
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible guardar el registro. Verifica los datos e inténtalo nuevamente."
                );

                await CargarProvincias();
                return View(model);
            }
        }

        // Mostrar formulario de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    u.Correo == model.Correo && u.Estado);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View(model);
            }

            var passwordHasher = new PasswordHasher<Usuario>();

            var resultado = passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.ContrasenaHash,
                model.Contrasena
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
        new Claim(ClaimTypes.Name, usuario.Nombre),
        new Claim(ClaimTypes.Email, usuario.Correo),
        new Claim(ClaimTypes.Role, usuario.IdRolNavigation.Nombre)
    };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // Obtener cantones según la provincia
        [HttpGet]
        public async Task<IActionResult> ObtenerCantones(int provinciaId)
        {
            var cantones = await _context.Cantons
                .Where(c => c.IdProvincia == provinciaId)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.IdCanton,
                    nombre = c.Nombre
                })
                .ToListAsync();

            return Json(cantones);
        }

        // Obtener distritos según el cantón
        [HttpGet]
        public async Task<IActionResult> ObtenerDistritos(int cantonId)
        {
            var distritos = await _context.Distritos
                .Where(d => d.IdCanton == cantonId)
                .OrderBy(d => d.Nombre)
                .Select(d => new
                {
                    id = d.IdDistrito,
                    nombre = d.Nombre
                })
                .ToListAsync();

            return Json(distritos);
        }

        // Cargar provincias
        private async Task CargarProvincias()
        {
            ViewBag.Provincias = await _context.Provincia
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
}
