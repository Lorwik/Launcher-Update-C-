// Decompiled with JetBrains decompiler
// Type: Launcher.App
// Assembly: Launcher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 29525C92-FB0F-4435-AC09-51B78CB2F18A
// Assembly location: C:\Users\Pablo\Desktop\Dofus Decay 2.43 BETA\Launcher Dofus Decay.exe

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace Launcher
{
    public class App : Application
    {
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            this.StartupUri = new Uri("src/MainWindow.xaml", UriKind.Relative);
        }

        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {

            // Si detectamos otra instancia de la aplicación, la cerramos.
            bool onlyInstance = false;
            Mutex mutex = new Mutex(true, @"Global\WinterAO_Launcher", out onlyInstance);
            if (!onlyInstance)
            {
                MessageBox.Show("El launcher ya esta abierto.", "Error", MessageBoxButton.OK);
                Environment.Exit(0);
            }

            try
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();

                // Prevenimos que el GarbageCollector limpie el Mutex si la aplicación se ejecuta durante mucho tiempo.
                GC.KeepAlive(mutex);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
