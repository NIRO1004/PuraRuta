
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
    public class ReservasController : Controller
    {
        private readonly PuraRutaContext _context;

        public ReservasController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int id)
        {
            var experiencia = await _context.Experiencia
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.IdExperiencia == id &&
                    e.Estado &&
                    e.IdComunidadNavigation.Estado);

            if (experiencia == null)
            {
                return NotFound();
            }

            ViewBag.Experiencia = experiencia;

            var modelo = new ReservaViewModel
            {
                IdExperiencia = experiencia.IdExperiencia,
                FechaVisita = DateOnly.FromDateTime(DateTime.Today),
                CantidadPersonas = 1
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> MisReservas()
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var reservas = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.IdExperienciaNavigation)
                .Include(r => r.IdEstadoReservaNavigation)
                .Where(r => r.IdTurista == idUsuario)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == id &&
                    r.IdTurista == idUsuario
                );

            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.IdEstadoReserva != 1)
            {
                TempData["MensajeReserva"] =
                    "Esta reserva no se puede cancelar porque no está confirmada.";

                return RedirectToAction(nameof(MisReservas));
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (reserva.FechaVisita < hoy.AddDays(7))
            {
                TempData["MensajeReserva"] =
                    "Solo puedes cancelar reservas con al menos 7 días de anticipación.";

                return RedirectToAction(nameof(MisReservas));
            }

            reserva.IdEstadoReserva = 2;

            await _context.SaveChangesAsync();

            TempData["MensajeReserva"] =
                "La reserva se canceló correctamente.";

            return RedirectToAction(nameof(MisReservas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReservaViewModel modelo)
        {

            Console.WriteLine("Cantidad de personas: " + modelo.CantidadPersonas);

            if (modelo.EdadesAcompanantes != null)
            {
                foreach (var edadAcompanante in modelo.EdadesAcompanantes)
                {
                    Console.WriteLine("Edad acompañante: " + edadAcompanante);
                }
            }

            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var turista = await _context.Turista
                .Include(t => t.IdTuristaNavigation)
                .FirstOrDefaultAsync(t => t.IdTurista == idUsuario);

            if (turista == null)
            {
                return NotFound("No se encontró el perfil del turista.");
            }

            var experiencia = await _context.Experiencia
                .Include(e => e.IdComunidadNavigation)
                .FirstOrDefaultAsync(e =>
                    e.IdExperiencia == modelo.IdExperiencia &&
                    e.Estado &&
                    e.IdComunidadNavigation.Estado);

            if (experiencia == null)
            {
                return NotFound("La experiencia no está disponible.");
            }

            ViewBag.Experiencia = experiencia;

            if (modelo.FechaVisita.HasValue &&
                modelo.FechaVisita.Value < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    "FechaVisita",
                    "No puedes reservar fechas anteriores a hoy."
                );
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var edad = hoy.Year - turista.FechaNacimiento.Year;

            if (hoy < turista.FechaNacimiento.AddYears(edad))
            {
                edad--;
            }

            if (edad < experiencia.EdadMinima)
            {
                ModelState.AddModelError(
                    "",
                    $"Debes tener al menos {experiencia.EdadMinima} años para reservar esta experiencia."
                );
            }


            int cantidadAcompanantes = modelo.CantidadPersonas - 1;

            if (modelo.EdadesAcompanantes == null ||
                modelo.EdadesAcompanantes.Count != cantidadAcompanantes)
            {
                ModelState.AddModelError(
                    "",
                    "Debes indicar la edad de todos los acompañantes."
                );
            }
            else
            {
                for (int i = 0; i < cantidadAcompanantes; i++)
                {
                    int edadAcompanante = modelo.EdadesAcompanantes[i];

                    if (edadAcompanante < 1 ||
                        edadAcompanante > 120 ||
                        edadAcompanante < experiencia.EdadMinima)
                    {
                        ModelState.AddModelError(
                            "",
                            $"El acompañante {i + 1} debe tener entre {experiencia.EdadMinima} y 120 años."
                        );
                    }
                }
            }


            if (modelo.FechaVisita.HasValue &&
                modelo.FechaVisita.Value >= DateOnly.FromDateTime(DateTime.Today) &&
                modelo.CantidadPersonas >= 1 &&
                modelo.CantidadPersonas <= 20)
            {
                var personasReservadas = await _context.Reservas
                    .Where(r =>
                        r.IdExperiencia == experiencia.IdExperiencia &&
                        r.FechaVisita == modelo.FechaVisita.Value &&
                        r.IdEstadoReserva == 1)
                    .SumAsync(r => (int?)r.CantidadPersonas) ?? 0;

                var espaciosDisponibles =
                    experiencia.CapacidadDiaria - personasReservadas;

                if (modelo.CantidadPersonas > espaciosDisponibles)
                {
                    ModelState.AddModelError(
                        "CantidadPersonas",
                        $"Solo quedan {Math.Max(0, espaciosDisponibles)} espacios disponibles."
                    );
                }
            }

            if (modelo.EdadesAcompanantes != null &&
                 modelo.EdadesAcompanantes.Any(e =>
                    e < experiencia.EdadMinima || e > 120))
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más acompañantes no cumplen con la edad mínima."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var precioTotal =
                experiencia.Precio * modelo.CantidadPersonas;

            var reserva = new Reserva
            {
                IdTurista = turista.IdTurista,
                IdExperiencia = experiencia.IdExperiencia,
                IdEstadoReserva = 1,
                FechaReserva = DateTime.Now,
                FechaVisita = modelo.FechaVisita!.Value,
                CantidadPersonas = modelo.CantidadPersonas,
                PrecioTotal = precioTotal
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["MensajeReserva"] =
                "¡Tu reserva se registró correctamente!";

            return RedirectToAction(
                "Detalles",
                "Catalogo",
                new { id = experiencia.IdExperiencia }
            );
        }
    }
}
