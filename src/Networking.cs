using System.Net;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using System;

namespace Launcher.src
{
    class Networking
    {
        public static string HOST = "https://winterao.com.ar/update/cliente/";
        private readonly string VERSIONFILE_URI = HOST + "VersionInfo2.json";
        public VersionInformation versionRemota;

        private static WebClient webClient = new WebClient();
        public List<string> fileQueue = new List<string>();

        /**
         * Comprueba la ultima version disponible
         */
        public List<string> CheckOutdatedFiles()
        {
            VersionInformation versionLocal = IO.Get_LocalVersion();
            versionRemota = Get_RemoteVersion();

            // OFF-TOPIC: Creo las carpetas necesarias para descargar las cosas.
            for (int j = 0; j < versionRemota.Folders.Count; j++) {
                string currentFolder = Directory.GetCurrentDirectory() + versionRemota.Folders[j];
                
                if (!Directory.Exists(currentFolder))
                {
                    Directory.CreateDirectory(currentFolder);
                }
            }

            // Itero la lista de archivos del servidor y lo comparo con lo que tengo en local.
            for(int i = 0; i < versionRemota.Files.Count; i++)
            {
                string localFile = versionLocal.Files[i].name;

                // Si existe el archivo, comparamos el MD5..
                if (File.Exists(localFile))
                {
                    // Si NO coinciden los hashes, ...
                    if (IO.checkMD5(localFile) != versionRemota.Files[i].checksum)
                    {
                        // ... lo agrego a la lista de archivos a descargar.
                        fileQueue.Add(localFile);
                    }
                }
                else // Si existe el archivo, ...
                {
                    // ... lo agrego a la lista de archivos a descargar.
                    fileQueue.Add(localFile);
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
                Environment.Exit(0);
            }

            // Serializo en un POJO lo que recibí.
            // Deserializamos el Version.json remoto
            VersionInformation versionRemota = null;
            try {
                versionRemota = JsonConvert.DeserializeObject<VersionInformation>(response);
            } catch (JsonException) {
                MessageBox.Show("Error al de-serializar: El Version.json del servidor tiene un formato inválido.");
            }

            return versionRemota;
        }
    }
}
