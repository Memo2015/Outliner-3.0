﻿using System;
using Autodesk.Max;
using Outliner.Scene;
using Outliner.Plugins;
using Outliner.MaxUtils;

namespace Outliner.Filters
{
   [OutlinerPlugin(OutlinerPluginType.Filter)]
   [LocalizedDisplayName(typeof(Resources), "Filter_Hidden")]
   [LocalizedDisplayImage(typeof(Resources), "hide")]
   [FilterCategory(FilterCategories.Properties)]
   public class HiddenFilter : NodePropertyFilter
   {
      public HiddenFilter() : base(BooleanNodeProperty.IsHidden)
      {
         this.Invert = true;
      }
   }
}