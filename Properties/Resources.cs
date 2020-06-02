// Decompiled with JetBrains decompiler
// Type: Uplauncher.Properties.Resources
// Assembly: Uplauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 29525C92-FB0F-4435-AC09-51B78CB2F18A
// Assembly location: C:\Users\Pablo\Desktop\Dofus Decay 2.43 BETA\Launcher Dofus Decay.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Uplauncher.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (Uplauncher.Properties.Resources.resourceMan == null)
          Uplauncher.Properties.Resources.resourceMan = new ResourceManager("Uplauncher.Properties.Resources", typeof (Uplauncher.Properties.Resources).Assembly);
        return Uplauncher.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return Uplauncher.Properties.Resources.resourceCulture;
      }
      set
      {
        Uplauncher.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
