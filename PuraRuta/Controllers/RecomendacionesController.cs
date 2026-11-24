
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PuraRuta.Data;

namespace PuraRuta.Controllers
{
    [Authorize(Roles = "Turista")]
    public class RecomendacionesController : Controller
    {
        private readonly PuraRutaContext _context;

        public RecomendacionesController(PuraRutaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var turista = await _context.Turista
                .Include(t => t.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .Include(t => t.TuristaCategoria)
                .FirstOrDefaultAsync(t => t.IdTurista == idUsuario);

            if (turista == null)
            {
                return NotFound("No se encontró el perfil del turista.");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            int edad = hoy.Year - turista.FechaNacimiento.Year;

            if (turista.FechaNacimiento > hoy.AddYears(-edad))
            {
                edad--;
            }

            var categoriasInteres = turista.TuristaCategoria
                .Select(tc => tc.IdCategoria)
                .ToList();

            var provinciaTurista = turista.IdDireccionNavigation
                .IdDistritoNavigation
                .IdCantonNavigation
                .IdProvincia;

            var experiencias = await _context.Experiencia
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdComunidadNavigation)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdDistritoNavigation)
                        .ThenInclude(d => d.IdCantonNavigation)
                            .ThenInclude(c => c.IdProvinciaNavigation)
                .Include(e => e.FotoExperiencia)
                .Where(e => e.Estado && e.EdadMinima <= edad)
                .ToListAsync();

            var recomendaciones = experiencias.Select(e =>
            {
                int puntuacion = 0;
                var motivos = new List<string>();

                if (categoriasInteres.Contains(e.IdCategoria))
                {
                    puntuacion += 40;
                    motivos.Add("Coincide con tus intereses.");
                }

                var provinciaExperiencia = e.IdDireccionNavigation
                    .IdDistritoNavigation
                    .IdCantonNavigation
                    .IdProvincia;

                if (provinciaExperiencia == provinciaTurista)
                {
                    puntuacion += 20;
                    motivos.Add("Se encuentra en tu provincia.");
                }

                double distancia = CalcularDistancia(
                    (double)turista.IdDireccionNavigation.Latitud,
                    (double)turista.IdDireccionNavigation.Longitud,
                    (double)e.IdDireccionNavigation.Latitud,
                    (double)e.IdDireccionNavigation.Longitud
                );

                if (distancia <= 25)
                {
                    puntuacion += 15;
                    motivos.Add("Está a menos de 25 km de tu ubicación.");
                }

                if (turista.TieneVehiculo)
                {
                    puntuacion += 10;
                    motivos.Add("Puedes llegar utilizando tu vehículo.");
                }
                else if (e.TransportePublico)
                {
                    puntuacion += 15;
                    motivos.Add("Cuenta con acceso mediante transporte público.");
                }

                return new
                {
                    Experiencia = e,
                    Puntuacion = puntuacion,
                    Distancia = Math.Round(distancia, 1),
                    Motivos = motivos
                };
            })
            .OrderByDescending(r => r.Puntuacion)
            .ThenBy(r => r.Distancia)
            .Take(5)
            .ToList();

            return View(recomendaciones);
        }

        private static double CalcularDistancia(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double radioTierra = 6371;

            double dLat = GradosARadianes(lat2 - lat1);
            double dLon = GradosARadianes(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(GradosARadianes(lat1)) *
                Math.Cos(GradosARadianes(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radioTierra * c;
        }

        private static double GradosARadianes(double grados)
        {
            return grados * Math.PI / 180;
        }
    }
}
