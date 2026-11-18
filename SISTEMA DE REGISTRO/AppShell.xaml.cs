using SISTEMA_DE_REGISTRO.Views;

namespace SISTEMA_DE_REGISTRO
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación directa desde el Panel Principal
            Routing.RegisterRoute(nameof(PanelPrincipalPage), typeof(PanelPrincipalPage));
            Routing.RegisterRoute(nameof(RegistroDispositivoPage), typeof(RegistroDispositivoPage));
            Routing.RegisterRoute(nameof(ControlAccesoPage), typeof(ControlAccesoPage));
            Routing.RegisterRoute(nameof(MisRegistrosAccesoPage), typeof(MisRegistrosAccesoPage));
        }
    }
}