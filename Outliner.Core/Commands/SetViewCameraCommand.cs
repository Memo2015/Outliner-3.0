﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outliner.Scene;
using Autodesk.Max;
using Outliner.MaxUtils;

namespace Outliner.Commands
{
   public class SetViewCameraCommand : Command
   {
      private IMaxNodeWrapper cameraNode;
      private IViewExp viewport;

      private IINode prevCameraNode;
      private Boolean prevIsPerspView;

      public SetViewCameraCommand(IMaxNodeWrapper cameraNode, IViewExp viewport)
      {
         ExceptionHelpers.ThrowIfArgumentIsNull(cameraNode, "cameraNode");
         ExceptionHelpers.ThrowIfArgumentIsNull(viewport, "viewport");

         this.cameraNode = cameraNode;
         this.viewport = viewport;
      }

      public override string Description
      {
         get
         {
            return "Set Camera";
         }
      }

      protected override void Do()
      {
         if (this.viewport == null || this.cameraNode == null)
            return;

         if (!this.cameraNode.IsValid)
            return;

         this.prevCameraNode = this.viewport.ViewCamera;
         this.prevIsPerspView = this.viewport.IsPerspView;
         

         viewport.ViewCamera = (IINode)this.cameraNode.WrappedNode;
      }

      protected override void Undo()
      {
         if (this.prevCameraNode == null)
            viewport.SetViewUser(this.prevIsPerspView);
         else
            viewport.ViewCamera = this.prevCameraNode;
      }
   }
}