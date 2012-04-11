﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Drawing;
using Outliner.Controls;

namespace Outliner.Scene
{
public class IINodeWrapper : IMaxNodeWrapper
{
   private IINode node;
   public IINodeWrapper(IINode node)
   {
      this.node = node;
   }

   public override object WrappedNode
   {
      get { return this.node; }
   }

   public IINode IINode
   {
      get { return this.node; }
   }

   public  IINodeLayerProperties NodeLayerProperties
   {
      get
      {
         IBaseInterface baseInterface = this.node.GetInterface(MaxInterfaces.NodeLayerProperties);
         return baseInterface as IINodeLayerProperties;
      }
   }

   public IILayerProperties Layer
   {
      get
      {
         IINodeLayerProperties nodeLayerProperties = this.NodeLayerProperties;
         if (nodeLayerProperties != null)
            return nodeLayerProperties.Layer;
         else
            return null;
      }
   }

   public override IMaxNodeWrapper Parent
   {
      get
      {
         return new IINodeWrapper(this.node.ParentNode);
      }
   }

   public override IEnumerable<Object> ChildNodes
   {
      get 
      {
         int numChildren = this.node.NumberOfChildren;
         List<IINode> nodes = new List<IINode>(numChildren);
         for (int i = 0; i < numChildren; i++)
            nodes.Add(this.node.GetChildNode(i));
         return nodes;
      }
   }

   public override bool CanAddChildNode(IMaxNodeWrapper node)
   {
      return node is IINodeWrapper && node != this;
   }
   public override bool CanAddChildNodes(IEnumerable<IMaxNodeWrapper> nodes)
   {
      return nodes.All(this.CanAddChildNode);
   }

   public override void AddChildNode(IMaxNodeWrapper node)
   {
      if (node.WrappedNode is IINode)
         this.node.AttachChild((IINode)node.WrappedNode, true);
   }

   public override void RemoveChildNode(IMaxNodeWrapper node)
   {
      if (node is IINodeWrapper)
         ((IINodeWrapper)node).node.Detach(0, true);
   }

   public override String Name
   {
      get { return this.node.Name; }
      set { this.node.Name = value; }
   }

   public override string DisplayName
   {
      get
      {
         if (this.Name == String.Empty)
            return "-unnamed-";

         Boolean isGroupedNode = this.node.IsGroupHead || this.node.IsGroupMember;
         if (HelperMethods.IsXref(this.node))
         {
            if (isGroupedNode)
               return "{[" + this.Name + "]}";
            else
               return "{" + this.Name + "}";
         }
         else if (isGroupedNode)
            return "[" + this.Name + "]";
         else
            return this.Name;
      }
   }

   public override IClass_ID ClassID
   {
      get { return this.node.ObjectRef.ClassID; }
   }

   public override SClass_ID SuperClassID
   {
      get { return this.node.ObjectRef.FindBaseObject().SuperClassID; }
   }

   public override bool Selected
   {
      get { return this.node.Selected; }
   }

   public override bool IsNodeType(MaxNodeTypes types)
   {
      return types.HasFlag(MaxNodeTypes.Object);
   }

   public override bool IsHidden
   {
      get { return this.node.IsObjectHidden; }
      set { this.node.Hide(value); }
   }

   public override bool IsFrozen
   {
      get { return this.node.IsObjectFrozen; }
      set { this.node.IsFrozen = value; }
   }

   public override bool BoxMode
   {
      get { return this.node.BoxMode_ != 0; }
      set { this.node.BoxMode(value); }
   }

   public override Color WireColor
   {
      get { return ColorHelpers.FromMaxColor(this.node.WireColor); }
      set { this.node.WireColor = value; }
   }

   public override bool Renderable
   {
      get { return this.node.Renderable != 0; }
      set { this.node.SetRenderable(value); }
   }


   public override bool IsValid
   {
      get
      {
         if (!base.IsValid)
            return false;

         try { return !this.node.TestAFlag(AnimatableFlags.IsDeleted); }
         catch { return false; }
      }
   }

   public bool IsInstance
   {
      get
      {
         IINodeTab instances = GlobalInterface.Instance.INodeTabNS.Create();
         uint numInstances = MaxInterfaces.InstanceMgr.GetInstances(this.node, instances);
         return numInstances > 1;
      }
   }

   public const String IMGKEY_BONE      = "bone";
   public const String IMGKEY_CAMERA    = "camera";
   public const String IMGKEY_GEOMETRY  = "geometry";
   public const String IMGKEY_GROUPHEAD = "group";
   public const String IMGKEY_HELPER    = "helper";
   public const String IMGKEY_LIGHT     = "light";
   public const String IMGKEY_MATERIAL  = "material";
   public const String IMGKEY_NURBS     = "nurbs";
   public const String IMGKEY_PARTICLE  = "particle";
   public const String IMGKEY_SHAPE     = "shape";
   public const String IMGKEY_SPACEWARP = "spacewarp";
   public const String IMGKEY_TARGET    = "helper";
   public const String IMGKEY_XREF      = "xref";
   

   public override string ImageKey
   {
      get
      {
         if (this.node == null || this.node.ObjectRef == null)
            return base.ImageKey;

         SClass_ID superClass = this.SuperClassID;
         switch (superClass)
         {
            case SClass_ID.Camera: return IMGKEY_CAMERA;
            case SClass_ID.Light: return IMGKEY_LIGHT;
            case SClass_ID.Material: return IMGKEY_MATERIAL;
            case SClass_ID.Shape: return IMGKEY_SHAPE;
            case SClass_ID.WsmObject: return IMGKEY_SPACEWARP;
         }

         if (superClass == SClass_ID.System && HelperMethods.IsXref(node))
            return IMGKEY_XREF;

         if (superClass == SClass_ID.Helper)
         {
            if (this.node.IsGroupHead)
               return IMGKEY_GROUPHEAD;
            else
               return IMGKEY_HELPER;
         }

         if (superClass == SClass_ID.Geomobject)
         {
            //Target objects (for light/camera target)
            if (node.IsTarget)
               return IMGKEY_TARGET;

            IObject objRef = node.ObjectRef;
            if (objRef == null)
               return IMGKEY_GEOMETRY;

            //Nurbs / Shape objects.
            if (objRef.IsShapeObject)
               return IMGKEY_NURBS;

            //Particle objects.
            if (objRef.IsParticleSystem)
               return IMGKEY_PARTICLE;

            //Bone and biped objects have Geomobject as a superclass.
            if (HelperMethods.IsBone(node))
               return IMGKEY_BONE;

            //All other geometry objects.
            return IMGKEY_GEOMETRY;
         }

         return base.ImageKey;
      }
   }
}
}
