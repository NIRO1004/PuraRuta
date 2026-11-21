
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionReservasController : Controller
    {
        private readonly PuraRutaContext _context;

        public AdministracionReservasController(
            PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.IdTuristaNavigation)
                    .ThenInclude(t => t.IdTuristaNavigation)
                .Include(r => r.IdExperienciaNavigation)
                .Include(r => r.IdEstadoReservaNavigation)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id, int idEstado)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound();

            var estadoExiste = await _context.EstadoReservas
                .AnyAsync(e => e.IdEstadoReserva == idEstado);

            if (!estadoExiste)
            {
                TempData["MensajeReservaAdmin"] =
                    "El estado seleccionado no es válido.";

                return RedirectToAction(nameof(Index));
            }

            reserva.IdEstadoReserva = idEstado;

            await _context.SaveChangesAsync();

            TempData["MensajeReservaAdmin"] =
                "El estado de la reserva se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
