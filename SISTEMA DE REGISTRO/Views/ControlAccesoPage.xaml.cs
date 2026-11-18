using Supabase;
using SISTEMA_DE_REGISTRO.Models;

namespace SISTEMA_DE_REGISTRO.Views
{
    public partial class ControlAccesoPage : ContentPage
    {
        private readonly Client _supabaseClient;

        private Estudiante? _estudianteEncontrado = null;
        private Dispositivo? _dispositivoEncontrado = null;

        public ControlAccesoPage(Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        private async void OnVolverClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnEscanearQRClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Lector QR", "Iniciando cámara para escaneo de código QR...", "OK");
        }

        // Búsqueda por Código de Estudiante o N° de Serie
        private async void OnBuscarClicked(object? sender, EventArgs e)
        {
            string query = txtBusqueda.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                await DisplayAlertAsync("Atención", "Ingresa un número de serie o código de estudiante.", "OK");
                return;
            }

            try
            {
                // 1. Buscar por N° de Serie en Dispositivos
                var respDisp = await _supabaseClient
                    .From<Dispositivo>()
                    .Where(d => d.NumeroSerie == query)
                    .Get();

                _dispositivoEncontrado = respDisp.Models.FirstOrDefault();

                if (_dispositivoEncontrado != null)
                {
                    // Buscar estudiante ligado al dispositivo
                    var respEst = await _supabaseClient
                        .From<Estudiante>()
                        .Where(e => e.Id == _dispositivoEncontrado.EstudianteId)
                        .Get();

                    _estudianteEncontrado = respEst.Models.FirstOrDefault();
                }
                else
                {
                    // 2. Si no es serie, buscar por Código de Estudiante
                    var respEst = await _supabaseClient
                        .From<Estudiante>()
                        .Where(e => e.CodigoEstudiante == query)
                        .Get();

                    _estudianteEncontrado = respEst.Models.FirstOrDefault();

                    if (_estudianteEncontrado != null)
                    {
                        var respDispEst = await _supabaseClient
                            .From<Dispositivo>()
                            .Where(d => d.EstudianteId == _estudianteEncontrado.Id)
                            .Get();

                        _dispositivoEncontrado = respDispEst.Models.FirstOrDefault();
                    }
                }

                MostrarDatosEnPantalla();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Búsqueda", $"No se pudo consultar: {ex.Message}", "OK");
            }
        }

        private void MostrarDatosEnPantalla()
        {
            if (_estudianteEncontrado != null)
            {
                lblEstudiante.Text = $"Estudiante: {_estudianteEncontrado.Nombres} {_estudianteEncontrado.Apellidos}";
                lblCodigo.Text = $"Código: {_estudianteEncontrado.CodigoEstudiante}";
                lblCarrera.Text = $"Carrera: {_estudianteEncontrado.CarreraProfesional}";
            }
            else
            {
                lblEstudiante.Text = "Estudiante: No encontrado";
                lblCodigo.Text = "Código: -";
                lblCarrera.Text = "Carrera: -";
            }

            if (_dispositivoEncontrado != null)
            {
                lblEquipo.Text = $"Equipo: {_dispositivoEncontrado.TipoDispositivo} {_dispositivoEncontrado.Marca} {_dispositivoEncontrado.Modelo}";
                lblSerie.Text = $"N° Serie: {_dispositivoEncontrado.NumeroSerie}";
            }
            else
            {
                lblEquipo.Text = "Equipo: Sin dispositivo registrado";
                lblSerie.Text = "N° Serie: -";
            }
        }

        private async void OnRegistrarEntradaClicked(object? sender, EventArgs e)
        {
            await RegistrarMovimientoAcceso("ENTRADA");
        }

        private async void OnRegistrarSalidaClicked(object? sender, EventArgs e)
        {
            await RegistrarMovimientoAcceso("SALIDA");
        }

        private async Task RegistrarMovimientoAcceso(string tipoEvento)
        {
            if (_estudianteEncontrado == null)
            {
                await DisplayAlertAsync("Atención", "Primero debes buscar y seleccionar a un estudiante válido.", "OK");
                return;
            }

            try
            {
                var nuevoAcceso = new RegistroAcceso
                {
                    CodigoEstudiante = _estudianteEncontrado.CodigoEstudiante,
                    Equipo = _dispositivoEncontrado != null ? $"{_dispositivoEncontrado.Marca} {_dispositivoEncontrado.Modelo}" : "Sin Equipo",
                    TipoEvento = tipoEvento,
                    EscaneadoPor = "Garita Principal",
                    NumeroSerie = _dispositivoEncontrado?.NumeroSerie ?? "S/N",
                    EstudianteId = _estudianteEncontrado.Id,
                    DispositivoId = _dispositivoEncontrado?.Id ?? string.Empty,
                    FechaRegistro = DateTime.UtcNow
                };

                await _supabaseClient.From<RegistroAcceso>().Insert(nuevoAcceso);

                await DisplayAlertAsync("Acceso Registrado", $"Se registró la {tipoEvento} correctamente.", "Aceptar");
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Registro", $"No se pudo guardar el acceso: {ex.Message}", "OK");
            }
        }

        private void LimpiarFormulario()
        {
            txtBusqueda.Text = string.Empty;
            _estudianteEncontrado = null;
            _dispositivoEncontrado = null;
            lblEstudiante.Text = "Estudiante: -";
            lblCodigo.Text = "Código: -";
            lblCarrera.Text = "Carrera: -";
            lblEquipo.Text = "Equipo: -";
            lblSerie.Text = "N° Serie: -";
        }
    }
}