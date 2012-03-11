﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Outliner.Scene;
using Outliner.Commands;

namespace Outliner.Controls.Layout
{
   public class FreezeButton : ImageButton
   {
      public FreezeButton() : base(OutlinerResources.freeze_button,
                                   OutlinerResources.freeze_button_disabled)
      { }

      public override bool IsEnabled(TreeNode tn)
      {
         IMaxNodeWrapper node = HelperMethods.GetMaxNode(tn);
         if (node == null)
            return false;

         return node.IsFrozen;
      }

      public override void HandleMouseUp(MouseEventArgs e, TreeNode tn)
      {
         if (this.Layout == null || this.Layout.TreeView == null)
            return;

         IMaxNodeWrapper node = HelperMethods.GetMaxNode(tn);
         if (node == null)
            return;

         TreeView tree = this.Layout.TreeView;
         IEnumerable<IMaxNodeWrapper> nodes = null;
         if (HelperMethods.ControlPressed && tree.IsSelectedNode(tn))
            nodes = HelperMethods.GetMaxNodes(tree.SelectedNodes);
         else
            nodes = new List<IMaxNodeWrapper>(1) { node };

         FreezeCommand cmd = new FreezeCommand(nodes, !node.IsFrozen);
         cmd.Execute(true);
      }
   }
}
