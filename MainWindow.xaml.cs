// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;


namespace Uplauncher{

      public partial class MainWindow : Window, IComponentConnector{

            //ATRIBUTOS
            private string versionRemota;   //Version actual del cliente remoto nos dice si hay parches
            private string versionLocal;    //Version actual Local del clienteo
            private double ParcheActual;    //Indica el parche en el que nos encontramos actualmente
            private bool UpdatePendiente;   //Bandera que nos indica si tenemos actualizaciones pendientes
            private bool Actualizando;      //Bandera que nos indica si estamos en proceso de actualizacion
            private readonly string URLWeb = "http://winterao.com.ar";
            private readonly string URLWiki = "http://winterao.com.ar/wiki/";
            private readonly string LocalVersionFile = "\\Init\\Version.dat";
            private readonly string RemoteVersionFile = "/update/version.txt";
        //METODOS

        /**
         * Main
         */
        public MainWindow(){
                this.InitializeComponent();

                //Inicializamos las variables
                this.ParcheActual = 0;
                this.Actualizando = false;
                
                //Comprobamos la version actual del cliente
                if (this.VerifyVersion()){
                    this.UpdatePendiente = false;
                    this.pbar.Value = 100.0;
                    this.lblDow.Content = (object)"Actualizado";
                    this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));

                }else{ //Si el cliente no esta actualizado, lo notificamos
                    this.UpdatePendiente = true;
                    this.lblDow.Content = (object)"NO Actualizado.";
                    this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
                }

                //Guardamos el parche actual
                this.ParcheActual = Convert.ToInt32(this.versionLocal);

        }

            /**
             * Inicia el proceso de actualizacion del cliente
             */
            private void Actualizar(){
                //¿El parche actual coincide con la version remota?
                if (Convert.ToInt32(this.ParcheActual) < Convert.ToInt32(this.versionRemota)){

                    this.ParcheActual++;
                    //Anunciamos el parche que estamos descargando
                    this.lblDow.Content = "Descargando actualizacion " + this.ParcheActual;

                    //Descargamos e instalamos
                    string DownloadTo = Directory.GetCurrentDirectory() + "\\update" + this.ParcheActual + ".zip";
                    this.lblDow.Content = (object)"Actualizando...";
                    this.Download(URLWeb +"/update/update" + this.ParcheActual + ".zip", DownloadTo);
                    lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#0092D6"));

                }else{ 

                    //int num1 = (int)MessageBox.Show("¡WinterAO ha sido actualizado! Ahora puedes jugar a la ultima versión.", "Notification");
                    this.Actualizando = false;
                    this.lblDow.Content = (object)"Actualizado";
                    this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));
                }
            }

            /**
             * Comprueba la ultima version disponible
             */
            private bool VerifyVersion(){
                StreamReader streamReader = new StreamReader(Directory.GetCurrentDirectory() + LocalVersionFile);
                this.versionLocal = streamReader.ReadToEnd();
                this.versionRemota = this.ReadRemoteTextFile(URLWeb + RemoteVersionFile);
                streamReader.Close();

                return this.versionRemota == versionLocal;
            }

            private string ReadRemoteTextFile(string Url){
                Stream responseStream = WebRequest.Create(new Uri(Url)).GetResponse().GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string end = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return end;
            }

            /**
             * Descarga la actualizacion
             */
            private void Download(string Url, string DownloadTo){
                 WebClient webClient = new WebClient();
                 webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.UpdateProgressChange);
                 webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.UpdateDone);
                 webClient.DownloadFileAsync(new Uri(Url), DownloadTo);
                 webClient.Dispose();
        }

            /**
             * Extrae el .zip
             */
            private void MyExtract(){
                string str = Directory.GetCurrentDirectory() + "\\update" + ParcheActual + ".zip";
                string baseDirectory = Directory.GetCurrentDirectory();

                if (!System.IO.File.Exists(str))
                    return;
                using (ZipFile zipFile = ZipFile.Read(str)){
                    foreach (ZipEntry zipEntry in zipFile)
                    zipEntry.Extract(baseDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
                try{
                    System.IO.File.Delete(str);
                    //Volvemos a llamar al Actualizar
                    this.Actualizar();
                }catch (IOException ex){
                    int num = (int) MessageBox.Show(ex.Message);
                }
            }

            /**
             * Actualiza la barra de progreso
             */
            private void UpdateProgressChange(object sender, DownloadProgressChangedEventArgs e){
                this.pbar.Value = (double) e.ProgressPercentage;

                if (this.pbar.Value != 100.0) { 
                    return;
                }
            }

            /**
             * Completa la actualizacion
             */
            private void UpdateDone(object sender, AsyncCompletedEventArgs e){

                //Posible mensaje de parche instalado
                //Extraemos
                this.MyExtract();
            try
            {
                    System.IO.File.Delete(Directory.GetCurrentDirectory() + LocalVersionFile);
                }catch (IOException ex){
                    int num2 = (int) MessageBox.Show(ex.Message);
                }

                StreamWriter streamWriter = new StreamWriter(Directory.GetCurrentDirectory() + LocalVersionFile);
                streamWriter.Write(this.ParcheActual);
                streamWriter.Close();
            }

            /**
             * Boton para ir a la web
             */
            private void btnSitio_Click(object sender, RoutedEventArgs e){
                Process.Start(URLWeb);
            }

            private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e){
                this.DragMove();
            }

            /**
             * Boton Salir
             */
            private void btnSalir_Click(object sender, RoutedEventArgs e){
                this.Close();
            }

            /**
             * Boton de jugar
             */
            private void btnJugar_Click(object sender, RoutedEventArgs e){
                if (this.Actualizando == false){
                    //¿Hay actualizaciones pendientes?
                    if (this.UpdatePendiente == false){
                
                       /*if (!System.IO.File.Exists("AH.dll"))
                        {
                        int num1 = (int) MessageBox.Show("No se ha podido encontrar el antihack!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        }*/
                        if (!System.IO.File.Exists("WinterAO Resurrection.exe")) {
                            int num2 = (int)MessageBox.Show("No se ha encontrado el ejecutable de WinterAO Resurrection!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        } else if (new Process() {
                            StartInfo = new ProcessStartInfo("WinterAO Resurrection.exe")
                        }.Start());

                        this.Close();

                }
                else{ //Las hay, comenzamos el proceso de actualizacion
                        this.Actualizando = true;
                        this.Actualizar();
                    }
                }
            }

            /**
             * Boton de minimizar
             */
            private void btnMini_Click(object sender, RoutedEventArgs e){
                this.WindowState = WindowState.Minimized;
            }
  }
}
