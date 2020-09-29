using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace Launcher.src
{
    class IO
    {
        public bool Actualizando = false;
        public int ArchivosDesactualizados = 0;
        public int ArchivoActual = 0;

        public static string VERSIONFILE_PATH = Directory.GetCurrentDirectory() + "\\Init\\VersionInfo2.json";

        public static VersionInformation Get_LocalVersion()
        {
            // Leemos el VersionIndo.json local
            StreamReader localStreamReader = null;
            string localFile = null;
            try
            {
                localStreamReader = new StreamReader(VERSIONFILE_PATH);
                localFile = localStreamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                localStreamReader.Close();
            }

            // Deserializamos el Version.json local
            VersionInformation versionLocal = null;
            try
            {
                versionLocal = JsonConvert.DeserializeObject<VersionInformation>(localFile);
            }
            catch (JsonException)
            {
                MessageBox.Show("Error al de-serializar: El Version.json tiene un formato inválido.");
            }

            return versionLocal;
        }

        /**
         * Calcula el checksum MD5
         */
        public static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Directory.GetCurrentDirectory() + filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        /**
         * Guarda la ultima version del VersionInfo.json una vez que terminó de actualizarse todo.
         */
        public static void SaveLatestVersionInfo(string versionInfo)
        {
            File.WriteAllText(VERSIONFILE_PATH, versionInfo);
        }
    }
}
