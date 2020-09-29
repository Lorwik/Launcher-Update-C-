// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Launcher.src;

namespace Launcher
{
    public partial class MainWindow : Window, IComponentConnector
    {

        //ATRIBUTOS
        private readonly IO local = new IO();
        private readonly Networking networking =  new Networking();

        //METODOS

        /**
         * Constructor
         */
        public MainWindow()
        {
            // Inicializamos los componentes de este formulario.
            InitializeComponent();

            // Buscamos actualizaciones...
            BuscarActualizaciones();
        }

        private void BuscarActualizaciones()
        {
            local.ArchivosDesactualizados = networking.CheckOutdatedFiles().Count;

            // Comprobamos la version actual del cliente
            if (local.ArchivosDesactualizados == 0)
            {
                pbar.Value = 100.0;
                lblDow.Content = "Actualizado";
                lblDow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));
            }
            else // Si el cliente no esta actualizado, lo notificamos
            {
                lblDow.Content = "Tienes " + local.ArchivosDesactualizados + " archivos desactualizados...";
                lblDow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }
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
                lblDow.Content = "Descargando actualizacion " + local.ArchivoActual + " de " + local.ArchivosDesactualizados;

                // Comenzamos la descarga
                Descargar(networking.fileQueue[local.ArchivoActual]);
            }
        }

        /**
         * Actualiza la barra de progreso
         */
        public void UpdateProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            pbar.Value = e.ProgressPercentage;

            if (pbar.Value != 100.0) return;
        }

        /**
         * Completa la actualizacion
         */
        private void UpdateDone(object sender, AsyncCompletedEventArgs e)
        {
            local.ArchivoActual++;
            // Volvemos a comprobar el MD5 del archivo descargado por si se descargo corrupto.
            string checksumArchivoLocal = IO.checkMD5(networking.fileQueue[local.ArchivoActual]);
            string checksumArchivoRemoto = networking.versionRemota.Files[local.ArchivoActual].checksum;

            // En caso de que sean diferentes...
            if (checksumArchivoLocal != checksumArchivoRemoto)
            {
                string error = "El MD5 de esta actualización NO COINCIDE!" + "\r\n" +
                                "Archivo: " + networking.versionRemota.Files[local.ArchivoActual].name + "\r\n" +
                                "MD5 Local:" + checksumArchivoLocal + "\r\n" +
                                "MD5 Remoto: " + checksumArchivoRemoto + "\r\n\r\n" +
                                "¿Desea intentar descargar la actualización una vez mas?";

                MessageBoxResult badUpdate = MessageBox.Show(error, "Descarga Corrupta", MessageBoxButton.YesNo);

                // Le preguntamos si quiere descargar la actualización 1 vez mas.
                if (badUpdate == MessageBoxResult.Yes)
                {
                    // Borramos la actualización corrupta.
                    File.Delete(Directory.GetCurrentDirectory() + networking.versionRemota.Files[local.ArchivoActual].name);

                    // Salimos del método.
                    Actualizar();
                    return;
                }
                else
                {
                    // Si elige no volver a descargar la actualización, cerramos el launcher.
                    Close();
                }

            }

            // Si terminamos de desactualizar, re-habilitamos el boton de Jugar
            if (local.ArchivoActual == local.ArchivosDesactualizados)
            {
                local.Actualizando = false;
                return;
            }
        }

        /**
         * Descarga la actualizacion
         */
        private void Descargar(string Url)
        {
            WebClient webClient = null;
            try
            {
                webClient = new WebClient();
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(UpdateProgressChange);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateDone);
                webClient.DownloadFileAsync(new Uri(Networking.HOST + Url), Directory.GetCurrentDirectory() + Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //call this if exception occurs or not
                //in this example, dispose the WebClient
                webClient.Dispose();
            }
        }


        /**
         * Boton para ir a la web
         */
        private void btnSitio_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(URLWeb);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /**
         * Boton Salir
         */
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            string gameExecutable = Directory.GetCurrentDirectory() + "/WinterAO Resurrection.exe";
            if (File.Exists(gameExecutable))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = gameExecutable;
                startInfo.UseShellExecute = false;

                try
                {
                    // Start the process with the info we specified.
                    Process.Start(startInfo);

                    // Cerramos el launcher.
                    Close();
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
         * Boton de minimizar
         */
        private void btnMini_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
