
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.Models;
using PuraRuta.ViewModels;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Turista")]
    public class EncuestasController : Controller
    {
        private readonly PuraRutaContext _context;

        public EncuestasController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var reserva = await _context.Reservas
                .Include(r => r.IdTuristaNavigation)
                .Include(r => r.IdExperienciaNavigation)
                .Include(r => r.EncuestaSatisfaccion)
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == id &&
                    r.IdTuristaNavigation.IdTuristaNavigation.IdUsuario == idUsuario &&
                    r.IdEstadoReserva == 3
                );

            if (reserva == null)
                return NotFound();

            if (reserva.EncuestaSatisfaccion != null)
            {
                TempData["MensajeEncuesta"] =
                    "Esta reserva ya tiene una encuesta registrada.";

                return RedirectToAction(nameof(MisEncuestas));
            }

            ViewBag.Experiencia = reserva.IdExperienciaNavigation.Nombre;

            return View(new EncuestaSatisfaccionViewModel
            {
                IdReserva = reserva.IdReserva
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            EncuestaSatisfaccionViewModel modelo)
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var reserva = await _context.Reservas
                .Include(r => r.IdTuristaNavigation)
                .Include(r => r.EncuestaSatisfaccion)
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == modelo.IdReserva &&
                    r.IdTuristaNavigation.IdTuristaNavigation.IdUsuario == idUsuario &&
                    r.IdEstadoReserva == 3
                );

            if (reserva == null)
                return NotFound();

            if (reserva.EncuestaSatisfaccion != null)
            {
                TempData["MensajeEncuesta"] =
                    "Esta reserva ya tiene una encuesta registrada.";

                return RedirectToAction(nameof(MisEncuestas));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Experiencia =
                    (await _context.Experiencia
                        .FirstAsync(e => e.IdExperiencia == reserva.IdExperiencia))
                    .Nombre;

                return View(modelo);
            }

            var encuesta = new EncuestaSatisfaccion
            {
                IdReserva = reserva.IdReserva,
                Calificacion = modelo.Calificacion,
                Comentario = modelo.Comentario.Trim(),
                Recomendaria = modelo.Recomendaria,
                FechaEncuesta = DateTime.Now
            };

            _context.EncuestaSatisfaccions.Add(encuesta);

            await _context.SaveChangesAsync();

            TempData["MensajeEncuesta"] =
                "¡Gracias! Tu encuesta se registró correctamente.";

            return RedirectToAction(nameof(MisEncuestas));
        }

        [HttpGet]
        public async Task<IActionResult> MisEncuestas()
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var encuestas = await _context.EncuestaSatisfaccions
                .AsNoTracking()
                .Include(e => e.IdReservaNavigation)
                    .ThenInclude(r => r.IdExperienciaNavigation)
                .Where(e =>
                    e.IdReservaNavigation.IdTuristaNavigation.IdTuristaNavigation.IdUsuario
                    == idUsuario)
                .OrderByDescending(e => e.FechaEncuesta)
                .ToListAsync();

            return View(encuestas);
        }
    }
}
