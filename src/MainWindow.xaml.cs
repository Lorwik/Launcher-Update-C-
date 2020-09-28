// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
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
        private readonly IO io = new IO();
        private readonly Networking networking = new Networking();

        //METODOS

        /**
         * Main
         */
        public MainWindow()
        {
            this.InitializeComponent();

            int updates = networking.VerifyVersion().Count;

            //Comprobamos la version actual del cliente
            if (updates == 0)
            {
                io.ActualizacionPendiente = false;

                pbar.Value = 100.0;
                lblDow.Content = "Actualizado";
                lblDow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));
            }
            else //Si el cliente no esta actualizado, lo notificamos
            { 
                io.ActualizacionPendiente = true;

                lblDow.Content = "Tienes " + updates +  " archivos pendientes por descargar...";
                lblDow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }

            //Guardamos el parche actual

        }

        /**
         * Inicia el proceso de actualizacion del cliente
         */
        private void Actualizar()
        {
            
        }

        /**
         * Actualiza la barra de progreso
         */
        public void UpdateProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            this.pbar.Value = e.ProgressPercentage;

            if (this.pbar.Value != 100.0)
            {
                return;
            }
        }

        /**
         * Completa la actualizacion
         */
        private void UpdateDone(object sender, AsyncCompletedEventArgs e)
        {
            
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
            this.DragMove();
        }

        /**
         * Boton Salir
         */
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            if (io.Actualizando == true) return;

            networking.Get_RemoteVersion();

            // abrir el cliente
        }

        /**
         * Boton de minimizar
         */
        private void btnMini_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
