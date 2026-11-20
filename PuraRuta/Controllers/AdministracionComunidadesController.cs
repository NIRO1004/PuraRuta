using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.Models;
using PuraRuta.ViewModels;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionComunidadesController : Controller
    {
        private readonly PuraRutaContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdministracionComunidadesController(
            PuraRutaContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var comunidades = await _context.Comunidads
                .Include(c => c.FotoComunidads)
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(comunidades);
        }
    }
}