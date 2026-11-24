
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;

namespace PuraRuta.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly PuraRutaContext _context;

        public CatalogoController(PuraRutaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? categoriaId,
            int? provinciaId,
            decimal? precioMinimo,
            decimal? precioMaximo)
        {
            var consulta = _context.Experiencia
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.FotoExperiencia)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .Where(e => e.Estado == true);

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.IdCategoria == categoriaId.Value);
            }

            if (provinciaId.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.IdDireccionNavigation
                     .IdDistritoNavigation
                     .IdCantonNavigation
                     .IdProvinciaNavigation.IdProvincia == provinciaId.Value);
            }

            if (precioMinimo.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.Precio >= precioMinimo.Value);
            }

            if (precioMaximo.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.Precio <= precioMaximo.Value);
            }

            var experiencias = await consulta
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categoria
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Provincias = await _context.Provincia
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.CategoriaId = categoriaId;
            ViewBag.ProvinciaId = provinciaId;
            ViewBag.PrecioMinimo = precioMinimo;
            ViewBag.PrecioMaximo = precioMaximo;

            return View(experiencias);
        }


        public async Task<IActionResult> Detalles(int id)
        {
            var experiencia = await _context.Experiencia
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.FotoExperiencia)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .FirstOrDefaultAsync(e => e.IdExperiencia == id && e.Estado);

            if (experiencia == null)
            {
                return NotFound();
            }

            return View(experiencia);
        }

    }
}
