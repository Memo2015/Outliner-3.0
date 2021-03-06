﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PJanssen.Outliner.Controls.Tree.Layout
{
public class TreeNodeLayoutItemCollection : IList<TreeNodeLayoutItem>
{
   private List<TreeNodeLayoutItem> items;
   private TreeNodeLayout layout;
   internal TreeNodeLayout Layout
   {
      get { return this.layout; }
      set 
      {
         this.layout = value;
         foreach (TreeNodeLayoutItem item in this.items)
            item.Layout = value;
      }
   }

   public TreeNodeLayoutItemCollection()
   {
      this.items = new List<TreeNodeLayoutItem>();
   }

   public TreeNodeLayoutItemCollection(TreeNodeLayoutItemCollection collection)
   {
      this.items = new List<TreeNodeLayoutItem>();

      foreach (TreeNodeLayoutItem item in collection)
      {
         this.Add(item.Copy());
      }
   }

   public void Add(TreeNodeLayoutItem item)
   {
      if (item == null)
         throw new ArgumentNullException("item");

      item.Layout = this.layout;
      this.items.Add(item);
   }

   public void Clear()
   {
      foreach (TreeNodeLayoutItem item in this.items)
         item.Layout = null;

      this.items.Clear();
   }

   public bool Contains(TreeNodeLayoutItem item)
   {
      return this.items.Contains(item);
   }

   public void CopyTo(TreeNodeLayoutItem[] array, int arrayIndex)
   {
      this.items.CopyTo(array, arrayIndex);
   }

   public int Count
   {
      get { return this.items.Count; }
   }

   public bool IsReadOnly
   {
      get { return false; }
   }

   public bool Remove(TreeNodeLayoutItem item)
   {
      return this.items.Remove(item);
   }

   public IEnumerator<TreeNodeLayoutItem> GetEnumerator()
   {
      return this.items.GetEnumerator();
   }

   System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
   {
      return this.items.GetEnumerator();
   }

   public int IndexOf(TreeNodeLayoutItem item)
   {
      return this.items.IndexOf(item);
   }

   public void Insert(int index, TreeNodeLayoutItem item)
   {
      this.items.Insert(index, item);
   }

   public void RemoveAt(int index)
   {
      this.items.RemoveAt(index);
   }

   public TreeNodeLayoutItem this[int index]
   {
      get
      {
         return this.items[index];
      }
      set
      {
         this.items[index] = value;
      }
   }
}
}
