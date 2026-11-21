
using System.Collections.Generic;

namespace PuraRuta.ViewModels
{
    public class DashboardAdminViewModel
    {
        public double CalificacionPromedio { get; set; }

        public List<ReservasPorMesViewModel> ReservasMensuales { get; set; }
            = new();

        public List<TopExperienciaViewModel> TopExperiencias { get; set; }
            = new();
    }

    public class ReservasPorMesViewModel
    {
        public string Mes { get; set; } = string.Empty;

        public int CantidadReservas { get; set; }
    }

    public class TopExperienciaViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public int CantidadReservas { get; set; }
    }
}
