﻿using System;
using System.Collections.Generic;

namespace Outliner.Controls.FiltersBase
{
   public class FilterCollection<T>
   {
      public delegate IEnumerable<T> GetChildNodes(T node);
      public GetChildNodes GetChildNodesFn { get; set; }


      public FilterCollection() : this(null) { }
      public FilterCollection(FilterCollection<T> collection)
      {
         if (collection == null)
         {
            _enabled = false;
            _filters = new List<Filter<T>>();
            _permanentFilters = new List<Filter<T>>();
         }
         else
         {
            _enabled = collection.Enabled;
            _filters = collection._filters;
            _permanentFilters = collection._permanentFilters;
         }
      }

      private List<Filter<T>> _filters;
      private List<Filter<T>> _permanentFilters;


      private Boolean _enabled;
      public Boolean Enabled
      {
         get { return _enabled; }
         set
         {
            _enabled = value;
            this.OnFiltersEnabled();
         }
      }

      public Boolean HasPermanentFilters
      {
         get { return _permanentFilters.Count > 0; }
      }

      /// <summary>
      /// Adds the supplied filter to the collection as non-permanent.
      /// </summary>
      public void Add(Filter<T> filter)
      {
         this.Add(filter, false);
      }
      /// <summary>
      /// Adds the supplied filter to the collection.
      /// </summary>
      /// <param name="filter">The filter to add.</param>
      /// <param name="permanent">If true, the collection's "Enabled" property does not affect this filter.</param>
      public void Add(Filter<T> filter, Boolean permanent)
      {
         if (permanent)
         {
            if (!_permanentFilters.Contains(filter))
               _permanentFilters.Add(filter);
         }
         else
         {
            if (!_filters.Contains(filter))
               _filters.Add(filter);
         }

         filter.FilterChanged += filterChanged;

         this.OnFilterAdded(filter);
      }


      /// <summary>
      /// Removes the supplied filter from the collection.
      /// </summary>
      public void Remove(Filter<T> filter)
      {
         if (_filters.Contains(filter))
            _filters.Remove(filter);

         if (_permanentFilters.Contains(filter))
            _permanentFilters.Remove(filter);

         filter.FilterChanged -= filterChanged;

         this.OnFilterRemoved(filter);
      }
      /// <summary>
      /// Removes all filters of the supplied type from the collection.
      /// </summary>
      public void Remove(Type filterType)
      {
         List<Filter<T>> filtersToRemove = new List<Filter<T>>();

         foreach (Filter<T> filter in _filters)
         {
            if (filter.GetType().Equals(filterType))
               filtersToRemove.Add(filter);
         }
         foreach (Filter<T> filter in _permanentFilters)
         {
            if (filter.GetType().Equals(filterType))
               filtersToRemove.Add(filter);
         }

         foreach (Filter<T> filter in filtersToRemove)
         {
            this.Remove(filter);
         }
         filtersToRemove.Clear();
      }


      /// <summary>
      /// Removes all non-permanent filters from the collection.
      /// </summary>
      public void Clear()
      {
         this.Clear(false);
      }
      /// <summary>
      /// Removes all filters from the collection.
      /// </summary>
      /// <param name="clearPermanentFilters">If true also removes permanent filters.</param>
      public void Clear(Boolean clearPermanentFilters)
      {
         foreach (Filter<T> filter in _filters)
         {
            filter.FilterChanged -= filterChanged;
         }
         _filters.Clear();

         if (clearPermanentFilters)
         {
            foreach (Filter<T> filter in _permanentFilters)
            {
               filter.FilterChanged -= filterChanged;
            }
            _permanentFilters.Clear();
         }

         this.OnFiltersCleared();
      }


      /// <summary>
      /// Retrieves the first found filter in the collection of the supplied type.
      /// </summary>
      public Filter<T> Get(Type filterType)
      {
         foreach (Filter<T> filter in _filters)
         {
            if (filter.GetType().Equals(filterType))
               return filter;
         }
         foreach (Filter<T> filter in _permanentFilters)
         {
            if (filter.GetType().Equals(filterType))
               return filter;
         }
         return null;
      }


      /// <summary>
      /// Returns true if the collection contains the supplied filter.
      /// </summary>
      public Boolean Contains(Filter<T> filter)
      {
         return _filters.Contains(filter);
      }
      /// <summary>
      /// Returns true if the collection contains a filter of the supplied type.
      /// </summary>
      public Boolean Contains(Type filterType)
      {
         foreach (Filter<T> filter in _filters)
         {
            if (filter.GetType().Equals(filterType))
               return true;
         }
         foreach (Filter<T> filter in _permanentFilters)
         {
            if (filter.GetType().Equals(filterType))
               return true;
         }

         return false;
      }



      /// <summary>
      /// Tests whether the supplied node and its children should be shown.
      /// </summary>
      public virtual FilterResult ShowNode(T n)
      {
         FilterResult filterResult = FilterResult.Show;

         // Loop through filters.
         if (this.Enabled && _filters.Count > 0)
         {
            foreach (Filter<T> filter in _filters)
            {
               if (filter.ShowNode(n) == FilterResult.Hide)
               {
                  filterResult = FilterResult.Hide;
                  break;
               }
            }
         }

         if (filterResult == FilterResult.Show && _permanentFilters.Count > 0)
         {
            foreach (Filter<T> filter in _permanentFilters)
            {
               if (filter.ShowNode(n) == FilterResult.Hide)
               {
                  filterResult = FilterResult.Hide;
                  break;
               }
            }
         }

         // If any of the filters return FilterResult.Hide, loop through children too.
         if (filterResult == FilterResult.Hide)
            return this.ShowChildNodes(n);
         else
            return filterResult;
      }

      /// <summary>
      /// Tests whether the children of the supplied node should be shown.
      /// </summary>
      protected virtual FilterResult ShowChildNodes(T n)
      {
         if (n == null || this.GetChildNodesFn == null)
            return FilterResult.Hide;

         IEnumerable<T> childNodes = this.GetChildNodesFn(n);
         foreach (T child in childNodes)
         {
            if (this.ShowNode(child) == FilterResult.Show
               || this.ShowChildNodes(child) == FilterResult.ShowChildren)
               return FilterResult.ShowChildren;
         }

         return FilterResult.Hide;
         /*
         if (n == null || this.GetChildNodeCountFn == null || this.GetChildNodeAtFn == null)
            return FilterResult.Hide;

         int numChildren = this.GetChildNodeCountFn(n);

         if (numChildren == 0)
            return FilterResult.Hide;

         for (int i = 0; i < numChildren; i++)
         {
            T child = this.GetChildNodeAtFn(n, i);
            if (this.ShowNode(child) == FilterResult.Show
               || this.ShowChildNodes(child) == FilterResult.ShowChildren)
               return FilterResult.ShowChildren;
         }

         return FilterResult.Hide;
         */
      }



      // Events.

      /// <summary>
      /// Raised when the filter collection's Enabled property has been changed.
      /// </summary>
      public event EventHandler FiltersEnabled;
      protected virtual void OnFiltersEnabled()
      {
         if (this.FiltersEnabled != null)
            this.FiltersEnabled(this, new EventArgs());
      }

      /// <summary>
      /// Raised when the Clear method has been called.
      /// </summary>
      public event EventHandler FiltersCleared;
      protected virtual void OnFiltersCleared()
      {
         if (this.FiltersCleared != null)
            this.FiltersCleared(this, new EventArgs());
      }

      /// <summary>
      /// Raised when a filter has been added to the collection.
      /// </summary>
      public event NodeFilterChangedEventHandler FilterAdded;
      protected virtual void OnFilterAdded(Filter<T> filter)
      {
         if (this.FilterAdded != null)
            this.FilterAdded(this, new NodeFilterChangedEventArgs<T>(filter));
      }

      /// <summary>
      /// Raised when a filter has been removed from the collection.
      /// </summary>
      public event NodeFilterChangedEventHandler FilterRemoved;
      protected virtual void OnFilterRemoved(Filter<T> filter)
      {
         if (this.FilterRemoved != null)
            this.FilterRemoved(this, new NodeFilterChangedEventArgs<T>(filter));
      }

      /// <summary>
      /// Raised when the properties of a filter in the collection has been changed.
      /// </summary>
      public event NodeFilterChangedEventHandler FilterChanged;
      protected void filterChanged(object sender, EventArgs e)
      {
         if (this.FilterChanged != null)
            this.FilterChanged(this, new NodeFilterChangedEventArgs<T>(sender as Filter<T>));
      }

      public delegate void NodeFilterChangedEventHandler(object sender, NodeFilterChangedEventArgs<T> e);
   }


   public class NodeFilterChangedEventArgs<T> : EventArgs
   {
      public Filter<T> Filter { get; private set; }

      public NodeFilterChangedEventArgs(Filter<T> filter)
      {
         this.Filter = filter;
      }
   }
}