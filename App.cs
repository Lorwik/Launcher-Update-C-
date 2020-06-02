// Decompiled with JetBrains decompiler
// Type: Uplauncher.App
// Assembly: Uplauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 29525C92-FB0F-4435-AC09-51B78CB2F18A
// Assembly location: C:\Users\Pablo\Desktop\Dofus Decay 2.43 BETA\Launcher Dofus Decay.exe

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace Uplauncher
{
  public class App : Application
  {
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }

    [STAThread]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public static void Main()
    {
      App app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
