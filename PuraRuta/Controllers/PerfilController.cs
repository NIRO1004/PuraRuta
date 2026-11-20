
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.Models;
using PuraRuta.ViewModels;
using System.Security.Claims;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Turista")]
    public class PerfilController : Controller
    {
        private readonly PuraRutaContext _context;

        public PerfilController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var turista = await _context.Turista
                .Include(t => t.IdTuristaNavigation)
                .Include(t => t.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                .Include(t => t.TuristaCategoria)
                .FirstOrDefaultAsync(t => t.IdTurista == idUsuario);

            if (turista == null)
                return NotFound();

            var direccion = turista.IdDireccionNavigation;
            var distrito = direccion.IdDistritoNavigation;
            var canton = distrito.IdCantonNavigation;

            var model = new PerfilViewModel
            {
                Nombre = turista.IdTuristaNavigation.Nombre,
                Apellido1 = turista.IdTuristaNavigation.Apellido1,
                Apellido2 = turista.IdTuristaNavigation.Apellido2,
                Correo = turista.IdTuristaNavigation.Correo,
                Telefono = turista.IdTuristaNavigation.Telefono,
                FechaNacimiento = turista.FechaNacimiento,
                IdProvincia = canton.IdProvincia,
                IdCanton = canton.IdCanton,
                IdDistrito = distrito.IdDistrito,
                DireccionExacta = direccion.DireccionExacta,
                Latitud = direccion.Latitud,
                Longitud = direccion.Longitud,
                TieneVehiculo = turista.TieneVehiculo,
                CategoriasSeleccionadas = turista.TuristaCategoria
                    .Select(tc => tc.IdCategoria)
                    .ToList()
            };

            await CargarDatos(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PerfilViewModel model)
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var usuario = await _context.Usuarios
                .Include(u => u.Turistum)
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuario && u.Estado);

            if (usuario?.Turistum == null)
                return NotFound();

            if (model.FechaNacimiento.HasValue &&
                model.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    "FechaNacimiento",
                    "La fecha de nacimiento no puede ser futura."
                );
            }

            bool correoExiste = await _context.Usuarios.AnyAsync(u =>
                u.Correo == model.Correo &&
                u.IdUsuario != idUsuario);

            if (correoExiste)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Este correo electrónico ya está registrado."
                );
            }

            bool cantonValido = model.IdProvincia.HasValue &&
                model.IdCanton.HasValue &&
                await _context.Cantons.AnyAsync(c =>
                    c.IdCanton == model.IdCanton.Value &&
                    c.IdProvincia == model.IdProvincia.Value);

            bool distritoValido = model.IdCanton.HasValue &&
                model.IdDistrito.HasValue &&
                await _context.Distritos.AnyAsync(d =>
                    d.IdDistrito == model.IdDistrito.Value &&
                    d.IdCanton == model.IdCanton.Value);

            if (!cantonValido || !distritoValido)
            {
                ModelState.AddModelError(
                    "IdDistrito",
                    "La ubicación seleccionada no es válida."
                );
            }

            if (model.CategoriasSeleccionadas == null)
                model.CategoriasSeleccionadas = new List<int>();

            var categoriasValidas = await _context.Categoria
                .Where(c => model.CategoriasSeleccionadas.Contains(c.IdCategoria))
                .Select(c => c.IdCategoria)
                .ToListAsync();

            if (categoriasValidas.Count != model.CategoriasSeleccionadas.Distinct().Count())
            {
                ModelState.AddModelError(
                    "CategoriasSeleccionadas",
                    "Una o más categorías seleccionadas no son válidas."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarDatos(model);
                return View(model);
            }

            var turista = usuario.Turistum;

            var direccion = await _context.Direccions
                .FirstOrDefaultAsync(d => d.IdDireccion == turista.IdDireccion);

            if (direccion == null)
                return NotFound();

            usuario.Nombre = model.Nombre.Trim();
            usuario.Apellido1 = model.Apellido1.Trim();
            usuario.Apellido2 = model.Apellido2.Trim();
            usuario.Correo = model.Correo.Trim();
            usuario.Telefono = model.Telefono.Trim();

            turista.FechaNacimiento = model.FechaNacimiento!.Value;
            turista.TieneVehiculo = model.TieneVehiculo;

            direccion.IdDistrito = model.IdDistrito!.Value;
            direccion.DireccionExacta = model.DireccionExacta.Trim();
            direccion.Latitud = model.Latitud!.Value;
            direccion.Longitud = model.Longitud!.Value;

            var interesesActuales = await _context.TuristaCategoria
                .Where(tc => tc.IdTurista == idUsuario)
                .ToListAsync();

            _context.TuristaCategoria.RemoveRange(interesesActuales);

            foreach (var idCategoria in categoriasValidas.Distinct())
            {
                _context.TuristaCategoria.Add(new TuristaCategorium
                {
                    IdTurista = idUsuario,
                    IdCategoria = idCategoria
                });
            }

            await _context.SaveChangesAsync();

            TempData["PerfilActualizado"] =
                "Tu perfil se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatos(PerfilViewModel model)
        {
            ViewBag.Provincias = await _context.Provincia
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categoria
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Cantones = model.IdProvincia.HasValue
                ? await _context.Cantons
                    .Where(c => c.IdProvincia == model.IdProvincia.Value)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync()
                : new List<Canton>();

            ViewBag.Distritos = model.IdCanton.HasValue
                ? await _context.Distritos
                    .Where(d => d.IdCanton == model.IdCanton.Value)
                    .OrderBy(d => d.Nombre)
                    .ToListAsync()
                : new List<Distrito>();
        }
    }
}
