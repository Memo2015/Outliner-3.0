﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PJanssen.Outliner
{
   internal static class NativeMethods
   {
      [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
      public static extern int StrCmpLogicalW(String x, String y);

      [DllImport("user32.dll")]
      public static extern IntPtr SetFocus(IntPtr hWnd);
   }
}
