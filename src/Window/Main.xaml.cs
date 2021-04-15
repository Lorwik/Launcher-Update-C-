// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using Launcher.src;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace Launcher
{
    public partial class Main : Window, IComponentConnector
    {
        private readonly IO local = new IO();
        private readonly Networking networking = new Networking();

        /**
         * Constructor
         */
        public Main()
        {
            // Inicializamos los componentes de este formulario.
            InitializeComponent();

            iniciarCheckUpdate();

        }

        public static void cambiarServer(string serverName, string serverEXE) {

            App.SERVER_SELECT = serverName;
            App.SERVER_EXE = serverEXE;
            App.CONFIG_FILE = App.ARGENTUM_PATH + App.SERVER_SELECT + "\\Init\\Config.ini";
            IO.VERSIONFILE_PATH = App.ARGENTUM_PATH + App.SERVER_SELECT + "\\VersionInfo.json";

        }

        /*
         * Inicia la comprobación de actualizaciones para el server seleccionada.
         * Probablemente esto sea temporal, ya que cuando cambiamos de server quizas no me interesa volver a 
         * comprobar la versión del launcher.
         */
        private void iniciarCheckUpdate() {
            if (BuscarActualizaciones() == -1) {
                MessageBoxResult result = MessageBox.Show("Esta versión del launcher es obsoleta, ¿Desea descargar la ultima versión?", "Versión desactualizada", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes) {
                    var uri = "https://winterao.com.ar";
                    var psi = new System.Diagnostics.ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = uri;
                    System.Diagnostics.Process.Start(psi);
                }
                this.Close();
            }

            getChangelog();
            checkConfiguracion();
        }

        private void checkConfiguracion()
        {
            if(!File.Exists(Directory.GetCurrentDirectory() + "\\WinterAO\\" + "Init\\Config.ini")){
                btnConfiguracion.Visibility = Visibility.Hidden;
            }
            else
            {
                btnConfiguracion.Visibility = Visibility.Visible;
            }
        }

        private int BuscarActualizaciones() {
            
            if (networking.CheckOutdatedFiles() != null) {
                local.ArchivosDesactualizados = networking.CheckOutdatedFiles().Count;

            } else {
                //si la función devuelve un 0 quiere decir que hay que actualizar el launcher.
                return -1;

            }

            // Comprobamos la version actual del cliente
            if (local.ArchivosDesactualizados == 0) {
                lblDow.Content = "¡Cliente al día!";
                lblDow.HorizontalContentAlignment = HorizontalAlignment.Center;
                lblDow.Foreground = new SolidColorBrush(Colors.Yellow);

            } else { // Si el cliente no esta actualizado, lo notificamos
                lblDow.Content = "Tienes " + local.ArchivosDesactualizados + " archivos desactualizados...";
                lblDow.HorizontalContentAlignment = HorizontalAlignment.Center;
                lblDow.Foreground = new SolidColorBrush(Colors.Red);
            }

            return local.ArchivosDesactualizados;
        }

        /**
         * Inicia el proceso de actualizacion del cliente
         */
        private void Actualizar()
        {
            // ¿Hay archivos desactualizados?
            if (local.ArchivosDesactualizados > 0)
            {
                // Le indico al programa que estamos en medio de una actualización.
                local.Actualizando = true;

                // Anunciamos el numero de archivo que estamos descargando
                lblDow.Content = "Descargando " + networking.fileQueue[local.ArchivoActual] + ". Archivo " + local.ArchivoActual + " de " + (networking.fileQueue.Count - 1);
                lblDow.HorizontalContentAlignment = HorizontalAlignment.Left;
                lblDow.Foreground = new SolidColorBrush(Colors.White);

                // Comenzamos la descarga
                DescargarActualizaciones();
            }
        }

        /**
         * Comienza a descargar los archivos desactualizados.
         */
        private async void DescargarActualizaciones()
        {
            networking.CrearCarpetasRequeridas();
            
            WebClient client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateDone);
            await networking.IniciarDescarga(client);
        }

        private void getChangelog()
        {
            txtChangelog.Text = null;
            string Url = "http://winterao.com.ar/update/changelog" + App.SERVER_SELECT + ".txt";
            var webRequest = WebRequest.Create(Url);
            var responseStream = webRequest.GetResponse().GetResponseStream();

            using var streamReader = new StreamReader(responseStream);
            // Return next available character or -1 if there are no characters to be read
            while (streamReader.Peek() > -1)
            {
                txtChangelog.Text += streamReader.ReadLine() + "\n";
            }
        }
       
        private void UpdateDone(object sender, AsyncCompletedEventArgs e)
        {
            // Decimos que ya terminó esta descarga
            networking.downloadQueue.SetResult(true);

            // Si NO quedan archivos pendientes por descargar...
            if (local.ArchivoActual == (networking.fileQueue.Count - 1))
            {
                // Actualizo el VersionInfo.json
                IO.SaveLatestVersionInfo(networking.versionRemotaString);
                
                // Actualizo el label.
                lblDow.Content = "¡Actualización Completada!";
                lblDow.HorizontalContentAlignment = HorizontalAlignment.Center;
                lblDow.Foreground = new SolidColorBrush(Colors.Yellow);

                //activo configuración.
                checkConfiguracion();

                //Si está chekeado el check de comenzar automático abro el juego.
                if (chkLanzarAutomatico.IsChecked == true)
                {
                    AbrirJuego();
                }

                // Le digo al programa que ya no estamos actualizando mas nada.
                local.Actualizando = false;
                local.ArchivoActual = 0;
                local.ArchivosDesactualizados = 0;

                return;
            }

            // Si quedan, actualizamos el label.
            if (local.ArchivoActual < networking.fileQueue.Count)
            {
                local.ArchivoActual++;

                lblDow.Content = "Descargando " + networking.fileQueue[local.ArchivoActual] + ". Archivo " + local.ArchivoActual + " de " + (networking.fileQueue.Count - 1);
                lblDow.HorizontalContentAlignment = HorizontalAlignment.Left;
                grdPbarLlena.Width = (416 * local.ArchivoActual) / (networking.fileQueue.Count - 1);
                grdPbarLlena.Visibility = Visibility.Visible;
            }
        }

        private static void AbrirJuego()
        {
            string gameExecutable = App.ARGENTUM_PATH + App.SERVER_SELECT + "\\" + App.SERVER_EXE;

            if (File.Exists(gameExecutable))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = gameExecutable;
                startInfo.UseShellExecute = false;

                try
                {
                    
                    Configuracion.WritePrivateProfileString("PARAMETERS", "LAUCH", "1", App.CONFIG_FILE);
                    // Start the process with the info we specified.
                    Process.Start(startInfo);

                    // Cerramos el launcher.
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No se pudo abrir el ejecutable del juego, al parecer no existe!");
            }
        }

        /**
         * Boton para ir a la web
         */
        private void btnSitio_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://winterao.com.ar");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        /**
         * Boton Salir
         */
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /**
         * Boton 'Jugar'
         * 
         * Si el cliente esta ACTUALIZADO y existe el ejecutable del cliente, lo abrimos.
         * Si el cliente NO esta ACTUALIZADO, descargamos e instalamos las actualizaciones.
         */
        private void btnJugar_Click(object sender, RoutedEventArgs e)
        {
            // Si estamos actualizando el cliente no lo dejo clickear este boton.
            if (local.Actualizando == true) return;

            // Si hay archivos desactualizados, primero los actualizamos.
            if (local.ArchivosDesactualizados > 0)
            {
                Actualizar();
                return;
            }

            // Abrimos el cliente.
            AbrirJuego();
        }

        /**
         * Boton de minimizar
         */
        private void btnMini_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /**
         * Boton de Winter
         */
        private void btnWinter_Click(object sender, RoutedEventArgs e) {

            cambiarServer("WinterClient","WinterAOResurrection.exe");

            iniciarCheckUpdate();

            ventanaImperium.Visibility = Visibility.Hidden;
            ventanaWinter.Visibility = Visibility.Visible;
        }
        /**
         * Boton de Imperium
         */
        private void btnimperium_Click(object sender, RoutedEventArgs e) {

            cambiarServer("ImperiumClasico", "ImperiumClasico.exe");

            iniciarCheckUpdate();

            ventanaWinter.Visibility = Visibility.Hidden;
            ventanaImperium.Visibility = Visibility.Visible;
        }

        private void image_discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/WHWZwYP");
        }

        private void image_facebook_Click(object sender, RoutedEventArgs e)
        {

            Process.Start("https://www.facebook.com/Winter-AO-Resurrection-100546474972832"); 
        }

        private void image_instagram_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.instagram.com/winterao_argentumonline");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            Configuracion configuracion = new Configuracion();
            configuracion.Show();
        }

        private void txtChangelog_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
