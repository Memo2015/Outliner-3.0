﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PJanssen.Outliner.Scene;
using Autodesk.Max;
using PJanssen.Outliner.Plugins;
using PJanssen.Outliner.Filters;

namespace PJanssen.Outliner.ColorTags
{
   [OutlinerPlugin(OutlinerPluginType.Filter)]
   [LocalizedDisplayName(typeof(Resources), "Filter_ColorTag")]
   public class ColorTagsFilter : Filter<IMaxNode>
   {
      private ColorTag tags;

      public ColorTagsFilter() : this(ColorTag.All) { }
      public ColorTagsFilter(ColorTag tags)
      {
         this.tags = tags;
      }

      public ColorTag Tags
      {
         get { return this.tags; }
         set
         {
            this.tags = value;
            this.OnFilterChanged();
         }
      }

      protected override Boolean ShowNodeInternal(IMaxNode data)
      {
         if (data == null)
            return false;

         return (data.GetColorTag() & this.tags) != 0;
      }
   }
}
