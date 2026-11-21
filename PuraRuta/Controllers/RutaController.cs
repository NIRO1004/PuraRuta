
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
    public class RutasController : Controller
    {
        private readonly PuraRutaContext _context;

        public RutasController(PuraRutaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var experiencias = await _context.Experiencia
                .AsNoTracking()
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdDireccionNavigation)
                .Include(e => e.FotoExperiencia)
                .Where(e => e.Estado && e.IdComunidadNavigation.Estado)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(experiencias);
        }


        [HttpGet]
        public async Task<IActionResult> MisRutas()
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var rutas = await _context.Ruta
                .AsNoTracking()
                .Where(r => r.IdTurista == idUsuario)
                .Include(r => r.RutaExperiencia)
                    .ThenInclude(re => re.IdExperienciaNavigation)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View(rutas);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RutaViewModel modelo)
        {
            // Validar cantidad de experiencias
            if (modelo.ExperienciasSeleccionadas == null ||
                modelo.ExperienciasSeleccionadas.Count < 2 ||
                modelo.ExperienciasSeleccionadas.Count > 5)
            {
                ModelState.AddModelError(
                    "ExperienciasSeleccionadas",
                    "Debes seleccionar entre 2 y 5 experiencias."
                );
            }

            if (!ModelState.IsValid)
            {
                var experienciasDisponibles = await _context.Experiencia
                    .AsNoTracking()
                    .Include(e => e.IdComunidadNavigation)
                    .Include(e => e.IdCategoriaNavigation)
                    .Include(e => e.IdDireccionNavigation)
                    .Include(e => e.FotoExperiencia)
                    .Where(e => e.Estado && e.IdComunidadNavigation.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();

                return View(experienciasDisponibles);
            }

            // Obtener el identificador del usuario autenticado
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // Buscar el turista y su ubicación registrada
            var turista = await _context.Turista
                .Include(t => t.IdDireccionNavigation)
                .FirstOrDefaultAsync(t => t.IdTurista == idUsuario);

            if (turista == null)
            {
                return NotFound("No se encontró el perfil del turista.");
            }

            // Obtener las experiencias seleccionadas
            var idsSeleccionados = modelo.ExperienciasSeleccionadas
                .Distinct()
                .ToList();

            if (idsSeleccionados.Count != modelo.ExperienciasSeleccionadas.Count)
            {
                ModelState.AddModelError(
                    "ExperienciasSeleccionadas",
                    "No puedes seleccionar experiencias repetidas."
                );
            }

            var experiencias = await _context.Experiencia
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.IdDireccionNavigation)
                .Where(e => idsSeleccionados.Contains(e.IdExperiencia)
                            && e.Estado
                            && e.IdComunidadNavigation.Estado)
                .ToListAsync();

            if (experiencias.Count != idsSeleccionados.Count)
            {
                ModelState.AddModelError(
                    "ExperienciasSeleccionadas",
                    "Una o más experiencias no están disponibles."
                );
            }

            if (!ModelState.IsValid)
            {
                var disponibles = await _context.Experiencia
                    .AsNoTracking()
                    .Include(e => e.IdComunidadNavigation)
                    .Include(e => e.IdCategoriaNavigation)
                    .Include(e => e.IdDireccionNavigation)
                    .Include(e => e.FotoExperiencia)
                    .Where(e => e.Estado && e.IdComunidadNavigation.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();

                return View(disponibles);
            }

            // Ordenar las experiencias desde la ubicación del turista
            double latitudActual = (double)turista.IdDireccionNavigation.Latitud;
            double longitudActual = (double)turista.IdDireccionNavigation.Longitud;

            var pendientes = experiencias.ToList();
            var ordenadas = new List<(Experiencium Experiencia, decimal Distancia)>();

            decimal distanciaTotal = 0;

            while (pendientes.Count > 0)
            {
                var siguiente = pendientes
                    .Select(e => new
                    {
                        Experiencia = e,
                        Distancia = CalcularDistancia(
                            latitudActual,
                            longitudActual,
                            (double)e.IdDireccionNavigation.Latitud,
                            (double)e.IdDireccionNavigation.Longitud
                        )
                    })
                    .OrderBy(x => x.Distancia)
                    .First();

                decimal distancia = Math.Round(
                    (decimal)siguiente.Distancia,
                    2
                );

                ordenadas.Add((siguiente.Experiencia, distancia));
                distanciaTotal += distancia;

                latitudActual = (double)siguiente.Experiencia.IdDireccionNavigation.Latitud;
                longitudActual = (double)siguiente.Experiencia.IdDireccionNavigation.Longitud;

                pendientes.Remove(siguiente.Experiencia);
            }

            // Crear la ruta
            var ruta = new Rutum
            {
                IdTurista = turista.IdTurista,
                Nombre = modelo.Nombre.Trim(),
                FechaCreacion = DateTime.Now,
                FechaVisita = modelo.FechaVisita!.Value,
                DistanciaTotalKm = distanciaTotal
            };

            _context.Ruta.Add(ruta);
            await _context.SaveChangesAsync();

            // Registrar las experiencias en el orden calculado
            for (int i = 0; i < ordenadas.Count; i++)
            {
                var punto = new RutaExperiencium
                {
                    IdRuta = ruta.IdRuta,
                    IdExperiencia = ordenadas[i].Experiencia.IdExperiencia,
                    OrdenVisita = i + 1,
                    DistanciaKm = ordenadas[i].Distancia
                };

                _context.RutaExperiencia.Add(punto);
            }

            await _context.SaveChangesAsync();

            TempData["MensajeRuta"] = "La ruta se creó correctamente.";

            return RedirectToAction("Crear");
        }


        private double CalcularDistancia(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double radioTierra = 6371;

            double diferenciaLatitud = GradosARadianes(lat2 - lat1);
            double diferenciaLongitud = GradosARadianes(lon2 - lon1);

            double a =
                Math.Sin(diferenciaLatitud / 2) *
                Math.Sin(diferenciaLatitud / 2) +
                Math.Cos(GradosARadianes(lat1)) *
                Math.Cos(GradosARadianes(lat2)) *
                Math.Sin(diferenciaLongitud / 2) *
                Math.Sin(diferenciaLongitud / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radioTierra * c;
        }

        private double GradosARadianes(double grados)
        {
            return grados * Math.PI / 180;
        }


    }
}
