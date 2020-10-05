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
        public static string HOST = "https://winterao.com.ar/update/cliente/";
        private readonly string VERSIONFILE_URI = HOST + "VersionInfo2.json";

        private readonly List<string> EXCEPCIONES = new List<string>() { 
            "/Init/Config.ini",
            "/Init/BindKeys.bin"
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
            // Obtenemos los datos necesarios del servidor.
            VersionInformation versionRemota = Get_RemoteVersion();

            // Si no existe VersionInfo.json en la carpeta Init, ...
            VersionInformation versionLocal;
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

            // Itero la lista de archivos del servidor y lo comparo con lo que tengo en local.
            for (int i = 0; i < versionRemota.Files.Count; i++)
            {
                // Si existe el archivo, comparamos el MD5..
                if (File.Exists(Directory.GetCurrentDirectory() + versionRemota.Files[i].name))
                {
                    // Si NO coinciden los hashes, ...
                    if (!EXCEPCIONES.Contains(versionRemota.Files[i].name) && 
                        IO.checkMD5(versionLocal.Files[i].name) != versionRemota.Files[i].checksum)
                    {
                        // ... lo agrego a la lista de archivos a descargar.
                        fileQueue.Add(versionRemota.Files[i].name);
                    }
                }
                else // Si existe el archivo, ...
                {
                    // ... lo agrego a la lista de archivos a descargar.
                    fileQueue.Add(versionRemota.Files[i].name);
                }
            }

            // Guardo en un field el objeto de-serializado de la info. remota.
            this.versionRemota = versionRemota;

            return fileQueue;
        }

        public VersionInformation Get_RemoteVersion()
        {
            // Envio un GET al servidor con el JSON de el archivo de versionado.
            WebClient webClient = new WebClient();
            try
            {
                versionRemotaString = webClient.DownloadString(VERSIONFILE_URI);
            }
            catch (WebException error)
            {
                MessageBox.Show(error.Message);
            }
            finally
            {
                webClient.Dispose();
            }

            // Me fijo que la response NO ESTÉ vacía.
            if (versionRemotaString == null)
            {
                MessageBox.Show("Hemos recibido una respuesta vacía del servidor. Contacta con un administrador :'(");
                Environment.Exit(0);
            }

            // Deserializamos el Version.json remoto
            VersionInformation versionRemota = null;
            try
            {
                versionRemota = JsonSerializer.Deserialize<VersionInformation>(versionRemotaString);
            }
            catch (JsonException)
            {
                MessageBox.Show("Error al de-serializar: El Version.json del servidor tiene un formato inválido.");
            }

            return versionRemota;
        }

        public void CrearCarpetasRequeridas()
        {
            foreach(string folder in versionRemota.Folders)
            {
                string currentFolder = Directory.GetCurrentDirectory() + folder;

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
            //files contains all URL links
            foreach (string file in fileQueue)
            {
                downloadQueue = new TaskCompletionSource<bool>();

                webClient.DownloadFileAsync(new Uri(HOST + file), Directory.GetCurrentDirectory() + file);

                await downloadQueue.Task;
            }
            downloadQueue = null;
        }
    }
}
