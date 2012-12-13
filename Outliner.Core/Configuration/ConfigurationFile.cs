﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Drawing;
using System.IO;
using Outliner.Plugins;
using System.Reflection;

namespace Outliner.Configuration
{
public abstract class ConfigurationFile
{
   public ConfigurationFile() 
      : this(String.Empty, String.Empty, String.Empty, null) { }

   public ConfigurationFile(String text, String image16, String image24, Type resType)
   {
      this.TextRes = text;
      this.Image16Res = image16;
      this.Image24Res = image24;
      if (resType != null)
         this.ResourceTypeName = resType.Name;
   }

   [XmlElement("text")]
   [DisplayName("Text")]
   [TypeConverter(typeof(StringResourceConverter))]
   public String TextRes { get; set; }

   [XmlElement("image16")]
   [DefaultValue("")]
   [DisplayName("Image 16x16")]
   [TypeConverter(typeof(ImageResourceConverter))]
   public String Image16Res { get; set; }

   [XmlElement("image24")]
   [DefaultValue("")]
   [DisplayName("Image 24x24")]
   [TypeConverter(typeof(ImageResourceConverter))]
   public String Image24Res { get; set; }

   private String resourceTypeName;
   [XmlElement("resource_type")]
   [DefaultValue("")]
   [Browsable(true)]
   public String ResourceTypeName 
   {
      get { return resourceTypeName; } 
      set
      {
         this.resourceTypeName = value;
         this.resourceType = null;
      }
   }

   private Type resourceType;
   [XmlIgnore]
   [Browsable(false)]
   [TypeConverter(typeof(ResourceTypeConverter))]
   public Type ResourceType
   {
      get
      {
         if (this.resourceType == null && !String.IsNullOrEmpty(this.ResourceTypeName))
         {
            foreach (Assembly pluginAssembly in OutlinerPlugins.PluginAssemblies)
            {
               this.resourceType = pluginAssembly.GetType(this.ResourceTypeName);
               if (this.resourceType != null)
                  break;
            }
         }
         return this.resourceType;
      }
      set
      {
         this.resourceType = value;
         this.resourceTypeName = value.Name;
      }
   }

   [XmlIgnore]
   [Browsable(false)]
   public String Text
   {
      get
      {
         if (this.ResourceType != null && this.TextRes != null)
            return ResourceHelpers.LookupString(this.ResourceType, this.TextRes);
         else
            return this.TextRes;
      }
   }

   [XmlIgnore]
   [Browsable(false)]
   public Image Image16
   {
      get { return LookupImage(this.Image16Res); }
   }

   [XmlIgnore]
   [Browsable(false)]
   public Image Image24
   {
      get { return LookupImage(this.Image24Res); }
   }


   protected abstract String ImageBasePath { get; }

   private Image LookupImage(String image)
   {
      if (String.IsNullOrEmpty(image))
         return null;

      if (this.ResourceType != null)
         return ResourceHelpers.LookupImage(this.ResourceType, image);
      else
         return this.ImageFromFile(image);
   }

   private Image ImageFromFile(String image)
   {
      Image img = null;
      if (!String.IsNullOrEmpty(image))
      {
         if (!Path.IsPathRooted(image))
            image = Path.Combine(this.ImageBasePath, image);

         if (!Path.HasExtension(image))
            image = Path.ChangeExtension(image, "png");

         if (File.Exists(image))
            img = Image.FromFile(image);
      }

      return img;
   }
}
}