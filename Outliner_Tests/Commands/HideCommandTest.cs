﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner.Scene;
using Outliner.Commands;

namespace Outliner_Tests.Commands
{
[TestClass]
public class HideCommandTest : MaxIntegrationTest
{
   [TestMethod]
   public void HideTest()
   {
      IINodeWrapper node = IMaxNodeWrapper.Create(MaxRemoting.CreateBox()) as IINodeWrapper;
      Assert.IsNotNull(node);
      IILayerWrapper layer = IMaxNodeWrapper.Create(MaxRemoting.CreateLayer()) as IILayerWrapper;
      Assert.IsNotNull(layer);

      Boolean nodeHidden = node.IsHidden;
      Boolean layerHidden = layer.IsHidden;
      List<IMaxNodeWrapper> nodes = new List<IMaxNodeWrapper>(2) { node, layer };
      HideCommand cmd = new HideCommand(nodes, !nodeHidden);

      cmd.Do();
      Assert.AreEqual(!nodeHidden, node.IsHidden);
      Assert.AreEqual(!layerHidden, layer.IsHidden);

      cmd.Undo();
      Assert.AreEqual(nodeHidden, node.IsHidden);
      Assert.AreEqual(layerHidden, layer.IsHidden);
   }
}
}