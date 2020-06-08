// AO Libre C# Launcher by Pablo M. Duval (Discord: Abusivo#1215)
// Este launcher y todo su contenido incluyendo sus códigos son de uso público y gratuito.

using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Uplauncher
{

    /*
     * Clase utilizada para des-serializar el Version.json del servidor.
     */
    class VersionInformation
    {
        public double patches { get; set; }
        public string[] checksums { get; set; }
    }

    public partial class MainWindow : Window, IComponentConnector
    {

        //ATRIBUTOS
        private VersionInformation versionRemota;   //Version actual del cliente remoto nos dice si hay parches
        private VersionInformation versionLocal;    //Version actual Local del clienteo

        private double ParcheActual = 0;            //Indica el parche en el que nos encontramos actualmente
        private bool UpdatePendiente;               //Bandera que nos indica si tenemos actualizaciones pendientes
        private bool Actualizando = false;          //Bandera que nos indica si estamos en proceso de actualizacion
        
        private readonly string URLWeb = "http://winterao.com.ar";
        private readonly string URLWiki = "http://winterao.com.ar/wiki/";
        private readonly string LocalVersionFile = "\\Init\\Version.json";
        private readonly string RemoteVersionFile = "/update/Version.json";

        private readonly string LocalVersionFilePath = Directory.GetCurrentDirectory() + "\\Init\\Version.json";

        //METODOS

        /**
         * Main
         */
        public MainWindow()
        {
            this.InitializeComponent();

            //Comprobamos la version actual del cliente
            if (this.VerifyVersion())
            {
                this.UpdatePendiente = false;
                this.pbar.Value = 100.0;
                this.lblDow.Content = (object)"Actualizado";
                this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));

            }
            else
            { //Si el cliente no esta actualizado, lo notificamos
                this.UpdatePendiente = true;
                this.lblDow.Content = (object)"Tienes actualizaciones pendientes por descargar...";
                this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }

            //Guardamos el parche actual
            this.ParcheActual = this.versionLocal.patches;

        }

        /**
         * Inicia el proceso de actualizacion del cliente
         */
        private void Actualizar()
        {
            //¿El parche actual coincide con la version remota?
            if (Convert.ToInt32(this.ParcheActual) < Convert.ToInt32(this.versionRemota.patches))
            {
                // Incrementamos el contador de parches en 1 para descargar la actualizacion siguiente
                this.ParcheActual++;

                //Anunciamos el parche que estamos descargando
                this.lblDow.Content = "Descargando actualizacion " + this.ParcheActual;

                //Descargamos e instalamos
                string DownloadTo = Directory.GetCurrentDirectory() + "\\update" + this.ParcheActual + ".zip";
                this.lblDow.Content = (object)"Actualizando...";
                this.Download(URLWeb + "/update/update" + this.ParcheActual + ".zip", DownloadTo);

                lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#0092D6"));

            }
            else
            {

                MessageBox.Show("¡WinterAO ha sido actualizado! Ahora puedes jugar a la ultima versión.", "¡Enhorabuena!");
                this.Actualizando = false;
                this.lblDow.Content = (object)"Actualizado";
                this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));
            }
        }

        /**
         * Comprueba la ultima version disponible
         */
        private bool VerifyVersion()
        {
            // Leemos el Version.dat local
            StreamReader localStreamReader = null;
            string localFile = null;
            try
            {
                localStreamReader = new StreamReader(LocalVersionFilePath);
                localFile = localStreamReader.ReadToEnd();
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ErrorLog(ex);
            }
            finally
            {
                localStreamReader.Close();
            }

            // Deserializamos el Version.json local
            try
            {
                this.versionLocal = JsonSerializer.Deserialize<VersionInformation>(localFile);

                // Seteo el parche actual
                this.ParcheActual = this.versionLocal.patches;
            }
            catch (JsonException ex)
            {
                MessageBox.Show("Error al de-serializar: El Version.json tiene un formato inválido.");
                ErrorLog(ex);
            }

            Stream responseStream = null;
            StreamReader remoteStreamReader = null;
            string remoteFile = null;
            try
            {
                // Obtenemos el archivo del servidor
                responseStream = WebRequest.Create(new Uri(URLWeb + RemoteVersionFile)).GetResponse().GetResponseStream();

                // Leemos el Version.json remoto
                remoteStreamReader = new StreamReader(responseStream);
                remoteFile = remoteStreamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ErrorLog(ex);
            }
            finally
            {
                remoteStreamReader.Close();
                responseStream.Close();
            }

            // Deserializamos el Version.json remoto
            try
            {
                this.versionRemota = JsonSerializer.Deserialize<VersionInformation>(remoteFile);
            }
            catch (JsonException ex)
            {
                MessageBox.Show("Error al de-serializar: El Version.json del servidor tiene un formato inválido.");
                ErrorLog(ex);
            }

            // Finalmente, hacemos la comparación de versiones.
            return this.versionRemota.patches == this.ParcheActual;
        }

        /**
         * Descarga la actualizacion
         */
        private void Download(string Url, string DownloadTo)
        {
            WebClient webClient = null;
            try
            {
                webClient = new WebClient();
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.UpdateProgressChange);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.UpdateDone);
                webClient.DownloadFileAsync(new Uri(Url), DownloadTo);
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
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
         * Extrae el .zip
         */
        private void MyExtract()
        {
            string str = Directory.GetCurrentDirectory() + "\\update" + ParcheActual + ".zip";
            string baseDirectory = Directory.GetCurrentDirectory();

            if (!File.Exists(str)) return;

            using (ZipFile zipFile = ZipFile.Read(str))
            {
                foreach (ZipEntry zipEntry in zipFile)
                    zipEntry.Extract(baseDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
            try
            {
                System.IO.File.Delete(str);

                //Volvemos a llamar al Actualizar
                this.Actualizar();

            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /**
         * Calcula el checksum MD5
         */
        public string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        /**
         * Actualiza la barra de progreso
         */
        private void UpdateProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            this.pbar.Value = (double)e.ProgressPercentage;

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

            string file = Directory.GetCurrentDirectory() + LocalVersionFile;

            string checksumArchivoLocal = checkMD5(Directory.GetCurrentDirectory() + "\\update" + ParcheActual + ".zip");
            string checksumArchivoRemoto = this.versionRemota.checksums[Convert.ToInt32(this.ParcheActual)];

            if (checksumArchivoLocal != checksumArchivoRemoto)
            {
                string error = "El MD5 de esta actualización NO COINCIDE!" + "\r\n" +
                                "Archivo: update" + ParcheActual + "\r\n" +
                                "MD5 Local:" + checksumArchivoRemoto + "\r\n" +
                                "MD5 Remoto: " + checksumArchivoRemoto + "\r\n\r\n" + 
                                "¿Desea intentar descargar la actualización una vez mas?";

                MessageBoxResult badUpdate = MessageBox.Show(error, "Descarga Corrupta", MessageBoxButton.YesNo);

                // Le preguntamos si quiere descargar la actualización 1 vez mas.
                if(badUpdate == MessageBoxResult.Yes)
                {
                    // Borramos la actualización corrupta.
                    File.Delete(Directory.GetCurrentDirectory() + "update" + this.ParcheActual + ".zip");

                    // Decrementamos el contador de parche actual en 1 para volver a descargar la actualización.
                    --this.ParcheActual;

                    // Salimos del método.
                    Actualizar();
                    return;
                }
                else
                {
                    // Si elige no volver a descargar la actualización, cerramos el launcher.
                    Environment.Exit(0);
                }

            }

            //Extraemos la actualización.
            this.MyExtract();
            try
            {
                File.Delete(file);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                ErrorLog(ex);
            }

            // Guardamos el Version.json del servidor en la carpeta Init
            using (FileStream fs = File.Create(file))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                
                JsonSerializer.SerializeAsync(fs, this.versionRemota, options);

                fs.Close();
            }
        }

        /**
         * Boton para ir a la web
         */
        private void btnSitio_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(URLWeb);
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
         * Boton de jugar
         */
        private void btnJugar_Click(object sender, RoutedEventArgs e)
        {
            if (this.Actualizando == false)
            {
                //¿Hay actualizaciones pendientes?
                if (this.UpdatePendiente == false)
                {
                    if (!File.Exists("WinterAO Resurrection.exe"))
                    {
                        MessageBox.Show("No se ha encontrado el ejecutable de WinterAO Resurrection!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                    else if (new Process()
                    {
                        StartInfo = new ProcessStartInfo("WinterAO Resurrection.exe")
                    }.Start())

                    this.Close();

                }
                else
                { //Las hay, comenzamos el proceso de actualizacion
                    this.Actualizando = true;
                    this.Actualizar();
                }
            }
        }

        /**
         * Boton de minimizar
         */
        private void btnMini_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public static void ErrorLog(Exception ex)
        {
            string strPath = Directory.GetCurrentDirectory();
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine("============= Registro de Errores ===========");
                sw.WriteLine("===========Inicio============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine("===========Fin============= " + DateTime.Now);

            }
        }
    }
}
