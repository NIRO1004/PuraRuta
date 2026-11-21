
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Data;
using PuraRuta.Models;
using PuraRuta.ViewModels;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionExperienciasController : Controller
    {
        private readonly PuraRutaContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdministracionExperienciasController> _logger;

        public AdministracionExperienciasController(
            PuraRutaContext context,
            IWebHostEnvironment environment,
            ILogger<AdministracionExperienciasController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var experiencias = await _context.Experiencia
                .AsNoTracking()
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdDireccionNavigation)
                .Include(e => e.FotoExperiencia)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(experiencias);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarDatos();
            return View(new ExperienciaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ExperienciaViewModel model)
        {
            await ValidarRelaciones(model);

            var fotos = ObtenerFotografias(model);
            ValidarFotografias(fotos);

            if (!ModelState.IsValid)
            {
                await CargarDatos(
                    model.IdProvincia,
                    model.IdCanton);

                return View(model);
            }

            var archivosGuardados = new List<string>();

            await using var tx =
                await _context.Database.BeginTransactionAsync();

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

                var experiencia = new Experiencium
                {
                    IdComunidad = model.IdComunidad!.Value,
                    IdCategoria = model.IdCategoria!.Value,
                    IdDireccion = direccion.IdDireccion,
                    Nombre = model.Nombre.Trim(),
                    DescripcionExperiencia =
                        model.DescripcionExperiencia.Trim(),
                    Precio = model.Precio,
                    DuracionHoras = model.DuracionHoras,
                    EdadMinima = model.EdadMinima,
                    Dificultad = model.Dificultad,
                    CapacidadDiaria = model.CapacidadDiaria,
                    TransportePublico = model.TransportePublico,
                    Estado = true
                };

                _context.Experiencia.Add(experiencia);
                await _context.SaveChangesAsync();

                await GuardarFotografias(
                    experiencia.IdExperiencia,
                    fotos,
                    archivosGuardados);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ExperienciaCreada"] =
                    "La experiencia se registró correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                EliminarArchivos(archivosGuardados);

                _logger.LogError(
                    ex,
                    "Error al registrar la experiencia.");

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar la experiencia.");

                await CargarDatos(
                    model.IdProvincia,
                    model.IdCanton);

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var experiencia = await _context.Experiencia
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                .Include(e => e.FotoExperiencia)
                .FirstOrDefaultAsync(
                    e => e.IdExperiencia == id);

            if (experiencia == null)
                return NotFound();

            var direccion = experiencia.IdDireccionNavigation;
            var distrito = direccion.IdDistritoNavigation;
            var canton = distrito.IdCantonNavigation;

            var model = new ExperienciaViewModel
            {
                IdExperiencia = experiencia.IdExperiencia,
                Nombre = experiencia.Nombre,
                DescripcionExperiencia =
                    experiencia.DescripcionExperiencia,
                IdComunidad = experiencia.IdComunidad,
                IdCategoria = experiencia.IdCategoria,
                IdProvincia = canton.IdProvincia,
                IdCanton = canton.IdCanton,
                IdDistrito = distrito.IdDistrito,
                DireccionExacta = direccion.DireccionExacta,
                Latitud = direccion.Latitud,
                Longitud = direccion.Longitud,
                Precio = experiencia.Precio,
                DuracionHoras = experiencia.DuracionHoras,
                EdadMinima = experiencia.EdadMinima,
                Dificultad = experiencia.Dificultad,
                CapacidadDiaria = experiencia.CapacidadDiaria,
                TransportePublico = experiencia.TransportePublico,
                Estado = experiencia.Estado,
                FotografiasExistentes =
                    experiencia.FotoExperiencia
                        .Select(f => f.RutaImagen).ToList()
            };

            await CargarDatos(
                model.IdProvincia,
                model.IdCanton,
                model.IdComunidad,
                model.IdCategoria);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            ExperienciaViewModel model)
        {
            if (id != model.IdExperiencia)
                return BadRequest();

            var experiencia = await _context.Experiencia
                .Include(e => e.IdDireccionNavigation)
                .Include(e => e.FotoExperiencia)
                .FirstOrDefaultAsync(
                    e => e.IdExperiencia == id);

            if (experiencia == null)
                return NotFound();

            await ValidarRelaciones(model);

            var fotos = ObtenerFotografias(model);
            ValidarFotografias(fotos);

            if (!ModelState.IsValid)
            {
                model.FotografiasExistentes =
                    experiencia.FotoExperiencia
                        .Select(f => f.RutaImagen).ToList();

                await CargarDatos(
                    model.IdProvincia,
                    model.IdCanton,
                    model.IdComunidad,
                    model.IdCategoria);

                return View(model);
            }

            var archivosGuardados = new List<string>();

            await using var tx =
                await _context.Database.BeginTransactionAsync();

            try
            {
                experiencia.IdComunidad = model.IdComunidad!.Value;
                experiencia.IdCategoria = model.IdCategoria!.Value;
                experiencia.Nombre = model.Nombre.Trim();
                experiencia.DescripcionExperiencia =
                    model.DescripcionExperiencia.Trim();
                experiencia.Precio = model.Precio;
                experiencia.DuracionHoras = model.DuracionHoras;
                experiencia.EdadMinima = model.EdadMinima;
                experiencia.Dificultad = model.Dificultad;
                experiencia.CapacidadDiaria = model.CapacidadDiaria;
                experiencia.TransportePublico =
                    model.TransportePublico;

                experiencia.IdDireccionNavigation.IdDistrito =
                    model.IdDistrito!.Value;

                experiencia.IdDireccionNavigation.DireccionExacta =
                    model.DireccionExacta.Trim();

                experiencia.IdDireccionNavigation.Latitud =
                    model.Latitud!.Value;

                experiencia.IdDireccionNavigation.Longitud =
                    model.Longitud!.Value;

                await GuardarFotografias(
                    id,
                    fotos,
                    archivosGuardados);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ExperienciaActualizada"] =
                    "La experiencia se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                EliminarArchivos(archivosGuardados);

                _logger.LogError(
                    ex,
                    "Error al editar la experiencia {Id}.",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar la experiencia.");

                model.FotografiasExistentes =
                    experiencia.FotoExperiencia
                        .Select(f => f.RutaImagen).ToList();

                await CargarDatos(
                    model.IdProvincia,
                    model.IdCanton,
                    model.IdComunidad,
                    model.IdCategoria);

                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var experiencia = await _context.Experiencia
                .FirstOrDefaultAsync(e => e.IdExperiencia == id);

            if (experiencia == null)
                return NotFound();

            experiencia.Estado = !experiencia.Estado;

            await _context.SaveChangesAsync();

            TempData["EstadoExperiencia"] = experiencia.Estado
                ? "La experiencia se activó correctamente."
                : "La experiencia se desactivó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task ValidarRelaciones(
            ExperienciaViewModel model)
        {
            bool comunidadValida =
                model.IdComunidad.HasValue &&
                await _context.Comunidads.AnyAsync(
                    c => c.IdComunidad == model.IdComunidad.Value
                         && c.Estado);

            bool categoriaValida =
                model.IdCategoria.HasValue &&
                await _context.Categoria.AnyAsync(
                    c => c.IdCategoria == model.IdCategoria.Value);

            bool cantonValido =
                model.IdProvincia.HasValue &&
                model.IdCanton.HasValue &&
                await _context.Cantons.AnyAsync(
                    c => c.IdCanton == model.IdCanton.Value
                         && c.IdProvincia == model.IdProvincia.Value);

            bool distritoValido =
                model.IdCanton.HasValue &&
                model.IdDistrito.HasValue &&
                await _context.Distritos.AnyAsync(
                    d => d.IdDistrito == model.IdDistrito.Value
                         && d.IdCanton == model.IdCanton.Value);

            if (!comunidadValida)
                ModelState.AddModelError(
                    nameof(model.IdComunidad),
                    "Selecciona una comunidad activa.");

            if (!categoriaValida)
                ModelState.AddModelError(
                    nameof(model.IdCategoria),
                    "Selecciona una categoría válida.");

            if (!cantonValido || !distritoValido)
                ModelState.AddModelError(
                    nameof(model.IdDistrito),
                    "Selecciona una ubicación geográfica válida.");
        }

        private static List<IFormFile> ObtenerFotografias(
            ExperienciaViewModel model)
        {
            return (model.Fotografias ?? new List<IFormFile>())
                .Where(f => f != null && f.Length > 0)
                .ToList();
        }

        private void ValidarFotografias(List<IFormFile> fotos)
        {
            if (fotos.Count > 10)
                ModelState.AddModelError(
                    nameof(ExperienciaViewModel.Fotografias),
                    "Puedes cargar un máximo de 10 fotografías.");

            var permitidas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            foreach (var foto in fotos)
            {
                if (!permitidas.Contains(
                    Path.GetExtension(foto.FileName)))
                {
                    ModelState.AddModelError(
                        nameof(ExperienciaViewModel.Fotografias),
                        $"Formato no permitido: {foto.FileName}.");
                }

                if (foto.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(ExperienciaViewModel.Fotografias),
                        $"La fotografía {foto.FileName} supera 5 MB.");
                }
            }
        }

        private async Task GuardarFotografias(
            int idExperiencia,
            List<IFormFile> fotos,
            List<string> archivosGuardados)
        {
            if (fotos.Count == 0)
                return;

            var webRoot = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");

            var carpeta = Path.Combine(
                webRoot,
                "imagenes",
                "experiencias");

            Directory.CreateDirectory(carpeta);

            foreach (var foto in fotos)
            {
                var extension =
                    Path.GetExtension(foto.FileName).ToLowerInvariant();

                var nombre = $"{Guid.NewGuid():N}{extension}";
                var rutaFisica = Path.Combine(carpeta, nombre);
                var rutaWeb = $"/imagenes/experiencias/{nombre}";

                await using (var stream = new FileStream(
                    rutaFisica,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    await foto.CopyToAsync(stream);
                }

                archivosGuardados.Add(rutaFisica);

                var descripcion =
                    Path.GetFileNameWithoutExtension(foto.FileName);

                if (descripcion.Length > 150)
                    descripcion = descripcion[..150];

                _context.FotoExperiencia.Add(
                    new FotoExperiencium
                    {
                        IdExperiencia = idExperiencia,
                        RutaImagen = rutaWeb,
                        Descripcion = string.IsNullOrWhiteSpace(descripcion)
                            ? "Fotografía de la experiencia"
                            : descripcion
                    });
            }
        }

        private void EliminarArchivos(List<string> rutas)
        {
            foreach (var ruta in rutas)
            {
                try
                {
                    if (System.IO.File.Exists(ruta))
                        System.IO.File.Delete(ruta);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "No se pudo eliminar el archivo {Ruta}.",
                        ruta);
                }
            }
        }

        private async Task CargarDatos(
            int? provinciaId = null,
            int? cantonId = null,
            int? comunidadId = null,
            int? categoriaId = null)
        {
            ViewBag.Provincias = await _context.Provincia
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Cantones = provinciaId.HasValue
                ? await _context.Cantons
                    .AsNoTracking()
                    .Where(c => c.IdProvincia == provinciaId.Value)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync()
                : new List<Canton>();

            ViewBag.Distritos = cantonId.HasValue
                ? await _context.Distritos
                    .AsNoTracking()
                    .Where(d => d.IdCanton == cantonId.Value)
                    .OrderBy(d => d.Nombre)
                    .ToListAsync()
                : new List<Distrito>();

            ViewBag.Comunidades = await _context.Comunidads
                .AsNoTracking()
                .Where(c => c.Estado || c.IdComunidad == comunidadId)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categoria
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Dificultades = new List<string>
            {
                "Baja",
                "Media",
                "Alta"
            };
        }
    }
}
