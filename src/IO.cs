﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;

namespace Launcher.src
{
    class IO
    {
        public bool Actualizando = false;
        public int ArchivosDesactualizados = 0;
        public int ArchivoActual = 0;

        public static string VERSIONFILE_PATH = App.ARGENTUM_PATH + App.SERVER_SELECT + "\\VersionInfo.json";

#nullable enable
        public static VersionInformation Get_LocalVersion(string? customVersionData)
        {

            // Acá guardo el string que obtengo ya sea del parámetro o del archivo en la carpeta Init.
#pragma warning disable CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL
            string data = customVersionData;
#pragma warning restore CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL

            // Si no damos un string custom, lo leemos de init/VersionInfo.json
            if (customVersionData == null)
            {
#nullable disable
                StreamReader localStreamReader = null;
                try
                {
                    if (File.Exists(VERSIONFILE_PATH))
                    {
                        localStreamReader = new StreamReader(VERSIONFILE_PATH);
                        data = localStreamReader.ReadToEnd();
                        localStreamReader.Close();
                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            // Deserializamos el VersionInfo.json local
            VersionInformation versionLocal = null;
            try
            {
                if (data != null)
                    versionLocal = JsonSerializer.Deserialize<VersionInformation>(data);
            }
            catch (JsonException)
            {
                MessageBox.Show("Error al de-serializar: El VersionInfo.json tiene un formato inválido.");
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
                using (var stream = File.OpenRead(App.ARGENTUM_PATH + filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
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
