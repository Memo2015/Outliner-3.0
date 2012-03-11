﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outliner.Scene;

namespace Outliner.Commands
{
public class RenameCommand : SetNodePropertyCommand<String>
{
   public RenameCommand(IEnumerable<IMaxNodeWrapper> nodes, String newName)
      : base(nodes, newName) { }

   public override string Description
   {
      get { return OutlinerResources.Command_Rename; }
   }

   public override string GetValue(IMaxNodeWrapper node)
   {
      return node.Name;
   }

   public override void SetValue(IMaxNodeWrapper node, string value)
   {
      node.Name = value;
   }
}
}
