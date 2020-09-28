using System.Net;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using System.Windows.Documents;
using System.Collections.Generic;

namespace Launcher.src
{
    class Networking
    {
        private static WebClient webClient = new WebClient();

        private static readonly string VERSIONFILE_URI = "https://winterao.com.ar/update/cliente/VersionInfo2.json";

        public List<string> fileQueue = new List<string>();

        /**
         * Comprueba la ultima version disponible
         */
        public List<string> VerifyVersion()
        {
            VersionInformation versionLocal = IO.Get_LocalVersion();
            VersionInformation versionRemota = Get_RemoteVersion();

            for(var i = 0; i < versionRemota.Files.Count; i++)
            {
                string localFileVersion = Directory.GetCurrentDirectory() + versionLocal.Files[i].name;

                // Si existe el archivo, comparamos el MD5..
                if (File.Exists(localFileVersion))
                {
                    // Si NO coinciden los hashes, ...
                    if (IO.checkMD5(localFileVersion) != versionRemota.Files[i].checksum)
                    {
                        // ... lo agrego a la lista de archivos a descargar.
                        fileQueue.Add(versionLocal.Files[i].name);
                    }
                }
                else // Si existe el archivo, ...
                {
                    // ... lo agrego a la lista de archivos a descargar.
                    fileQueue.Add(versionLocal.Files[i].name);
                }
            }

            return fileQueue;
        }

        public VersionInformation Get_RemoteVersion()
        {
            // Envio un GET al servidor con el JSON de el archivo de versionado.
            string response = null;
            try {
                response = webClient.DownloadString(VERSIONFILE_URI);
            } catch (WebException error) {
                MessageBox.Show(error.Message);
            } finally {
                webClient.Dispose();
            }

            // Me fijo que la response NO ESTÉ vacía.
            if (response == null)
            {
                MessageBox.Show("Hemos recibido una respuesta vacía del servidor. Contacta con un administrador :'(");
                return null;
            }

            // Serializo en un POJO lo que recibí.
            // Deserializamos el Version.json remoto
            VersionInformation versionRemota = null;
            try {
                versionRemota = JsonSerializer.Deserialize<VersionInformation>(response);
            } catch (JsonException) {
                MessageBox.Show("Error al de-serializar: El Version.json del servidor tiene un formato inválido.");
            }

            return versionRemota;
        }
    }
}
