using Supabase;
using SISTEMA_DE_REGISTRO.Models;
using SISTEMA_DE_REGISTRO.Services;

namespace SISTEMA_DE_REGISTRO.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly Client _supabaseClient;

        public LoginPage(Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string correo = txtCorreo.Text?.Trim() ?? string.Empty;
            string password = txtPassword.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlertAsync("Atención", "Por favor ingresa tu correo y contraseña.", "OK");
                return;
            }

            try
            {
                // Consulta directa a la tabla estudiantes para verificar credenciales
                var resultado = await _supabaseClient
                    .From<Estudiante>()
                    .Where(e => e.CorreoInstitucional == correo && e.Contrasena == password)
                    .Get();

                var estudianteEncontrado = resultado.Models.FirstOrDefault();

                if (estudianteEncontrado != null)
                {
                    if (!estudianteEncontrado.EstadoActivo)
                    {
                        await DisplayAlertAsync("Acceso Denegado", "Tu cuenta se encuentra inactiva. Contacta al administrador.", "OK");
                        return;
                    }

                    // Guardar los datos del estudiante en la sesión activa
                    SesionActual.Usuario = estudianteEncontrado;

                    // Navegar directamente al Panel Principal en el Shell
                    await Shell.Current.GoToAsync("//PanelPrincipalPage");
                }
                else
                {
                    await DisplayAlertAsync("Error de Acceso", "Correo o contraseña incorrectos.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", $"No se pudo iniciar sesión: {ex.Message}", "OK");
            }
        }

        private async void OnRegistroTapped(object sender, EventArgs e)
        {
            // Asegurar que no haya una sesión previa al registrar una cuenta nueva
            SesionActual.Usuario = null;
            await Navigation.PushAsync(new RegistroDispositivoPage(_supabaseClient));
        }
    }
}