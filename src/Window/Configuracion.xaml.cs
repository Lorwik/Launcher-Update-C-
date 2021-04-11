using IniParser;
using IniParser.Model;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Launcher.src
{
    /// <summary>
    /// Lógica de interacción para Configuracion.xaml
    /// </summary>
    public partial class Configuracion : Window
    {
        AOCfg AOCfg = new AOCfg();
        private static string CONFIG_FILE = App.ARGENTUM_PATH + "Argentum20\\Recursos\\OUTPUT\\Configuracion.ini";
        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
        
        public Configuracion()
        {
            InitializeComponent();
            cargarConfiguraciones();
            actualizarCheckboxs();
        }

        private void cargarConfiguraciones()
        {
            var parser = new FileIniDataParser();
            IniData file = parser.ReadFile(CONFIG_FILE);

            AOCfg.PantallaCompleta = Convert.ToBoolean(Convert.ToInt32(file["VIDEO"]["PantallaCompleta"]));
            AOCfg.PrecargaGrafica = Convert.ToBoolean(Convert.ToInt32(file["VIDEO"]["UtilizarPreCarga"]));
            AOCfg.PunterosGraficos = Convert.ToBoolean(Convert.ToInt32(file["VIDEO"]["CursoresGraficos"]));
            AOCfg.VSync = Convert.ToBoolean(Convert.ToInt32(file["VIDEO"]["VSync"]));

            AOCfg.Musica = Convert.ToBoolean(Convert.ToInt32(file["AUDIO"]["Musica"]));
            AOCfg.Efectos = Convert.ToBoolean(Convert.ToInt32(file["AUDIO"]["Fx"]));
        }

        private void actualizarCheckboxs()
        {
            chkPantallaCompleta.IsChecked = AOCfg.PantallaCompleta;
            chkEfectos.IsChecked = AOCfg.Efectos;
            chkMusica.IsChecked = AOCfg.Musica;
            chkPrecargaGrafica.IsChecked = AOCfg.PrecargaGrafica;
            chkPunterosGraficcos.IsChecked = AOCfg.PunterosGraficos;
            chkSincronizacionVertical.IsChecked = AOCfg.VSync;
        }

        private void btnAceptar_click(object sender, RoutedEventArgs e)
        {
            WritePrivateProfileString("VIDEO", "PantallaCompleta", Convert.ToInt32(chkPantallaCompleta.IsChecked).ToString(), CONFIG_FILE);
            WritePrivateProfileString("VIDEO", "UtilizarPreCarga", Convert.ToInt32(chkPrecargaGrafica.IsChecked).ToString(), CONFIG_FILE);
            WritePrivateProfileString("VIDEO", "CursoresGraficos", Convert.ToInt32(chkPunterosGraficcos.IsChecked).ToString(), CONFIG_FILE);
            WritePrivateProfileString("VIDEO", "VSync", Convert.ToInt32(chkSincronizacionVertical.IsChecked).ToString(), CONFIG_FILE);
            WritePrivateProfileString("AUDIO", "Musica", Convert.ToInt32(chkMusica.IsChecked).ToString(), CONFIG_FILE);
            WritePrivateProfileString("AUDIO", "Fx", Convert.ToInt32(chkEfectos.IsChecked).ToString(), CONFIG_FILE);

            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class AOCfg
    {
        // Video
        public bool PantallaCompleta { get; set; }
        public bool PunterosGraficos { get; set; }
        public bool PrecargaGrafica { get; set; }
        public bool VSync { get; set; }
        // Audio
        public bool Efectos { get; set; }
        public bool Musica { get; set; }
    }
}
