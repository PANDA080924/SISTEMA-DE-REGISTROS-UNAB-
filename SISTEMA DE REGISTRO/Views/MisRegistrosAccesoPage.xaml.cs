using Supabase;
using SISTEMA_DE_REGISTRO.Models;
using SISTEMA_DE_REGISTRO.Services;

namespace SISTEMA_DE_REGISTRO.Views
{
    public partial class MisRegistrosAccesoPage : ContentPage
    {
        private readonly Client _supabaseClient;
        private List<RegistroAcceso> _todosLosRegistros = new();
        private bool _filtrandoPorFecha = true;

        public MisRegistrosAccesoPage(Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
            dpFechaFiltro.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarHistorial();
        }

        private async Task CargarHistorial()
        {
            if (SesionActual.Usuario == null) return;

            rvRegistros.IsRefreshing = true;

            try
            {
                var respuesta = await _supabaseClient
                    .From<RegistroAcceso>()
                    .Where(x => x.EstudianteId == SesionActual.Usuario.Id)
                    .Order(x => x.CreadoAt, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                _todosLosRegistros = respuesta.Models;

                DateTime? fecha = _filtrandoPorFecha ? (dpFechaFiltro.Date ?? DateTime.Today) : null;
                AplicarFiltro(fecha);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo obtener el historial: {ex.Message}", "OK");
            }
            finally
            {
                rvRegistros.IsRefreshing = false;
            }
        }

        private void AplicarFiltro(DateTime? fechaSeleccionada)
        {
            IEnumerable<RegistroAcceso> filtrados = _todosLosRegistros;

            if (fechaSeleccionada.HasValue)
            {
                filtrados = _todosLosRegistros
                    .Where(r => r.CreadoAt.ToLocalTime().Date == fechaSeleccionada.Value.Date);
            }

            var listaFormateada = filtrados.Select(r =>
            {
                DateTime fechaLocal = r.CreadoAt.ToLocalTime();
                bool esEntrada = r.TipoEvento.Equals("ENTRADA", StringComparison.OrdinalIgnoreCase);

                return new MovimientoHistorialItem
                {
                    TituloEvento = esEntrada ? "ENTRADA REGISTRADA" : "SALIDA REGISTRADA",
                    TipoEventoColor = esEntrada ? Color.FromArgb("#16A34A") : Color.FromArgb("#DC2626"),
                    TipoEventoIcono = esEntrada ? "IN" : "OUT",
                    EscaneadoPor = string.IsNullOrWhiteSpace(r.EscaneadoPor) ? "No especificada" : r.EscaneadoPor,
                    EquipoDetalle = $"{r.Equipo} (S/N: {r.NumeroSerie})",
                    HoraFormateada = fechaLocal.ToString("hh:mm:ss tt"),
                    FechaFormateada = fechaLocal.ToString("dd/MM/yyyy")
                };
            }).ToList();

            cvHistorial.ItemsSource = listaFormateada;

            int entradas = listaFormateada.Count(x => x.TipoEventoIcono == "IN");
            int salidas = listaFormateada.Count(x => x.TipoEventoIcono == "OUT");

            string etiquetaPeriodo = fechaSeleccionada.HasValue
                ? $"Fecha {fechaSeleccionada.Value:dd/MM/yyyy}"
                : "Historial completo";

            lblResumenMovimientos.Text = $"{etiquetaPeriodo}: {entradas} Entrada(s) · {salidas} Salida(s)";
        }

        private void OnFechaFiltroChanged(object? sender, DateChangedEventArgs e)
        {
            _filtrandoPorFecha = true;
            AplicarFiltro(e.NewDate);
        }

        private void OnFiltrarHoyClicked(object? sender, EventArgs e)
        {
            _filtrandoPorFecha = true;
            dpFechaFiltro.Date = DateTime.Today;
            AplicarFiltro(DateTime.Today);
        }

        private void OnVerTodosClicked(object? sender, EventArgs e)
        {
            _filtrandoPorFecha = false;
            AplicarFiltro(null);
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await CargarHistorial();
        }
    }

    public class MovimientoHistorialItem
    {
        public string TituloEvento { get; set; } = string.Empty;
        public string TipoEventoIcono { get; set; } = string.Empty;
        public Color TipoEventoColor { get; set; } = Colors.Gray;
        public string EscaneadoPor { get; set; } = string.Empty;
        public string EquipoDetalle { get; set; } = string.Empty;
        public string HoraFormateada { get; set; } = string.Empty;
        public string FechaFormateada { get; set; } = string.Empty;
    }
}