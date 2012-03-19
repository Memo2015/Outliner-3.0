﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Outliner.Controls.FiltersBase;
using System.Resources;
using System.Xml.Serialization;
using System.ComponentModel;
using Outliner.Scene;

namespace Outliner.Controls.Layout
{
public class TreeNodeIcon : TreeNodeButton
{
   private Dictionary<String, Bitmap> icons;
   private Size iconSize;
   private IconSet iconSet;
   private Boolean invert;

   [XmlAttribute("iconSet")]
   public IconSet IconSet
   {
      get { return this.iconSet; }
      set
      {
         this.iconSet = value;
         this.icons = IconHelperMethods.CreateIconSetBitmaps(value, this.invert);
         this.iconSize = (this.icons.Count == 0) ? Size.Empty : this.icons.First().Value.Size;
      }
   }

   [XmlAttribute("invert")]
   [DefaultValue(true)]
   public Boolean Invert
   {
      get { return this.invert; }
      set
      {
         this.invert = value;
         this.icons = IconHelperMethods.CreateIconSetBitmaps(this.iconSet, value);
      }
   }

   public TreeNodeIcon() { }

   public TreeNodeIcon(Dictionary<String, Bitmap> icons) 
   {
      this.icons = icons;
      this.iconSize = (this.icons.Count == 0) ? Size.Empty : this.icons.First().Value.Size;
   }

   public TreeNodeIcon(ResourceSet resSet, Boolean invert) 
   {
      this.icons = IconHelperMethods.CreateIconSetBitmaps(resSet, invert);
      this.iconSize = (this.icons.Count == 0) ? Size.Empty : this.icons.First().Value.Size;
      this.invert = invert;
   }

   public TreeNodeIcon(IconSet iconSet, Boolean invert)
   {
      this.icons = IconHelperMethods.CreateIconSetBitmaps(iconSet, invert);
      this.iconSize = (this.icons.Count == 0) ? Size.Empty : this.icons.First().Value.Size;
      this.invert = invert;
   }

   public override int GetWidth(TreeNode tn)
   {
      return this.iconSize.Width;
   }

   public override int GetHeight(TreeNode tn)
   {
      return this.iconSize.Height;
   }

   public override void Draw(Graphics g, TreeNode tn)
   {
      if (this.Layout == null || this.icons == null)
         return;

      Bitmap icon = null;
      String iconKey = tn.ImageKey;

      if (iconKey == null)
         iconKey = IconHelperMethods.IMGKEY_UNKNOWN;

      TreeNodeData data = tn.Tag as TreeNodeData;
      if (data != null && data.FilterResult == FilterResult.ShowChildren)
         iconKey += "_filtered";

      if (!this.icons.TryGetValue(iconKey, out icon))
      {
         if (!this.icons.TryGetValue(IconHelperMethods.IMGKEY_UNKNOWN, out icon))
            return;
      }

      g.DrawImage(icon, this.GetBounds(tn));
   }

   public override void HandleClick(MouseEventArgs e, TreeNode tn) 
   {
      IMaxNodeWrapper node = HelperMethods.GetMaxNode(tn);
      if (node == null)
         return;

      if (isLight(node))
      {
         Autodesk.Max.IINode inode = node.WrappedNode as Autodesk.Max.IINode;
         if (inode == null)
            return;
         Autodesk.Max.ILightObject light = inode.ObjectRef as Autodesk.Max.ILightObject;
         if (light == null)
            return;
         Outliner.Commands.ToggleLightCommand cmd = new Commands.ToggleLightCommand(new List<IMaxNodeWrapper>(1) { node }, !light.UseLight);
         cmd.Execute(true);
      }
      else if (isCamera(node))
      {
         Autodesk.Max.IInterface ip = Autodesk.Max.GlobalInterface.Instance.COREInterface;
         Autodesk.Max.IViewExp vpt = ip.ActiveViewExp;
         Outliner.Commands.SetViewCameraCommand cmd = new Commands.SetViewCameraCommand(node, vpt);
         cmd.Execute(true);
      }
   }

   protected override string GetTooltipText(TreeNode tn)
   {
      IMaxNodeWrapper node = HelperMethods.GetMaxNode(tn);
      if (node == null)
         return null;

      if (isLight(node))
         return OutlinerResources.Tooltip_ToggleLight;
      else if (isCamera(node))
         return OutlinerResources.Tooltip_SetCamera;
      else
         return null;
   }

   private Boolean isLight(IMaxNodeWrapper node)
   {
      return node.SuperClassID == Autodesk.Max.SClass_ID.Light;
   }

   private Boolean isCamera(IMaxNodeWrapper node)
   {
      return node.SuperClassID == Autodesk.Max.SClass_ID.Camera;
   }
}
}
