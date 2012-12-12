﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using Outliner.Scene;
using System.Globalization;
using Outliner.MaxUtils;
using Outliner.Plugins;
using ManagedServices;

namespace Outliner.Filters
{
[OutlinerPlugin(OutlinerPluginType.Filter)]
[LocalizedDisplayName(typeof(OutlinerResources), "Str_MaxscriptFilter")]
public class MaxscriptFilter : Filter<IMaxNodeWrapper>
{
   public MaxscriptFilter()
   {
      _script = "";
   }

   private const String execFilterTemplate = "( local node = getAnimByHandle {0}; {1} )";

   private String _script;
   private String _filterFn;
   public String Script
   {
      get { return _script; }
      set
      {
         _script = value;
         _filterFn = String.Format(CultureInfo.InvariantCulture, execFilterTemplate, "{0:d}", value);
      }
   }

   protected override Boolean ShowNodeInternal(IMaxNodeWrapper data)
   {
      if (String.IsNullOrEmpty(_script))
         return true;

      IINodeWrapper iinodeWrapper = data as IINodeWrapper;
      if (data == null)
         return false;

      UIntPtr handle = MaxInterfaces.Global.Animatable.GetHandleByAnim(iinodeWrapper.IINode);
      String script = String.Format(CultureInfo.InvariantCulture, _filterFn, handle);
      return MaxscriptSDK.ExecuteBooleanMaxscriptQuery(script);
   }
}
}