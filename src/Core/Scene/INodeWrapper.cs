﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Autodesk.Max;
using PJanssen.Outliner.MaxUtils;

namespace PJanssen.Outliner.Scene
{
   /// <summary>
   /// Defines a MaxNodeWrapper class for an INode object.
   /// </summary>
   public class INodeWrapper : MaxNodeWrapper
   {
      private IINode inode;

      /// <summary>
      /// Initializes a new instance of the INodeWrapper class for the given INode.
      /// </summary>
      /// <param name="inode">The INode object to wrap.</param>
      public INodeWrapper(IINode inode)
      {
         Throw.IfNull(inode, "inode");
         this.inode = inode;
      }

      public override object BaseObject
      {
         get { return this.INode; }
      }

      public override IMaxNode Parent
      {
         get
         {
            return new INodeWrapper(this.INode.ParentNode);
         }
         set
         {
            if (value != null)
               value.AddChildNode(this);
            else
               MaxScene.SceneRoot.AddChildNode(this);
         }
      }

      public override bool IsSelected
      {
         get
         {
            return this.INode.Selected;
         }
         set
         {
            if (value)
               MaxInterfaces.COREInterface.SelectNode(this.INode, false);
            else
               MaxInterfaces.COREInterface.DeSelectNode(this.INode);
         }
      }

      public override bool IsValid
      {
         get
         {
            if (!base.IsValid)
               return false;

            try
            {
               return !this.INode.TestAFlag(AnimatableFlags.IsDeleted);
            }
            catch
            {
               return false;
            }
         }
      }

      #region INodeWrapper specific

      /// <summary>
      /// Gets the wrapped INode.
      /// </summary>
      public IINode INode
      {
         get { return this.inode; }
      }

      /// <summary>
      /// Gets the IINodeLayerProperties from the wrapped INode.
      /// </summary>
      public IINodeLayerProperties NodeLayerProperties
      {
         get
         {
            IBaseInterface baseInterface = this.INode.GetInterface(MaxInterfaces.NodeLayerProperties);
            return baseInterface as IINodeLayerProperties;
         }
      }

      /// <summary>
      /// Gets the layer to which this node belongs.
      /// </summary>
      public IILayer ILayer
      {
         get
         {
            return this.INode.GetReference((int)ReferenceNumbers.NodeLayerRef) as IILayer;
         }
      }

      /// <summary>
      /// Gets whether the wrapped INode is an instanced object.
      /// </summary>
      public bool IsInstance
      {
         get
         {
            IINodeTab instances = MaxInterfaces.Global.INodeTabNS.Create();
            uint numInstances = MaxInterfaces.InstanceMgr.GetInstances(this.INode, instances);
            return numInstances > 1;
         }
      }


      #endregion


      #region Equality
      
      public override bool Equals(object obj)
      {
         INodeWrapper otherObj = obj as INodeWrapper;
         return otherObj != null && this.inode.Handle == otherObj.inode.Handle;
      }

      public override int GetHashCode()
      {
         return this.BaseObject.GetHashCode();
      }

      #endregion


      #region Delete

      public override bool CanDelete
      {
         get { return true; }
      }

      public override void Delete()
      {
         if (this.CanDelete)
         {
            MaxInterfaces.COREInterface.DeleteNode(this.INode, false, false);
         }
      }

      #endregion


      #region Childnodes

      public override Int32 ChildNodeCount
      {
         get { return this.INode.NumberOfChildren; }
      }

      public override IEnumerable<Object> ChildBaseObjects
      {
         get
         {
            int numChildren = this.INode.NumberOfChildren;
            List<IINode> nodes = new List<IINode>(numChildren);
            for (int i = 0; i < numChildren; i++)
            {
               nodes.Add(this.INode.GetChildNode(i));
            }
            return nodes;
         }
      }

      public override bool CanAddChildNode(IMaxNode node)
      {
         if (node == null)
            return false;

         //Aggregates (e.g. SelectionSet)
         if (node.IsAggregate)
            return this.CanAddChildNodes(node.ChildNodes);

         //IINode
         INodeWrapper inodeWrapper = node as INodeWrapper;
         if (inodeWrapper == null)
            return false;

         if (node.Parent.Equals(this))
            return false;

         RefResult loop = this.INode.TestForLoop( MaxInterfaces.IntervalForever
                                                , inodeWrapper.INode as IReferenceMaker);

         return loop == RefResult.Succeed;
      }

      public override void AddChildNode(IMaxNode node)
      {
         Throw.IfNull(node, "node");

         if (!this.CanAddChildNode(node))
            return;

         //Aggregates (e.g. SelectionSet)
         if (node.IsAggregate)
            this.AddChildNodes(node.ChildNodes);

         IINode inode = node.BaseObject as IINode;
         if (inode != null)
            this.INode.AttachChild(inode, true);
      }

      public override void RemoveChildNode(IMaxNode node)
      {
         Throw.IfNull(node, "node");

         INodeWrapper inodeWrapper = node as INodeWrapper;
         if (inodeWrapper != null)
            inodeWrapper.INode.Detach(0, true);
      }

      #endregion


      #region Node Type

      protected override MaxNodeType MaxNodeType
      {
         get { return MaxNodeType.Object; }
      }

      public override SClass_ID SuperClassID
      {
         get { return this.INode.ObjectRef.FindBaseObject().SuperClassID; }
      }

      public override IClass_ID ClassID
      {
         get { return this.INode.ObjectRef.ClassID; }
      }

      #endregion


      #region Name

      public override string Name
      {
         get { return this.INode.Name; }
         set
         {
            Throw.IfNull(value, "value");
            this.INode.Name = value;
            MaxInterfaces.Global.BroadcastNotification(SystemNotificationCode.NodeRenamed, this.INode);
         }
      }

      public override bool CanEditName
      {
         get { return true; }
      }

      public override string DisplayName
      {
         get
         {
            String name = this.Name;

            if (String.IsNullOrEmpty(name))
               return "-unnamed-";

            Boolean isGroupedNode = this.INode.IsGroupHead || this.INode.IsGroupMember;
            if (IINodes.IsXref(this.INode))
            {
               if (isGroupedNode)
                  return "{[" + name + "]}";
               else
                  return "{" + name + "}";
            }
            else if (isGroupedNode)
               return "[" + name + "]";
            else
               return name;
         }
      }

      public override string NodeTypeDisplayName
      {
         get { return OutlinerResources.Str_INode; }
      }

      #endregion


      #region Node Properties

      private Boolean IntToBool(int i)
      {
         return i != 0;
      }

      private int BoolToInt(bool b)
      {
         return b ? 1 : 0;
      }

      public override bool GetNodeProperty(BooleanNodeProperty property)
      {
         switch (property)
         {
            case BooleanNodeProperty.IsHidden: return this.INode.IsObjectHidden;
            case BooleanNodeProperty.IsFrozen: return this.INode.IsObjectFrozen;
            case BooleanNodeProperty.SeeThrough: return IntToBool(this.INode.XRayMtl_);
            case BooleanNodeProperty.BoxMode: return IntToBool(this.INode.BoxMode_);
            case BooleanNodeProperty.BackfaceCull: return IntToBool(this.INode.BackCull_);
            case BooleanNodeProperty.AllEdges: return IntToBool(this.INode.AllEdges_);
            case BooleanNodeProperty.VertexTicks: return IntToBool(this.INode.VertTicks);
            case BooleanNodeProperty.Trajectory: return IntToBool(this.INode.TrajectoryON);
            case BooleanNodeProperty.IgnoreExtents: return IntToBool(this.INode.IgnoreExtents_);
            case BooleanNodeProperty.FrozenInGray: return IntToBool(this.INode.ShowFrozenWithMtl);
            case BooleanNodeProperty.Renderable: return IntToBool(this.INode.Renderable);
            case BooleanNodeProperty.InheritVisibility: return this.INode.InheritVisibility;
            case BooleanNodeProperty.PrimaryVisibility: return IntToBool(this.INode.PrimaryVisibility);
            case BooleanNodeProperty.SecondaryVisibility: return IntToBool(this.INode.SecondaryVisibility);
            case BooleanNodeProperty.ReceiveShadows: return IntToBool(this.INode.RcvShadows);
            case BooleanNodeProperty.CastShadows: return IntToBool(this.INode.CastShadows);
            case BooleanNodeProperty.ApplyAtmospherics: return IntToBool(this.INode.ApplyAtmospherics);
            case BooleanNodeProperty.RenderOccluded: return this.INode.RenderOccluded;
            default: return base.GetNodeProperty(property);
         }
      }

      public override void SetNodeProperty(BooleanNodeProperty property, bool value)
      {
         switch (property)
         {
            case BooleanNodeProperty.IsHidden:
               this.INode.Hide(value);
               break;
            case BooleanNodeProperty.IsFrozen:
               this.INode.IsFrozen = value;
               break;
            case BooleanNodeProperty.SeeThrough:
               this.INode.XRayMtl(value);
               break;
            case BooleanNodeProperty.BoxMode:
               this.INode.BoxMode(value);
               break;
            case BooleanNodeProperty.BackfaceCull:
               this.INode.BackCull(value);
               break;
            case BooleanNodeProperty.AllEdges:
               this.INode.AllEdges(value);
               break;
            case BooleanNodeProperty.VertexTicks:
               this.INode.VertTicks = BoolToInt(value);
               break;
            case BooleanNodeProperty.Trajectory:
               this.INode.SetTrajectoryON(value);
               break;
            case BooleanNodeProperty.IgnoreExtents:
               this.INode.IgnoreExtents(value);
               break;
            case BooleanNodeProperty.FrozenInGray:
               this.INode.SetShowFrozenWithMtl(value);
               break;
            case BooleanNodeProperty.Renderable:
               this.INode.SetRenderable(value);
               break;
            case BooleanNodeProperty.InheritVisibility:
               this.INode.InheritVisibility = value;
               break;
            case BooleanNodeProperty.PrimaryVisibility:
               this.INode.SetPrimaryVisibility(value);
               break;
            case BooleanNodeProperty.SecondaryVisibility:
               this.INode.SetSecondaryVisibility(value);
               break;
            case BooleanNodeProperty.ReceiveShadows:
               this.INode.SetRcvShadows(value);
               break;
            case BooleanNodeProperty.CastShadows:
               this.INode.SetCastShadows(value);
               break;
            case BooleanNodeProperty.ApplyAtmospherics:
               this.INode.SetApplyAtmospherics(value);
               break;
            case BooleanNodeProperty.RenderOccluded:
               this.INode.RenderOccluded = value;
               break;
            default:
               base.SetNodeProperty(property, value);
               break;
         }
      }

      public override bool IsNodePropertyInherited(NodeProperty property)
      {
         IILayer layer = this.ILayer;
         IINodeLayerProperties layerProperties = this.NodeLayerProperties;

         if (property == NodeProperty.IsHidden)
            return layer != null && layer.IsHidden;
         else if (property == NodeProperty.IsFrozen)
            return layer != null && layer.IsFrozen;
         else if (property == NodeProperty.WireColor)
            return this.NodeLayerProperties.ColorByLayer;
         else if (NodeProperties.IsDisplayProperty(property))
            return layerProperties.DisplayByLayer;
         else if (NodeProperties.IsRenderProperty(property))
            return layerProperties.RenderByLayer;
         else
            return false;
      }

      public override Color WireColor
      {
         get { return Colors.FromMaxColor(this.INode.WireColor); }
         set
         {
            Throw.IfNull(value, "value");
            this.INode.WireColor = value;
         }
      }

      #endregion


      #region ImageKey

      private const String ImgKeyBone      = "bone";
      private const String ImgKeyCamera    = "camera";
      private const String ImgKeyContainer = "container";
      private const String ImgKeyGeometry  = "geometry";
      private const String ImgKeyGroupHead = "group";
      private const String ImgKeyHelper    = "helper";
      private const String ImgKeyLightOn   = "light_on";
      private const String ImgKeyLightOff  = "light_off";
      private const String ImgKeyMaterial  = "material";
      private const String ImgKeyNurbs     = "nurbs";
      private const String ImgKeyParticle  = "particle";
      private const String ImgKeyShape     = "shape";
      private const String ImgKeySpaceWarp = "spacewarp";
      private const String ImgKeyTarget    = "helper";
      private const String ImgKeyXref      = "xref";


      public override string ImageKey
      {
         get
         {
            IINode inode = this.INode;

            if (inode == null || inode.ObjectRef == null)
               return base.ImageKey;

            SClass_ID superClass = this.SuperClassID;
            switch (superClass)
            {
               case SClass_ID.Camera: return ImgKeyCamera;
               case SClass_ID.Light:
                  {
                     if (((ILightObject)inode.ObjectRef).UseLight)
                        return ImgKeyLightOn;
                     else
                        return ImgKeyLightOff;
                  }
               case SClass_ID.Material: return ImgKeyMaterial;
               case SClass_ID.Shape: return ImgKeyShape;
               case SClass_ID.WsmObject: return ImgKeySpaceWarp;
            }

            //if (superClass == SClass_ID.System && IINodes.IsXref(inode))
            //   return ImgKeyXref;

            if (superClass == SClass_ID.Helper)
            {
               if (inode.IsGroupHead)
                  return ImgKeyGroupHead;
               else if (MaxInterfaces.ContainerManager.IsContainerNode(inode) != null)
                  return ImgKeyContainer;
               else
                  return ImgKeyHelper;
            }

            if (superClass == SClass_ID.Geomobject)
            {
               //Target objects (for light/camera target)
               if (inode.IsTarget)
                  return ImgKeyTarget;

               IObject objRef = inode.ObjectRef;
               if (objRef == null)
                  return ImgKeyGeometry;

               //Nurbs / Shape objects.
               if (objRef.IsShapeObject)
                  return ImgKeyNurbs;

               //Particle objects.
               if (objRef.IsParticleSystem)
                  return ImgKeyParticle;

               //Bone and biped objects have Geomobject as a superclass.
               if (IINodes.IsBone(inode))
                  return ImgKeyBone;

               //All other geometry objects.
               return ImgKeyGeometry;
            }

            return base.ImageKey;
         }
      }

      #endregion


      #region Transforms

      public Boolean AllTransformsLocked
      {
         get
         {
            return INode.GetTransformLock(0, 0)
                && INode.GetTransformLock(0, 1)
                && INode.GetTransformLock(0, 2)
                && INode.GetTransformLock(1, 0)
                && INode.GetTransformLock(1, 1)
                && INode.GetTransformLock(1, 2)
                && INode.GetTransformLock(2, 0)
                && INode.GetTransformLock(2, 1)
                && INode.GetTransformLock(2, 2);
         }
         set
         {
            INode.SetTransformLock(0, 0, value);
            INode.SetTransformLock(0, 1, value);
            INode.SetTransformLock(0, 2, value);
            INode.SetTransformLock(1, 0, value);
            INode.SetTransformLock(1, 1, value);
            INode.SetTransformLock(1, 2, value);
            INode.SetTransformLock(2, 0, value);
            INode.SetTransformLock(2, 1, value);
            INode.SetTransformLock(2, 2, value);
         }
      }

      #endregion

      public override string ToString()
      {
         return String.Format("INodeWrapper ({0})", this.Name);
      }
   }
}
