
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;

namespace PuraRuta.Controllers
{
    public class MapaController : Controller
    {
        private readonly PuraRutaContext _context;

        public MapaController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var experiencias = await _context.Experiencia
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdDireccionNavigation)
                .Include(e => e.FotoExperiencia)
                .Where(e => e.Estado == true)
                .ToListAsync();

            return View(experiencias);
        }
    }
}
