
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.ViewModels;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DashboardAdminController : Controller
    {
        private readonly PuraRutaContext _context;

        public DashboardAdminController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? anio)
        {
            // Año seleccionado o año actual por defecto
            var anioSeleccionado = anio ?? DateTime.Today.Year;

            // Reservas agrupadas por mes del año seleccionado
            var reservasMensuales = await _context.Reservas
                .AsNoTracking()
                .Where(r => r.FechaReserva.Year == anioSeleccionado)
                .GroupBy(r => r.FechaReserva.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            // Crear los 12 meses, incluso si no tienen reservas
            var modelo = new DashboardAdminViewModel
            {
                ReservasMensuales = Enumerable.Range(1, 12)
                    .Select(m => new ReservasPorMesViewModel
                    {
                        Mes = new DateTime(anioSeleccionado, m, 1)
                            .ToString("MMM"),
                        CantidadReservas = reservasMensuales
                            .FirstOrDefault(r => r.Mes == m)?.Cantidad ?? 0
                    })
                    .ToList()
            };

            // Top 5 experiencias más reservadas
            modelo.TopExperiencias = await _context.Reservas
                .AsNoTracking()
                .GroupBy(r => r.IdExperienciaNavigation.Nombre)
                .Select(g => new TopExperienciaViewModel
                {
                    Nombre = g.Key,
                    CantidadReservas = g.Count()
                })
                .OrderByDescending(x => x.CantidadReservas)
                .ThenBy(x => x.Nombre)
                .Take(5)
                .ToListAsync();

            // Calificación promedio de las encuestas
            modelo.CalificacionPromedio = await _context.EncuestaSatisfaccions
                .AsNoTracking()
                .Select(e => (double?)e.Calificacion)
                .AverageAsync() ?? 0;

            // Enviar el año seleccionado a la vista
            ViewBag.AnioSeleccionado = anioSeleccionado;

            return View(modelo);
        }
    }
}
