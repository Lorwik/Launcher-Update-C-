using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher.src
{
    class Networking
    {
        public static string ROOT_PATH = "https://storageao20.blob.core.windows.net/resourcesao20";
        private readonly string VERSION_PATH = ROOT_PATH + "/Version.json";

        private readonly List<string> EXCEPCIONES = new List<string>() {
            "Argentum20\\Recursos\\OUTPUT\\Configuracion.ini",
            "Argentum20\\Recursos\\OUTPUT\\Teclas.ini"
        };

        // Acá está la info. del VersionInfo.json
        public VersionInformation versionRemota; // Acá como objeto de-serializado.
        public string versionRemotaString;      // Acá como texto plano.

        public List<string> fileQueue = new List<string>();
        public TaskCompletionSource<bool> downloadQueue;

        /**
         * Comprueba la ultima version disponible
         */
        public List<string> CheckOutdatedFiles()
        {

            fileQueue.Clear();

            // Obtenemos los datos necesarios del servidor.
            VersionInformation versionRemota = Get_RemoteVersion();

            // Si no existe VersionInfo.json en la carpeta Init, ...
            VersionInformation versionLocal;

            byte[] LauncherHadh;
            string hashConverted;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(Directory.GetCurrentDirectory() + "/Launcher - Argentum20.exe"))
                {                    
                    LauncherHadh = md5.ComputeHash(stream);
                    hashConverted = BitConverter.ToString(LauncherHadh).Replace("-", "").ToLower();
                }
            }

            if (!File.Exists(IO.VERSIONFILE_PATH))
            {
                // ... parseamos el string que obtuvimos del servidor.
                
                versionLocal = IO.Get_LocalVersion(versionRemotaString);
            }
            else // Si existe, ...
            {
                // ... buscamos y parseamos el que está en la carpeta Init.
                versionLocal = IO.Get_LocalVersion(null);
            }

            VersionInformation.File archivoLocal, archivoRemoto;

            //El archivo posicion 0 en el Version.JSON debe ser el launcher para comparar si está actualizado.
            if (hashConverted.ToUpper() != versionRemota.Manifest.LauncherVersion)
            {
                return null;
            }

            // Itero la lista de archivos del servidor y lo comparo con lo que tengo en local.
            for (int i = 0; i < versionRemota.Files.Count; i++)
            {
                archivoLocal = versionLocal.Files[i];
                archivoRemoto = versionRemota.Files[i];               

                //Si existe el archivo

                    //Si está en las excepciones lo omito

                    //



                // Si existe el archivo, comparamos el MD5..
                if (File.Exists(App.ARGENTUM_PATH + "\\" + archivoRemoto.name))
                {
                    // Si NO coinciden los hashes, ...
                    if (!EXCEPCIONES.Contains(archivoRemoto.name))
                    {
                        if (IO.checkMD5(archivoLocal.name) != archivoRemoto.checksum)
                        {
                            // ... lo agrego a la lista de archivos a descargar.
                            fileQueue.Add(archivoRemoto.name);
                        }
                    }

                }
                else // Si existe el archivo, ...
                {
                    // ... lo agrego a la lista de archivos a descargar.
                    fileQueue.Add(archivoRemoto.name);
                }
            }

            // Guardo en un field el objeto de-serializado de la info. remota.
            this.versionRemota = versionRemota;

            return fileQueue;
        }



        public VersionInformation Get_RemoteVersion()
        {
            WebClient webClient = new WebClient();
            VersionInformation versionRemota = null;
            try
            {
                // Envio un GET al servidor con el JSON de el archivo de versionado.
                versionRemotaString = webClient.DownloadString(VERSION_PATH);
                
                // Me fijo que la response NO ESTÉ vacía.
                if (versionRemotaString == null)
                {
                    MessageBox.Show("Hemos recibido una respuesta vacía del servidor. Contacta con un administrador :'(");
                    Environment.Exit(0);
                }

                // Deserializamos el Version.json remoto
                versionRemota = JsonSerializer.Deserialize<VersionInformation>(versionRemotaString);
            }
            catch (WebException error)
            {
                MessageBox.Show(error.Message);
            }
            catch (JsonException)
            {
                MessageBox.Show("Has recibido una respuesta invalida por parte del servidor.");
            }
            finally
            {
                webClient.Dispose();
            }

            return versionRemota;
        }

        public void CrearCarpetasRequeridas()
        {
            foreach(string folder in versionRemota.Folders)
            {
                string currentFolder = App.ARGENTUM_PATH + "\\" + folder;

                if (!Directory.Exists(currentFolder))
                {
                    Directory.CreateDirectory(currentFolder);
                }
            }
        }

        /**
         * ADVERTENCIA: Esto es parte de el método DescargarActualizaciones() en MainWindow.xaml.cs
         *              NO EJECUTAR DIRECTAMENTE, HACERLO A TRAVÉS DE ESE METODO!
         *              
         * Fuente: https://stackoverflow.com/questions/39552021/multiple-asynchronous-download-with-progress-bar
         */
        public async Task IniciarDescarga(WebClient webClient)
        {
            Uri uriDescarga;
            //files contains all URL links
            foreach (string file in fileQueue)
            {
                downloadQueue = new TaskCompletionSource<bool>();
                uriDescarga = new Uri(ROOT_PATH + "/" + file);
                webClient.DownloadFileAsync(uriDescarga, App.ARGENTUM_PATH + file);

                await downloadQueue.Task;
            }
            downloadQueue = null;
        }
    }
}
