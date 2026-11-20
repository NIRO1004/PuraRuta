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
        private readonly ILogger<AdministracionComunidadesController> _logger;

        public AdministracionComunidadesController(PuraRutaContext context, IWebHostEnvironment environment,
            ILogger<AdministracionComunidadesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comunidades = await _context.Comunidads.AsNoTracking()
                .Include(c => c.FotoComunidads)
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .OrderBy(c => c.Nombre).ToListAsync();
            return View(comunidades);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarDatos();
            return View(new ComunidadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ComunidadViewModel model)
        {
            await ValidarUbicacion(model);
            var fotos = ObtenerFotografias(model);
            ValidarFotografias(fotos);

            if (!ModelState.IsValid)
            {
                await CargarDatos(model.IdProvincia, model.IdCanton);
                return View(model);
            }

            var archivosGuardados = new List<string>();
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var direccion = new Direccion
                {
                    IdDistrito = model.IdDistrito!.Value,
                    DireccionExacta = model.DireccionExacta.Trim(),
                    Latitud = model.Latitud!.Value,
                    Longitud = model.Longitud!.Value
                };
                _context.Direccions.Add(direccion);
                await _context.SaveChangesAsync();

                var comunidad = new Comunidad
                {
                    IdDireccion = direccion.IdDireccion,
                    Nombre = model.Nombre.Trim(),
                    DescripcionComunidad = model.DescripcionComunidad.Trim(),
                    Estado = true
                };
                _context.Comunidads.Add(comunidad);
                await _context.SaveChangesAsync();

                await GuardarFotografias(comunidad.IdComunidad, fotos, archivosGuardados);
                await tx.CommitAsync();
                TempData["ComunidadCreada"] = "La comunidad se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                EliminarArchivos(archivosGuardados);
                _logger.LogError(ex, "Error al registrar la comunidad.");
                ModelState.AddModelError(string.Empty, "No se pudo registrar la comunidad. Revisa los datos e inténtalo nuevamente.");
                await CargarDatos(model.IdProvincia, model.IdCanton);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var comunidad = await _context.Comunidads
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                .Include(c => c.FotoComunidads)
                .FirstOrDefaultAsync(c => c.IdComunidad == id);

            if (comunidad == null) return NotFound();

            var direccion = comunidad.IdDireccionNavigation;
            var distrito = direccion.IdDistritoNavigation;
            var canton = distrito.IdCantonNavigation;
            var model = new ComunidadViewModel
            {
                IdComunidad = comunidad.IdComunidad,
                Nombre = comunidad.Nombre,
                DescripcionComunidad = comunidad.DescripcionComunidad,
                Estado = comunidad.Estado,
                IdProvincia = canton.IdProvincia,
                IdCanton = canton.IdCanton,
                IdDistrito = distrito.IdDistrito,
                DireccionExacta = direccion.DireccionExacta,
                Latitud = direccion.Latitud,
                Longitud = direccion.Longitud,
                FotografiasExistentes = comunidad.FotoComunidads.Select(f => f.RutaImagen).ToList()
            };

            await CargarDatos(model.IdProvincia, model.IdCanton);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ComunidadViewModel model)
        {
            if (id != model.IdComunidad) return BadRequest();

            var comunidad = await _context.Comunidads
                .Include(c => c.IdDireccionNavigation)
                .Include(c => c.FotoComunidads)
                .FirstOrDefaultAsync(c => c.IdComunidad == id);
            if (comunidad == null) return NotFound();

            await ValidarUbicacion(model);
            var fotos = ObtenerFotografias(model);
            ValidarFotografias(fotos);

            if (!ModelState.IsValid)
            {
                model.FotografiasExistentes = comunidad.FotoComunidads.Select(f => f.RutaImagen).ToList();
                await CargarDatos(model.IdProvincia, model.IdCanton);
                return View(model);
            }

            var archivosGuardados = new List<string>();
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                comunidad.Nombre = model.Nombre.Trim();
                comunidad.DescripcionComunidad = model.DescripcionComunidad.Trim();
                comunidad.IdDireccionNavigation.IdDistrito = model.IdDistrito!.Value;
                comunidad.IdDireccionNavigation.DireccionExacta = model.DireccionExacta.Trim();
                comunidad.IdDireccionNavigation.Latitud = model.Latitud!.Value;
                comunidad.IdDireccionNavigation.Longitud = model.Longitud!.Value;

                await GuardarFotografias(id, fotos, archivosGuardados);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                TempData["ComunidadActualizada"] = "La comunidad se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                EliminarArchivos(archivosGuardados);
                _logger.LogError(ex, "Error al editar la comunidad {IdComunidad}.", id);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar la comunidad. Revisa los datos e inténtalo nuevamente.");
                model.FotografiasExistentes = comunidad.FotoComunidads.Select(f => f.RutaImagen).ToList();
                await CargarDatos(model.IdProvincia, model.IdCanton);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var comunidad = await _context.Comunidads.FirstOrDefaultAsync(c => c.IdComunidad == id);
            if (comunidad == null) return NotFound();

            comunidad.Estado = !comunidad.Estado;
            await _context.SaveChangesAsync();
            TempData["EstadoComunidad"] = comunidad.Estado
                ? "La comunidad se activó correctamente."
                : "La comunidad se desactivó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidarUbicacion(ComunidadViewModel model)
        {
            bool cantonValido = model.IdProvincia.HasValue && model.IdCanton.HasValue &&
                await _context.Cantons.AnyAsync(c => c.IdCanton == model.IdCanton.Value && c.IdProvincia == model.IdProvincia.Value);
            bool distritoValido = model.IdCanton.HasValue && model.IdDistrito.HasValue &&
                await _context.Distritos.AnyAsync(d => d.IdDistrito == model.IdDistrito.Value && d.IdCanton == model.IdCanton.Value);
            if (!cantonValido || !distritoValido)
                ModelState.AddModelError(nameof(model.IdDistrito), "Selecciona una provincia, cantón y distrito relacionados correctamente.");
        }

        private static List<IFormFile> ObtenerFotografias(ComunidadViewModel model) =>
            (model.Fotografias ?? new List<IFormFile>()).Where(f => f != null && f.Length > 0).ToList();

        private void ValidarFotografias(List<IFormFile> fotos)
        {
            if (fotos.Count > 10) ModelState.AddModelError(nameof(ComunidadViewModel.Fotografias), "Puedes cargar un máximo de 10 fotografías por operación.");
            var permitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
            foreach (var foto in fotos)
            {
                if (!permitidas.Contains(Path.GetExtension(foto.FileName)))
                    ModelState.AddModelError(nameof(ComunidadViewModel.Fotografias), $"Formato no permitido: {foto.FileName}.");
                if (foto.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError(nameof(ComunidadViewModel.Fotografias), $"La fotografía {foto.FileName} supera 5 MB.");
            }
        }

        private async Task GuardarFotografias(int idComunidad, List<IFormFile> fotos, List<string> archivosGuardados)
        {
            if (fotos.Count == 0) return;
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            var carpeta = Path.Combine(webRoot, "imagenes", "comunidades");
            Directory.CreateDirectory(carpeta);

            foreach (var foto in fotos)
            {
                var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                var nombre = $"{Guid.NewGuid():N}{extension}";
                var rutaFisica = Path.Combine(carpeta, nombre);
                var rutaWeb = $"/imagenes/comunidades/{nombre}";
                await using (var stream = new FileStream(rutaFisica, FileMode.CreateNew, FileAccess.Write))
                    await foto.CopyToAsync(stream);
                archivosGuardados.Add(rutaFisica);

                var descripcion = Path.GetFileNameWithoutExtension(foto.FileName);
                if (descripcion.Length > 150) descripcion = descripcion[..150];
                _context.FotoComunidads.Add(new FotoComunidad
                {
                    IdComunidad = idComunidad,
                    RutaImagen = rutaWeb,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? "Fotografía de la comunidad" : descripcion
                });
            }
        }

        private void EliminarArchivos(List<string> rutas)
        {
            foreach (var ruta in rutas)
            {
                try { if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta); }
                catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar el archivo {Ruta}.", ruta); }
            }
        }

        private async Task CargarDatos(int? provinciaId = null, int? cantonId = null)
        {
            ViewBag.Provincias = await _context.Provincia.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Cantones = provinciaId.HasValue
                ? await _context.Cantons.AsNoTracking().Where(c => c.IdProvincia == provinciaId.Value).OrderBy(c => c.Nombre).ToListAsync()
                : new List<Canton>();
            ViewBag.Distritos = cantonId.HasValue
                ? await _context.Distritos.AsNoTracking().Where(d => d.IdCanton == cantonId.Value).OrderBy(d => d.Nombre).ToListAsync()
                : new List<Distrito>();
        }
    }
}
