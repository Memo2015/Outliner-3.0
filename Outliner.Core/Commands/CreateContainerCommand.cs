﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using Outliner.MaxUtils;
using Outliner.Scene;

namespace Outliner.Commands
{
   public class CreateContainerCommand : Command
   {
      private IEnumerable<IMaxNodeWrapper> nodes;
      private IINode containerNode;

      public CreateContainerCommand() : this(Enumerable.Empty<IMaxNodeWrapper>()) { }
      public CreateContainerCommand(IEnumerable<IMaxNodeWrapper> nodes)
      {
         Throw.IfArgumentIsNull(nodes, "nodes");

         this.nodes = nodes;
      }

      public override string Description
      {
         get
         {
            return OutlinerResources.Command_AddToNewContainer;
         }
      }

      protected override void Do()
      {
         this.containerNode = MaxInterfaces.ContainerManager.CreateContainer(HelperMethods.ToIINodeTab(this.nodes));
         this.containerNode.SetAFlag(AnimatableFlags.Held);
      }

      protected override void Undo()
      {
         if (this.containerNode != null)
         {
            MaxInterfaces.COREInterface.DeleteNode(this.containerNode, false, false);
            this.containerNode = null;
         }
      }
   }
}