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
    private string versionActual;
    private string versionCliente;

    //METODOS

    /**
     * Main
     */
    public MainWindow(){
      this.InitializeComponent();
      this.btnJugar.IsEnabled = false;

       //Comprobamos la version actual del cliente
      if (this.VerifyVersion()){
        this.pbar.Value = 100.0;
        this.btnJugar.IsEnabled = true;
        this.lblDow.Content = (object) "Actualizado";
        this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));

      }else{
        string DownloadTo = Directory.GetCurrentDirectory() + "\\update" + versionActual + ".zip";
        this.lblDow.Content = (object) "Actualizando...";
        this.Download("http://winterao.com.ar/update/update" + versionActual + ".zip", DownloadTo);
        lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#0092D6"));
      }
    }

    /**
     * Comprueba la ultima version disponible
     */
    private bool VerifyVersion(){
      StreamReader streamReader = new StreamReader(Directory.GetCurrentDirectory() + "\\version.txt");
      string end = streamReader.ReadToEnd();
      this.versionActual = this.ReadRemoteTextFile("http://winterao.com.ar/update/version.txt");
      streamReader.Close();

      return this.versionActual == end;
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
     * Extra el .zip
     */
    private void MyExtract(){
      string str = Directory.GetCurrentDirectory() + "\\update" + versionActual + ".zip";
      string baseDirectory = Directory.GetCurrentDirectory();

      if (!System.IO.File.Exists(str))
        return;
      using (ZipFile zipFile = ZipFile.Read(str))
      {
        foreach (ZipEntry zipEntry in zipFile)
          zipEntry.Extract(baseDirectory, ExtractExistingFileAction.OverwriteSilently);
      }
      try{
        System.IO.File.Delete(str);
      }catch (IOException ex){
        int num = (int) MessageBox.Show(ex.Message);
      }
    }

    /**
     * Actualiza la barra de progreso
     */
    private void UpdateProgressChange(object sender, DownloadProgressChangedEventArgs e){
      this.pbar.Value = (double) e.ProgressPercentage;
      if (this.pbar.Value != 100.0)
        return;
      this.MyExtract();
    }

    /**
     * Completa la actualizacion
     */
    private void UpdateDone(object sender, AsyncCompletedEventArgs e){

      int num1 = (int) MessageBox.Show("¡WinterAO ha sido actualizado! Ahora puedes jugar a la ultima versión.", "Notification");
      this.btnJugar.IsEnabled = true;
      this.lblDow.Content = (object) "Actualizado";
      this.lblDow.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D62D"));

      try{
        System.IO.File.Delete(Directory.GetCurrentDirectory() + "\\version.txt");
      }catch (IOException ex){
        int num2 = (int) MessageBox.Show(ex.Message);
      }

      StreamWriter streamWriter = new StreamWriter(Directory.GetCurrentDirectory() + "\\version.txt");
      streamWriter.Write(this.versionActual);
      streamWriter.Close();
    }

    /**
     * Boton para ir a la web
     */
    private void btnSitio_Click(object sender, RoutedEventArgs e){
      Process.Start("https://winterao.com.ar/");
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
      /*if (!System.IO.File.Exists("AH.dll"))
      {
        int num1 = (int) MessageBox.Show("No se ha podido encontrar el antihack!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
      }*/
      if (!System.IO.File.Exists("WinterAO Resurrection.exe"))
      {
        int num2 = (int) MessageBox.Show("No se ha encontrado el ejecutable de WinterAO Resurrection!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
      }
      else if (new Process()
        {
          StartInfo = new ProcessStartInfo("WinterAO Resurrection.exe")
        }.Start())
          ;
    }

    /**
     * Boton de minimizar
     */
    private void btnMini_Click(object sender, RoutedEventArgs e){
      this.WindowState = WindowState.Minimized;
    }
  }
}
