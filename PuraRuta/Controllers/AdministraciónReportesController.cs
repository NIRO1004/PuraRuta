
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionReportesController : Controller
    {
        private readonly PuraRutaContext _context;

        public AdministracionReportesController(
            PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var encuestas = await _context.EncuestaSatisfaccions
                .AsNoTracking()
                .Include(e => e.IdReservaNavigation)
                    .ThenInclude(r => r.IdExperienciaNavigation)
                .Include(e => e.IdReservaNavigation)
                    .ThenInclude(r => r.IdTuristaNavigation)
                        .ThenInclude(t => t.IdTuristaNavigation)
                .OrderByDescending(e => e.FechaEncuesta)
                .ToListAsync();

            ViewBag.TotalEncuestas = encuestas.Count;

            ViewBag.CalificacionPromedio = encuestas.Any()
                ? encuestas.Average(e => e.Calificacion)
                : 0;

            ViewBag.TotalRecomendaciones =
                encuestas.Count(e => e.Recomendaria);

            ViewBag.PorcentajeRecomendacion = encuestas.Any()
                ? encuestas.Count(e => e.Recomendaria) * 100.0
                    / encuestas.Count
                : 0;

            var resumenExperiencias = encuestas
                .GroupBy(e => e.IdReservaNavigation
                    .IdExperienciaNavigation.Nombre)
                .Select(g => new
                {
                    Experiencia = g.Key,
                    CantidadEncuestas = g.Count(),
                    Promedio = g.Average(e => e.Calificacion),
                    Recomendaciones = g.Count(e => e.Recomendaria)
                })
                .OrderByDescending(g => g.Promedio)
                .ToList();

            ViewBag.ResumenExperiencias = resumenExperiencias;

            return View(encuestas);
        }
    }
}
