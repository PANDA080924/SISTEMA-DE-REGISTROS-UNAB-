using Supabase;
using ZXing.Net.Maui;
using SISTEMA_DE_REGISTRO.Models;
using SISTEMA_DE_REGISTRO.Services;

namespace SISTEMA_DE_REGISTRO.Views
{
    public partial class PanelPrincipalPage : ContentPage
    {
        private readonly Client _supabaseClient;
        private string _modoActual = string.Empty; // "ENTRADA" o "SALIDA"
        private bool _procesandoQr = false;
        private bool _actualizacionVerificada = false;

        public PanelPrincipalPage(Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            lblVersionApp.Text = $"Versión {AppInfo.Current.VersionString}";

            if (SesionActual.Usuario != null)
            {
                var est = SesionActual.Usuario;
                lblNombreEstudiante.Text = $"{est.Apellidos.ToUpper()}, {est.Nombres.ToUpper()}";
                lblCarreraFacultad.Text = $"{est.CarreraProfesional} | {est.Facultad}";
                lblCiclo.Text = $"Ciclo Académico: {est.CicloAcademico}";
            }

            if (!_actualizacionVerificada)
            {
                _actualizacionVerificada = true;
                await VerificarActualizacionAsync();
            }
        }

        private async Task VerificarActualizacionAsync()
        {
            try
            {
                var res = await _supabaseClient
                    .From<ControlVersion>()
                    .Order(v => v.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var ultimaVersion = res.Models.FirstOrDefault();
                if (ultimaVersion == null || string.IsNullOrWhiteSpace(ultimaVersion.Version))
                    return;

                string versionActual = AppInfo.Current.VersionString;

                if (!string.Equals(ultimaVersion.Version.Trim(), versionActual.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    bool descargar = await DisplayAlertAsync(
                        $"Nueva Versión Disponible ({ultimaVersion.Version})",
                        $"Novedades:\n{ultimaVersion.Novedades}\n\n¿Deseas descargar e instalar la actualización ahora?",
                        "Actualizar",
                        "Más tarde");

                    if (descargar && !string.IsNullOrWhiteSpace(ultimaVersion.UrlDescarga))
                    {
                        await Launcher.Default.OpenAsync(new Uri(ultimaVersion.UrlDescarga.Trim()));
                    }
                }
            }
            catch
            {
                // Ignorar silenciosamente si no hay conexión o la tabla está vacía
            }
        }

        private async void OnAbrirCamaraClicked(object sender, EventArgs e)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Permiso Requerido", "Se necesita acceso a la cámara para escanear el QR.", "OK");
                return;
            }

            contenedorCamara.IsVisible = true;
            btnAbrirCamara.IsVisible = false;
            _modoActual = string.Empty;
            barcodeView.IsDetecting = false;
        }

        private void OnCerrarCamaraClicked(object sender, EventArgs e)
        {
            barcodeView.IsDetecting = false;
            contenedorCamara.IsVisible = false;
            btnAbrirCamara.IsVisible = true;
            _modoActual = string.Empty;
            _procesandoQr = false;
        }

        private void OnEntradaClicked(object sender, EventArgs e)
        {
            _modoActual = "ENTRADA";
            _procesandoQr = false;
            barcodeView.IsDetecting = true;
        }

        private void OnSalidaClicked(object sender, EventArgs e)
        {
            _modoActual = "SALIDA";
            _procesandoQr = false;
            barcodeView.IsDetecting = true;
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var codigo = e.Results?.FirstOrDefault();
            if (codigo == null || string.IsNullOrWhiteSpace(codigo.Value)) return;

            if (_procesandoQr || string.IsNullOrEmpty(_modoActual)) return;

            _procesandoQr = true;
            barcodeView.IsDetecting = false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (SesionActual.Usuario == null)
                    {
                        await DisplayAlertAsync("Error de Sesión", "No hay un estudiante autenticado.", "OK");
                        return;
                    }

                    // 1. Validar formato: UNAB|PUERTA|CODIGO_ESTUDIANTE(|TIMESTAMP)
                    string[] partes = codigo.Value.Split('|');
                    if (partes.Length < 3 || partes[0] != "UNAB")
                    {
                        await DisplayAlertAsync("QR Inválido", "Este código QR no pertenece al sistema de control UNAB.", "OK");
                        return;
                    }

                    string puertaEscaneada = partes[1];
                    string codigoEstudianteQR = partes[2];

                    // 2. Verificar que el QR corresponda al estudiante logueado
                    if (!string.Equals(codigoEstudianteQR, SesionActual.Usuario.CodigoEstudiante, StringComparison.OrdinalIgnoreCase))
                    {
                        await DisplayAlertAsync(
                            "Acceso Denegado",
                            $"El QR pertenece al código {codigoEstudianteQR}, pero tu sesión corresponde a {SesionActual.Usuario.CodigoEstudiante}.",
                            "OK");
                        return;
                    }

                    // 3. Buscar el dispositivo registrado del estudiante (si tiene alguno)
                    var resDisp = await _supabaseClient
                        .From<Dispositivo>()
                        .Where(d => d.EstudianteId == SesionActual.Usuario.Id)
                        .Get();

                    var dispositivo = resDisp.Models.FirstOrDefault();

                    // 4. Insertar en la tabla real 'registros_accesos'
                    var nuevoRegistro = new RegistroAcceso
                    {
                        CreadoAt = DateTime.UtcNow,
                        CodigoEstudiante = SesionActual.Usuario.CodigoEstudiante,
                        TipoEvento = _modoActual,
                        EscaneadoPor = puertaEscaneada,
                        EstudianteId = SesionActual.Usuario.Id,
                        DispositivoId = dispositivo?.Id,
                        Equipo = dispositivo != null ? $"{dispositivo.TipoDispositivo} {dispositivo.Marca} {dispositivo.Modelo}".Trim() : "Sin dispositivo",
                        NumeroSerie = dispositivo?.NumeroSerie ?? "N/A"
                    };

                    await _supabaseClient.From<RegistroAcceso>().Insert(nuevoRegistro);

                    // 5. Mostrar confirmación
                    if (_modoActual == "ENTRADA")
                    {
                        await DisplayAlertAsync("Entrada", "Registro de entrada procesado.", "OK");
                    }
                    else if (_modoActual == "SALIDA")
                    {
                        await DisplayAlertAsync("Salida", "Registro de salida procesado.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error al Registrar", $"No se pudo guardar en la base de datos: {ex.Message}", "OK");
                }
                finally
                {
                    _modoActual = string.Empty;
                    _procesandoQr = false;
                }
            });
        }

        private async void OnMiControlAsistenciaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(MisRegistrosAccesoPage));
        }

        private async void OnRegistrarDispositivoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegistroDispositivoPage));
        }

        private async void OnControlGaritaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ControlAccesoPage));
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlertAsync("Cerrar Sesión", "¿Seguro que deseas salir?", "Sí", "No");
            if (confirmar)
            {
                barcodeView.IsDetecting = false;
                SesionActual.Usuario = null;
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}