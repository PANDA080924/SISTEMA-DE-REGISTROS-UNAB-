using Microsoft.Extensions.Logging;
using Supabase;
using SISTEMA_DE_REGISTRO.Views;
using ZXing.Net.Maui.Controls; // 👈 1. Agregar namespace

namespace SISTEMA_DE_REGISTRO
{
    public static class MauiProgram
    {
        private const string SUPABASE_URL = "https://xnumrsfknhxuorroykuq.supabase.co";
        private const string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhudW1yc2Zrbmh4dW9ycm95a3VxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODYzMTI4NTUsImV4cCI6MjEwMTg4ODg1NX0.9-mcWmlNeoRN8hnC6TQ51R4rooLnF_uFEd0ZfEfI9ZM";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader() // 👈 2. Inicializar el lector de código de barras / QR
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            var supabaseClient = new Client(SUPABASE_URL, SUPABASE_KEY, options);
            builder.Services.AddSingleton(supabaseClient);

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<PanelPrincipalPage>();
            builder.Services.AddTransient<RegistroDispositivoPage>();
            builder.Services.AddTransient<ControlAccesoPage>();
            builder.Services.AddTransient<MisRegistrosAccesoPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}