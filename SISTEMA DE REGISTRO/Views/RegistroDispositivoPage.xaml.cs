using Supabase;
using SISTEMA_DE_REGISTRO.Models;
using SISTEMA_DE_REGISTRO.Services;

namespace SISTEMA_DE_REGISTRO.Views
{
    public partial class RegistroDispositivoPage : ContentPage
    {
        private readonly Client _supabaseClient;

        private List<CarreraInfo> _todasLasCarreras = new();
        private CarreraInfo? _carreraSeleccionadaObj = null;

        private string _facultadSeleccionada = string.Empty;
        private string _carreraSeleccionada = string.Empty;
        private string _cicloSeleccionado = string.Empty;
        private string _tipoSeleccionado = string.Empty;

        private FileResult? _fotoSeleccionada = null;

        public RegistroDispositivoPage(Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarCarrerasInfoAsync();

            // Autorrellenar si el estudiante ya inició sesión en la aplicación
            if (SesionActual.Usuario != null)
            {
                var est = SesionActual.Usuario;

                txtNombres.Text = est.Nombres;
                txtApellidos.Text = est.Apellidos;
                txtCorreo.Text = est.CorreoInstitucional;
                txtContrasena.Text = est.Contrasena;
                txtCodigoEstudiante.Text = est.CodigoEstudiante;

                // Bloquear edición de credenciales del alumno logueado
                txtNombres.IsReadOnly = true;
                txtApellidos.IsReadOnly = true;
                txtCorreo.IsReadOnly = true;
                txtContrasena.IsReadOnly = true;
                txtCodigoEstudiante.IsReadOnly = true;

                if (!string.IsNullOrWhiteSpace(est.Facultad))
                {
                    _facultadSeleccionada = est.Facultad;
                    lblFacultad.Text = est.Facultad;
                    lblFacultad.TextColor = Colors.White;
                }

                if (!string.IsNullOrWhiteSpace(est.CarreraProfesional))
                {
                    _carreraSeleccionada = est.CarreraProfesional;
                    _carreraSeleccionadaObj = _todasLasCarreras.FirstOrDefault(c => c.NombreCarrera == est.CarreraProfesional);
                    lblCarrera.Text = est.CarreraProfesional;
                    lblCarrera.TextColor = Colors.White;
                }

                if (!string.IsNullOrWhiteSpace(est.CicloAcademico))
                {
                    _cicloSeleccionado = est.CicloAcademico;
                    lblCiclo.Text = est.CicloAcademico;
                    lblCiclo.TextColor = Colors.White;
                }
            }
        }

        private async Task CargarCarrerasInfoAsync()
        {
            try
            {
                var resultado = await _supabaseClient.From<CarreraInfo>().Get();
                _todasLasCarreras = resultado.Models;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error BD", $"No se pudieron cargar las carreras: {ex.Message}", "OK");
            }
        }

        private async void OnVolverClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnEscanearQRClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Escanear QR", "Lector QR activado para escaneo.", "OK");
        }

        private async void OnTomarFotoClicked(object? sender, EventArgs e)
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlertAsync("No disponible", "Este dispositivo no soporta captura con cámara.", "OK");
                    return;
                }

                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlertAsync("Permiso Requerido", "Debes permitir el acceso a la cámara para tomar la foto.", "OK");
                    return;
                }

                var foto = await MediaPicker.Default.CapturePhotoAsync();
                if (foto != null)
                {
                    _fotoSeleccionada = foto;
                    imgDispositivo.Source = ImageSource.FromFile(foto.FullPath);
                    imgDispositivo.Aspect = Aspect.AspectFill;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Cámara", $"No se pudo capturar la imagen: {ex.Message}", "OK");
            }
        }

        private async void OnSeleccionarFotoClicked(object? sender, EventArgs e)
        {
            try
            {
                var resultados = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                {
                    Title = "Selecciona la foto de tu dispositivo"
                });

                var foto = resultados?.FirstOrDefault();
                if (foto != null)
                {
                    _fotoSeleccionada = foto;
                    imgDispositivo.Source = ImageSource.FromFile(foto.FullPath);
                    imgDispositivo.Aspect = Aspect.AspectFill;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Galería", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            }
        }

        private async void OnSeleccionarTipoTapped(object? sender, EventArgs e)
        {
            string? opcion = await DisplayActionSheetAsync("Selecciona Tipo", "Cancelar", null, "Laptop", "Tablet", "Smartphone", "Otro");
            if (!string.IsNullOrEmpty(opcion) && opcion != "Cancelar")
            {
                _tipoSeleccionado = opcion;
                lblTipo.Text = opcion;
                lblTipo.TextColor = Colors.White;
            }
        }

        private async void OnSeleccionarFacultadTapped(object? sender, EventArgs e)
        {
            if (SesionActual.Usuario != null) return; // Evita cambiar facultad si ya está logueado

            if (!_todasLasCarreras.Any())
            {
                await CargarCarrerasInfoAsync();
            }

            var facultadesDisponibles = _todasLasCarreras
                .Select(c => c.Facultad)
                .Distinct()
                .ToArray();

            string? opcion = await DisplayActionSheetAsync("Selecciona Facultad", "Cancelar", null, facultadesDisponibles);

            if (!string.IsNullOrEmpty(opcion) && opcion != "Cancelar")
            {
                _facultadSeleccionada = opcion;
                lblFacultad.Text = opcion;
                lblFacultad.TextColor = Colors.White;

                _carreraSeleccionadaObj = null;
                _carreraSeleccionada = string.Empty;
                lblCarrera.Text = "Selecciona Carrera Profesional";
                lblCarrera.TextColor = Color.FromArgb("#64748B");

                _cicloSeleccionado = string.Empty;
                lblCiclo.Text = "Ciclo Académico";
                lblCiclo.TextColor = Color.FromArgb("#64748B");
            }
        }

        private async void OnSeleccionarCarreraTapped(object? sender, EventArgs e)
        {
            if (SesionActual.Usuario != null) return; // Evita cambiar carrera si ya está logueado

            if (string.IsNullOrEmpty(_facultadSeleccionada))
            {
                await DisplayAlertAsync("Atención", "Por favor selecciona primero una Facultad.", "OK");
                return;
            }

            var carrerasFacultad = _todasLasCarreras
                .Where(c => c.Facultad == _facultadSeleccionada)
                .ToList();

            string[] nombresCarreras = carrerasFacultad.Select(c => c.NombreCarrera).ToArray();

            string? opcion = await DisplayActionSheetAsync("Selecciona Carrera Profesional", "Cancelar", null, nombresCarreras);

            if (!string.IsNullOrEmpty(opcion) && opcion != "Cancelar")
            {
                _carreraSeleccionadaObj = carrerasFacultad.FirstOrDefault(c => c.NombreCarrera == opcion);
                _carreraSeleccionada = opcion;
                lblCarrera.Text = opcion;
                lblCarrera.TextColor = Colors.White;

                _cicloSeleccionado = string.Empty;
                lblCiclo.Text = "Ciclo Académico";
                lblCiclo.TextColor = Color.FromArgb("#64748B");
            }
        }

        private async void OnSeleccionarCicloTapped(object? sender, EventArgs e)
        {
            if (SesionActual.Usuario != null) return; // Evita cambiar ciclo si ya está logueado

            int limiteCiclos = _carreraSeleccionadaObj?.TotalCiclos ?? 10;

            List<string> listaCiclos = new();
            for (int i = 1; i <= limiteCiclos; i++)
            {
                listaCiclos.Add($"Ciclo {i}");
            }

            string? opcion = await DisplayActionSheetAsync("Ciclo Académico", "Cancelar", null, listaCiclos.ToArray());

            if (!string.IsNullOrEmpty(opcion) && opcion != "Cancelar")
            {
                _cicloSeleccionado = opcion;
                lblCiclo.Text = opcion;
                lblCiclo.TextColor = Colors.White;
            }
        }

        private async Task<string?> SubirFotoDispositivoAsync(string estudianteId)
        {
            if (_fotoSeleccionada == null) return null;

            using var stream = await _fotoSeleccionada.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] bytesImagen = ms.ToArray();

            string extension = Path.GetExtension(_fotoSeleccionada.FileName);
            if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";

            string nombreArchivo = $"{estudianteId}_{Guid.NewGuid():N}{extension}";

            await _supabaseClient.Storage
                .From("dispositivos_fotos")
                .Upload(bytesImagen, nombreArchivo);

            return _supabaseClient.Storage
                .From("dispositivos_fotos")
                .GetPublicUrl(nombreArchivo);
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            // Validamos que Código, Nombres, Apellidos y Contraseña no estén vacíos
            if (string.IsNullOrWhiteSpace(txtCodigoEstudiante.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                await DisplayAlertAsync("Campos Incompletos", "Completa como mínimo Código, Nombres, Apellidos y Contraseña.", "OK");
                return;
            }

            btnGuardar.IsEnabled = false;
            btnGuardar.Text = "Guardando...";

            try
            {
                string estudianteId;

                // CASO 1: El estudiante ya tiene sesión activa (solo registra un dispositivo nuevo)
                if (SesionActual.Usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(txtMarca.Text) || string.IsNullOrWhiteSpace(txtNumeroSerie.Text))
                    {
                        await DisplayAlertAsync("Datos de Equipo", "Ingresa al menos la Marca y el Número de Serie del dispositivo.", "OK");
                        return;
                    }

                    estudianteId = SesionActual.Usuario.Id;
                }
                // CASO 2: Usuario nuevo registrándose desde el Login
                else
                {
                    var nuevoEstudiante = new Estudiante
                    {
                        CodigoEstudiante = txtCodigoEstudiante.Text.Trim(),
                        Nombres = txtNombres.Text.Trim(),
                        Apellidos = txtApellidos.Text.Trim(),
                        CorreoInstitucional = txtCorreo.Text?.Trim() ?? string.Empty,
                        Contrasena = txtContrasena.Text.Trim(),
                        Facultad = _facultadSeleccionada,
                        CarreraProfesional = _carreraSeleccionada,
                        CicloAcademico = _cicloSeleccionado,
                        EstadoActivo = true,
                        FechaRegistro = DateTime.UtcNow
                    };

                    var resultado = await _supabaseClient.From<Estudiante>().Insert(nuevoEstudiante);
                    var estudianteGuardado = resultado.Models.FirstOrDefault();
                    estudianteId = estudianteGuardado?.Id ?? string.Empty;
                }

                // Guardar el dispositivo si se ingresaron sus datos
                if (!string.IsNullOrWhiteSpace(txtMarca.Text) && !string.IsNullOrWhiteSpace(txtNumeroSerie.Text))
                {
                    string? urlFoto = await SubirFotoDispositivoAsync(estudianteId);

                    var nuevoDispositivo = new Dispositivo
                    {
                        EstudianteId = estudianteId,
                        Marca = txtMarca.Text.Trim(),
                        Modelo = txtModelo.Text?.Trim() ?? string.Empty,
                        NumeroSerie = txtNumeroSerie.Text.Trim(),
                        TipoDispositivo = _tipoSeleccionado,
                        FotoUrl = urlFoto,
                        FechaRegistro = DateTime.UtcNow
                    };

                    await _supabaseClient.From<Dispositivo>().Insert(nuevoDispositivo);
                }

                string mensajeExito = SesionActual.Usuario != null
                    ? "El dispositivo y su fotografía se registraron correctamente."
                    : "El estudiante y su dispositivo se guardaron correctamente.";

                await DisplayAlertAsync("Éxito", mensajeExito, "Aceptar");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", $"No se pudo guardar: {ex.Message}", "OK");
            }
            finally
            {
                btnGuardar.IsEnabled = true;
                btnGuardar.Text = "Guardar Registro";
            }
        }
    }
}