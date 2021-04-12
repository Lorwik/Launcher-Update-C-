using System;
using System.Threading;
using System.IO;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;
		
		// Todos los archivos del cliente en la subcarpeta WinterAO, para no mezclarlos con los archivos del Launcher.
        public static string ARGENTUM_PATH = Directory.GetCurrentDirectory() + "\\";
        public static string SERVER_SELECT = "WinterClient";
        public static string SERVER_EXE = "WinterAOResurrection.exe";
        public static string CONFIG_FILE = App.ARGENTUM_PATH + App.SERVER_SELECT + "\\Init\\Config.ini";

        protected override void OnStartup(StartupEventArgs e)
		{
            // Chequeo que solo haya 1 instancia de la aplicacion.
            _mutex = new Mutex(true, "Launcher - ComunidadWinter", out bool singleInstance);
			if (!singleInstance)
			{
				// ya hay una instancia de esta aplicación, cerramos la nueva.
				MessageBox.Show("Ya hay una instancia de esta aplicación abierta");
				Environment.Exit(0);
			}
			
            // Continuamos.
			base.OnStartup(e);
		}

        protected override void OnExit(ExitEventArgs e)
        {
			_mutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
