// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using Launcher.src;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;

using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Launcher
{
    public partial class MainWindow : Window, IComponentConnector
    {

        //ATRIBUTOS
        private readonly IO local = new IO();
        private readonly Networking networking = new Networking();

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
                lblDow.Content = "Descargando " + networking.versionRemota.Files[local.ArchivoActual].name + ". Archivo " + local.ArchivoActual + " de " + (local.ArchivosDesactualizados - 1);

                // Comenzamos la descarga
                Descargar(networking.fileQueue[local.ArchivoActual]);
            }
        }

        /**
         * Comienza a descargar los archivos desactualizados.
         */
        private void Descargar(string URL)
        {
            // Creo las carpetas necesarias para descargar las cosas.
            networking.CrearCarpetasRequeridas();

            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(UpdateProgressChange);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateDone);
            client.DownloadFileAsync(new Uri(Networking.HOST + URL), Directory.GetCurrentDirectory() + URL);
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
            if (local.ArchivoActual < networking.fileQueue.Count - 1)
            {
                local.ArchivoActual++;

                lblDow.Content = "Descargando " + networking.fileQueue[local.ArchivoActual] +
                                 ". Archivo " + local.ArchivoActual + " de " + networking.fileQueue.Count;

                Descargar(networking.fileQueue[local.ArchivoActual]);

            }
            else
            {
                // Guardamos el VersionInfo.json actualizado. 
                IO.SaveLatestVersionInfo(networking.versionRemotaString);

                // Limpiamos la cola de archivos para descargar.
                networking.fileQueue.Clear();

                // Le decimos al programa que ya NO estamos en medio de una actualizacion.
                local.ArchivoActual = 0;
                local.ArchivosDesactualizados = 0;
                local.Actualizando = false;
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
            string gameExecutable = Directory.GetCurrentDirectory() + "/WinterAOResurrection.exe";
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
