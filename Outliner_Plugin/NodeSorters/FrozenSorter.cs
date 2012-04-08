﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outliner.Scene;
using Outliner.Controls.Tree;

namespace Outliner.NodeSorters
{
   public class FrozenSorter : NodeSorter
   {
      public FrozenSorter() : base() { }
      public FrozenSorter(Boolean invert) : base(invert) { }

      protected override int InternalCompare(TreeNode x, TreeNode y)
      {
         if (x == y)
            return 0;

         IMaxNodeWrapper nodeX = HelperMethods.GetMaxNode(x);
         if (nodeX == null || !nodeX.IsValid) return 0;

         IMaxNodeWrapper nodeY = HelperMethods.GetMaxNode(y);
         if (nodeY == null || !nodeY.IsValid) return 0;

         if (nodeX.IsFrozen == nodeY.IsFrozen)
            return NativeMethods.StrCmpLogicalW(nodeX.Name, nodeY.Name);
         else if (nodeX.IsFrozen)
            return 1;
         else
            return -1;
      }
   }
}
