﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using Outliner.MaxUtils;
using Outliner.Commands;
using Outliner.Scene;

namespace Outliner.Controls.ContextMenu
{
public class NodePropertyMenuItemModel : MenuItemModel
{
   public NodePropertyMenuItemModel() : base() 
   {
      this.Property = BooleanNodeProperty.None;
   }

   public NodePropertyMenuItemModel( String text, String image, Type resType
                              , BooleanNodeProperty property) : base(text, image, resType)
   {
      this.Property = property;
   }

   [XmlAttribute("property")]
   [DefaultValue(BooleanNodeProperty.None)]
   public BooleanNodeProperty Property { get; set; }


   protected override Boolean Enabled( Outliner.Controls.Tree.TreeView treeView
                                     , Outliner.Controls.Tree.TreeNode clickedTn)
   {
      ExceptionHelpers.ThrowIfArgumentIsNull(treeView, "treeView");

      IEnumerable<IMaxNodeWrapper> context = HelperMethods.GetMaxNodes(treeView.SelectedNodes);
      return !context.All(n => n.IsNodePropertyInherited(this.Property));
   }


   protected override Boolean Checked(Outliner.Controls.Tree.TreeView treeView
                                     , Outliner.Controls.Tree.TreeNode clickedTn)
   {
      ExceptionHelpers.ThrowIfArgumentIsNull(treeView, "treeView");

      IEnumerable<IMaxNodeWrapper> context = HelperMethods.GetMaxNodes(treeView.SelectedNodes);
      return context.Any(n => n.GetNodeProperty(this.Property));
   }


   protected override void OnClick(Outliner.Controls.Tree.TreeView treeView
                                  , Outliner.Controls.Tree.TreeNode clickedTn)
   {
      ExceptionHelpers.ThrowIfArgumentIsNull(treeView, "treeView");

      IEnumerable<IMaxNodeWrapper> context = HelperMethods.GetMaxNodes(treeView.SelectedNodes);
      Boolean newValue = !this.Checked(treeView, clickedTn);
      NodeProperty prop = NodePropertyHelpers.ToProperty(this.Property);
      SetNodePropertyCommand<Boolean> cmd = new SetNodePropertyCommand<Boolean>(context, prop, newValue);
      cmd.Execute(true);
   }
}
}