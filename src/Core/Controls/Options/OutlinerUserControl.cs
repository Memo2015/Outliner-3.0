﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using PJanssen.Outliner.MaxUtils;
using Autodesk.Max;

namespace PJanssen.Outliner.Controls.Options
{
public class OutlinerUserControl : UserControl
{
   public OutlinerUserControl() { }

   protected override void OnLoad(EventArgs e)
   {
      ControlHelpers.Set3dsMaxControlColors(this);
      base.OnLoad(e);
   }
}
}
