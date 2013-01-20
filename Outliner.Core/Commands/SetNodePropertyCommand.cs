﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outliner.MaxUtils;
using Outliner.Scene;
using System.Reflection;
using Autodesk.Max.Plugins;
using Autodesk.Max;

namespace Outliner.Commands
{
public class SetNodePropertyCommand<T> : CustomRestoreObjCommand
{
   private IEnumerable<IMaxNode> nodes;
   private NodeProperty property;
   private T newValue;
   private IEnumerable<Tuple<IMaxNode, object>> oldValues;

   public SetNodePropertyCommand(IEnumerable<IMaxNode> nodes, NodeProperty property, T newValue)
   {
      Throw.IfArgumentIsNull(nodes, "nodes");

      this.nodes = nodes.ToList();
      this.property = property;
      this.newValue = newValue;
   }

   public override string Description
   {
      get { return OutlinerResources.Command_SetProperty; }
   }

   public override void Redo()
   {
      this.oldValues = this.nodes.Select(n => new Tuple<IMaxNode, object>(n, n.GetNodeProperty(this.property)))
                                 .ToList();
      this.nodes.ForEach(n => n.SetNodeProperty(this.property, this.newValue));
   }

   public override void Restore(bool isUndo)
   {
      foreach (Tuple<IMaxNode, object> oldValue in this.oldValues)
      {
         oldValue.Item1.SetNodeProperty(this.property, oldValue.Item2);
      }
   }
}
}
