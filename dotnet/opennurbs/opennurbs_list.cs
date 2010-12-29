using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

using Rhino.Geometry;

namespace Rhino.Collections
{
  /// <summary>
  /// Represents a list of generic data. This class is similar to System.Collections.Generic.List(T) 
  /// but exposes a few more methods.
  /// </summary>
  [Serializable,
  DebuggerTypeProxy(typeof(ListDebuggerDisplayProxy<>)),
  DebuggerDisplay("Count = {Count}")]
  public class RhinoList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
  {
    #region Fields

    /// <summary>
    /// Internal array of items. The array will contain trailing invalid items if Capacity > Count. 
    /// WARNING! Do not store a reference to this array anywhere! The List class may decide to replace 
    /// the internal array with another one.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal T[] m_items;

    /// <summary>
    /// The number of "valid" elements in m_items (same as m_count in ON_SimpleArray)
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal int m_size;

    [NonSerialized]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object m_syncRoot;

    /// <summary>
    /// The version counter is incremented whenever a change is made to the list.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int m_version;
    #endregion

    #region Constructors
    /// <summary>
    /// Create a new, empty list.
    /// </summary>
    public RhinoList()
    {
      this.m_items = new T[0];
    }

    /// <summary>
    /// Create an empty list with a certain capacity.
    /// </summary>
    /// <param name="initialCapacity">Number of items this list can store without resizing.</param>
    public RhinoList(int initialCapacity)
    {
      if (initialCapacity < 0)
      {
        throw new ArgumentOutOfRangeException("initialCapacity", "RhinoList cannot be constructed with a negative capacity");
      }
      this.m_items = new T[initialCapacity];
    }

    /// <summary>
    /// Create a new List with a specified amount of values.
    /// </summary>
    /// <param name="amount">Number of values to add to this list. Must be equal to or larger than zero.</param>
    /// <param name="defaultValue">Value to add, for reference types, 
    /// the same item will be added over and over again.</param>
    public RhinoList(int amount, T defaultValue)
    {
      if (amount < 0) { throw new ArgumentOutOfRangeException("amount", "RhinoList cannot be constructed with a negative amount"); }
      if (amount == 0) { return; }

      this.m_items = new T[amount];
      this.m_size = amount;

      for (int i = 0; i < amount; i++)
      {
        this.m_items[i] = defaultValue;
      }
    }

    /// <summary>
    /// Create a list that is a shallow duplicate of a collection. 
    /// </summary>
    /// <param name="collection">Collection of items to duplicate.</param>
    public RhinoList(IEnumerable<T> collection)
    {
      if (collection == null)
      {
        throw new ArgumentNullException("collection");
      }

      ICollection<T> is2 = collection as ICollection<T>;
      if (is2 != null)
      {
        int count = is2.Count;
        this.m_items = new T[count];

        is2.CopyTo(this.m_items, 0);
        this.m_size = count;
      }
      else
      {
        this.m_size = 0;
        this.m_items = new T[4];
        using (IEnumerator<T> enumerator = collection.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            this.Add(enumerator.Current);
          }
        }
      }
    }

    /// <summary>
    /// Copy constructor. Create a shallow duplicate of another list.
    /// </summary>
    /// <param name="list">List to mimic.</param>
    public RhinoList(RhinoList<T> list)
    {
      if (list == null) { throw new ArgumentNullException("list"); }

      //Set capacity to match.
      this.Capacity = list.Capacity;

      if (list.m_size > 0)
      {
        Array.Copy(list.m_items, this.m_items, list.m_items.Length);
      }
    }

    /// <summary>
    /// Create a shallow copy of the items in this list.
    /// </summary>
    /// <returns>An array containing all the items in this list. 
    /// Trailing items are not included.</returns>
    public T[] ToArray()
    {
      T[] destinationArray = new T[this.m_size];
      Array.Copy(this.m_items, 0, destinationArray, 0, this.m_size);
      return destinationArray;
    }
    #endregion

    #region Properties
    private void EnsureCapacity(int min)
    {
      if (this.m_items.Length < min)
      {
        int num = (this.m_items.Length == 0) ? 4 : (this.m_items.Length * 2);
        if (num < min)
        {
          num = min;
        }
        this.Capacity = num;
      }
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the List, 
    /// if that number is less than a threshold value.
    /// </summary>
    /// <remarks>This function differs from the DotNET implementation of List&lt;T&gt; 
    /// since that one only trims the excess if the excess exceeds 10% of the list length.</remarks>
    public void TrimExcess()
    {
      this.Capacity = this.m_size;
    }

    /// <summary>
    /// Gets or sets the total number of elements the internal data structure can hold without resizing.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Capacity
    {
      get
      {
        return this.m_items.Length;
      }
      set
      {
        if (value != this.m_items.Length)
        {
          if (value < this.m_size)
            throw new ArgumentOutOfRangeException("value","Capacity must be larger than or equal to the list Count");

          if (value > 0)
          {
            T[] destinationArray = new T[value];
            if (this.m_size > 0)
            {
              Array.Copy(this.m_items, 0, destinationArray, 0, this.m_size);
            }
            this.m_items = destinationArray;
          }
          else
          {
            this.m_items = new T[0];
          }
        }
      }
    }

    /// <summary>
    /// Gets the number of elements actually contained in the List.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Count
    {
      get
      {
        return this.m_size;
      }
    }

    /// <summary>
    /// Gets the number of null references (Nothing in Visual Basic) in this list. 
    /// If T is a valuetype, this property always return zero.
    /// </summary>
    public int NullCount
    {
      get
      {
        Type Tt = typeof(T);
        if (Tt.IsValueType) { return 0; }

        int N = 0;
        for (int i = 0; i < m_size; i++)
        {
          if (this.m_items[i] == null) { N++; }
        }

        return N;
      }
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public T this[int index]
    {
      get
      {
        // IronPython seems to expect IndexOutOfRangeExceptions with
        // indexing properties
        if (index >= this.m_size) { throw new IndexOutOfRangeException("index"); }
        return this.m_items[index];
      }
      set
      {
        if (index >= this.m_size) { throw new IndexOutOfRangeException("You cannot set items which do not yet exist, consider using Insert or Add instead."); }

        this.m_items[index] = value;
        this.m_version++;
      }
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    object IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        RhinoList<T>.VerifyValueType(value);
        this[index] = (T)value;
      }
    }

    /// <summary>
    /// Gets or sets the first item in the list. This is synonymous to calling List[0].
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public T First
    {
      get { return this[0]; }
      set { this[0] = value; }
    }

    /// <summary>
    /// Gets or sets the last item in the list. This is synonymous to calling List[Count-1].
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public T Last
    {
      get { return this[m_size - 1]; }
      set { this[m_size - 1] = value; }
    }

    /// <summary>
    /// Remap an index in the infinite range onto the List index range.
    /// </summary>
    /// <param name="index">Index to remap.</param>
    /// <returns>Remapped index</returns>
    public int RemapIndex(int index)
    {
      int c = index % (m_size - 1);
      if (c < 0) { c = (m_size - 1) + c; }
      return c;
    }

    /// <summary>
    /// When implemented by a class, gets a value indicating whether the IList is read-only. 
    /// RhinoList is never ReadOnly.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IList.IsReadOnly { get { return false; } }

    /// <summary>
    /// When implemented by a class, gets a value indicating whether the IList has a fixed size. 
    /// RhinoList is never fixed.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IList.IsFixedSize { get { return false; } }

    /// <summary>
    /// When implemented by a class, gets a value indicating whether the IList is read-only. 
    /// RhinoList is never ReadOnly.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool ICollection<T>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Creates a multi-line string representation of all the items in this list.
    /// </summary>
    string ListToString
    {
      get
      {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(this.Count * 20);

        for (int i = 0; i < this.Count; i++)
        {
          sb.AppendLine( this[i].ToString() );
        }

        return sb.ToString();
      }
    }

    /// <summary>
    /// When implemented by a class, gets a value indicating whether access to the ICollection is synchronized (thread-safe).
    /// ON_List is never Synchronized.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool ICollection.IsSynchronized { get { return false; } }

    /// <summary>
    /// Gets an object that can be used to synchronize access to the ICollection.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    object ICollection.SyncRoot
    {
      get
      {
        if (this.m_syncRoot == null)
        {
          Interlocked.CompareExchange(ref this.m_syncRoot, new object(), null);
        }
        return this.m_syncRoot;
      }
    }
    #endregion

    #region Methods
    #region Addition and Removal

    /// <summary>
    /// Removes all elements from the List.
    /// </summary>
    public void Clear()
    {
      if (this.m_size == 0) { return; }

      Array.Clear(this.m_items, 0, this.m_size);
      this.m_size = 0;
      this.m_version++;
    }

    /// <summary>
    /// Adds an item to the IList.
    /// </summary>
    /// <param name="item">The Object to add to the IList.</param>
    /// <returns>The position into which the new element was inserted.</returns>
    int IList.Add(object item)
    {
      RhinoList<T>.VerifyValueType(item);
      this.Add((T)item);
      return (this.Count - 1);
    }
    private static void VerifyValueType(object value)
    {
      if (!RhinoList<T>.IsCompatibleObject(value))
      {
        throw new ArgumentException("value is not a supported type");
      }
    }
    private static bool IsCompatibleObject(object value)
    {
      if (!(value is T) && ((value != null) || typeof(T).IsValueType))
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Adds an object to the end of the List.
    /// </summary>
    /// <param name="item">Item to append.</param>
    public void Add(T item)
    {
      if (this.m_size == this.m_items.Length)
      {
        this.EnsureCapacity(this.m_size + 1);
      }

      this.m_items[this.m_size++] = item;
      this.m_version++;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the List.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the List. 
    /// The collection itself cannot be a null reference (Nothing in Visual Basic), 
    /// but it can contain elements that are a null reference (Nothing in Visual Basic), 
    /// if type T is a reference type.
    /// </param>
    public void AddRange(IEnumerable<T> collection)
    {
      this.InsertRange(this.m_size, collection);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the List.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the List. 
    /// The collection itself cannot be a null reference (Nothing in Visual Basic), 
    /// but it can contain elements that are a null reference (Nothing in Visual Basic). 
    /// Objects in collection which cannot be represented as T will throw exceptions.
    /// </param>
    public void AddRange(IEnumerable collection)
    {
      Type Tt = typeof(T);

      foreach (object obj in collection)
      {
        if (obj == null)
        {
          this.Add(default(T));
          continue;
        }

        if (Tt.IsAssignableFrom(obj.GetType()))
        {
          this.Add((T)obj);
        }
        else
        {
          string local_type = Tt.Name;
          string import_type = obj.GetType().Name;
          string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "You cannot add an object of type {0} to a list of type {1}",
            import_type,
            local_type);
          throw new InvalidCastException( msg );
        }
      }
    }

    /// <summary>
    /// Inserts an element into the List at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert. The value can be a null reference 
    /// (Nothing in Visual Basic) for reference types.</param>
    void IList.Insert(int index, object item)
    {
      RhinoList<T>.VerifyValueType(item);
      this.Insert(index, (T)item);
    }

    /// <summary>
    /// Inserts an element into the List at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert. The value can be a null reference 
    /// (Nothing in Visual Basic) for reference types.</param>
    public void Insert(int index, T item)
    {
      if (index > this.m_size)
      {
        throw new ArgumentOutOfRangeException("index");
      }

      if (this.m_size == this.m_items.Length)
      {
        this.EnsureCapacity(this.m_size + 1);
      }

      if (index < this.m_size)
      {
        Array.Copy(this.m_items, index, this.m_items, index + 1, this.m_size - index);
      }

      this.m_items[index] = item;
      this.m_size++;
      this.m_version++;
    }

    /// <summary>
    /// Inserts the elements of a collection into the List at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="collection">The collection whose elements should be inserted into the List. 
    /// The collection itself cannot be a null reference (Nothing in Visual Basic), 
    /// but it can contain elements that are a null reference (Nothing in Visual Basic), 
    /// if type T is a reference type.</param>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
      if (collection == null)
      {
        throw new ArgumentNullException("collection");
      }

      if (index > this.m_size)
      {
        throw new ArgumentOutOfRangeException("index");
      }

      ICollection<T> is2 = collection as ICollection<T>;
      if (is2 != null)
      {
        int count = is2.Count;
        if (count > 0)
        {
          this.EnsureCapacity(this.m_size + count);
          if (index < this.m_size)
          {
            Array.Copy(this.m_items, index, this.m_items, index + count, this.m_size - index);
          }
          if (this == is2)
          {
            Array.Copy(this.m_items, 0, this.m_items, index, index);
            Array.Copy(this.m_items, (int)(index + count), this.m_items, (int)(index * 2), (int)(this.m_size - index));
          }
          else
          {
            T[] array = new T[count];
            is2.CopyTo(array, 0);
            array.CopyTo(this.m_items, index);
          }
          this.m_size += count;
        }
      }
      else
      {
        using (IEnumerator<T> enumerator = collection.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            this.Insert(index++, enumerator.Current);
          }
        }
      }
      this.m_version++;
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the List.
    /// </summary>
    /// <param name="item">The object to remove from the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    void IList.Remove(object item)
    {
      if (RhinoList<T>.IsCompatibleObject(item))
      {
        this.Remove((T)item);
      }
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the List.
    /// </summary>
    /// <param name="item">The object to remove from the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <returns>true if item is successfully removed; otherwise, false. 
    /// This method also returns false if item was not found in the List.</returns>
    public bool Remove(T item)
    {
      int index = this.IndexOf(item);
      if (index >= 0)
      {
        this.RemoveAt(index);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Removes the all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements removed from the List.</returns>
    public int RemoveAll(Predicate<T> match)
    {
      if (match == null)
      {
        throw new ArgumentNullException("match");
      }

      int index = 0;
      while ((index < this.m_size) && !match(this.m_items[index]))
      {
        index++;
      }

      if (index >= this.m_size)
      {
        return 0;
      }

      int num2 = index + 1;
      while (num2 < this.m_size)
      {
        while ((num2 < this.m_size) && match(this.m_items[num2]))
        {
          num2++;
        }
        if (num2 < this.m_size)
        {
          this.m_items[index++] = this.m_items[num2++];
        }
      }

      Array.Clear(this.m_items, index, this.m_size - index);
      int num3 = this.m_size - index;
      this.m_size = index;
      this.m_version++;

      return num3;
    }

    /// <summary>
    /// Removes all elements from the List that are null references (Nothing in Visual Basic). 
    /// This function will not do anything if T is not a Reference type.
    /// </summary>
    /// <returns>The number of nulls removed from the List.</returns>
    public int RemoveNulls()
    {
      Type Tt = typeof(T);
      if (Tt.IsValueType) { return 0; }
      return RemoveAll(ON_Null_Predicate);
    }
    internal bool ON_Null_Predicate(T val) { return val == null; }

    /// <summary>
    /// Removes the element at the specified index of the List.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public void RemoveAt(int index)
    {
      if (index >= this.m_size)
      {
        throw new ArgumentOutOfRangeException("index");
      }

      this.m_size--;
      if (index < this.m_size)
      {
        Array.Copy(this.m_items, index + 1, this.m_items, index, this.m_size - index);
      }
      this.m_items[this.m_size] = default(T);
      this.m_version++;
    }

    /// <summary>
    /// Removes a range of elements from the List.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    public void RemoveRange(int index, int count)
    {
      if (index < 0) { throw new ArgumentOutOfRangeException("index"); }
      if (count < 0) { throw new ArgumentOutOfRangeException("count"); }

      if ((this.m_size - index) < count)
      {
        throw new ArgumentException("This combination of index and count is not valid");
      }

      if (count > 0)
      {
        this.m_size -= count;
        if (index < this.m_size)
        {
          Array.Copy(this.m_items, index + count, this.m_items, index, this.m_size - index);
        }
        Array.Clear(this.m_items, this.m_size, count);
        this.m_version++;
      }
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source List.
    /// </summary>
    /// <param name="index">The zero-based List index at which the range starts.</param>
    /// <param name="count">The number of elements in the range.</param>
    /// <returns>A shallow copy of a range of elements in the source List.</returns>
    public RhinoList<T> GetRange(int index, int count)
    {
      if ((index < 0) || (count < 0))
        throw new ArgumentOutOfRangeException("index");

      if ((this.m_size - index) < count)
        throw new ArgumentOutOfRangeException("index");

      RhinoList<T> list = new RhinoList<T>(count);
      Array.Copy(this.m_items, index, list.m_items, 0, count);
      list.m_size = count;
      return list;
    }
    #endregion

    #region Searching
    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the 
    /// first occurrence within the entire List.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) 
    /// for reference types.</param>
    /// <returns>The zero-based index of the first occurrence of item within 
    /// the entire List, if found; otherwise, �1.</returns>
    int IList.IndexOf(object item)
    {
      if (RhinoList<T>.IsCompatibleObject(item))
      {
        return this.IndexOf((T)item);
      }
      return -1;
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the 
    /// first occurrence within the entire List.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) 
    /// for reference types.</param>
    /// <returns>The zero-based index of the first occurrence of item within 
    /// the entire List, if found; otherwise, �1.</returns>
    public int IndexOf(T item)
    {
      return Array.IndexOf<T>(this.m_items, item, 0, this.m_size);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of 
    /// the first occurrence within the range of elements in the List that 
    /// extends from the specified index to the last element.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) 
    /// for reference types.</param>
    /// <param name="index">The zero-based starting index of the search.</param>
    /// <returns>The zero-based index of the first occurrence of item within 
    /// the entire List, if found; otherwise, �1.</returns>
    public int IndexOf(T item, int index)
    {
      if (index > this.m_size) { throw new ArgumentOutOfRangeException("index"); }
      return Array.IndexOf<T>(this.m_items, item, index, this.m_size - index);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first 
    /// occurrence within the range of elements in the List that starts at the specified 
    /// index and contains the specified number of elements.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) 
    /// for reference types.</param>
    /// <param name="index">The zero-based starting index of the search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <returns>The zero-based index of the first occurrence of item within 
    /// the entire List, if found; otherwise, �1.</returns>
    public int IndexOf(T item, int index, int count)
    {
      if (index > this.m_size) { throw new ArgumentOutOfRangeException("index"); }
      if ((count < 0) || (index > (this.m_size - count)))
      {
        throw new ArgumentOutOfRangeException("count");
      }

      return Array.IndexOf<T>(this.m_items, item, index, count);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based 
    /// index of the last occurrence within the entire List.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <returns>The zero-based index of the last occurrence of item within 
    /// the entire the List, if found; otherwise, �1.</returns>
    public int LastIndexOf(T item)
    {
      return this.LastIndexOf(item, this.m_size - 1, this.m_size);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index 
    /// of the last occurrence within the range of elements in the List 
    /// that extends from the first element to the specified index.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <returns>The zero-based index of the last occurrence of item within 
    /// the entire the List, if found; otherwise, �1.</returns>
    public int LastIndexOf(T item, int index)
    {
      if (index >= this.m_size) { throw new ArgumentOutOfRangeException("index"); }
      return this.LastIndexOf(item, index, index + 1);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the 
    /// last occurrence within the range of elements in the List that contains 
    /// the specified number of elements and ends at the specified index.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <returns>The zero-based index of the last occurrence of item within 
    /// the entire the List, if found; otherwise, �1.</returns>
    public int LastIndexOf(T item, int index, int count)
    {
      if (this.m_size == 0) { return -1; }
      if ((index < 0) || (count < 0))
        throw new ArgumentOutOfRangeException("index");

      if ((index >= this.m_size) || (count > (index + 1)))
        throw new ArgumentOutOfRangeException("index");

      return Array.LastIndexOf<T>(this.m_items, item, index, count);
    }

    /// <summary>
    /// Searches the entire sorted List for an element using the default comparer 
    /// and returns the zero-based index of the element.
    /// </summary>
    /// <param name="item">The object to locate. The value can be a null reference 
    /// (Nothing in Visual Basic) for reference types.</param>
    /// <returns>The zero-based index of item in the sorted List, if item is found; 
    /// otherwise, a negative number that is the bitwise complement of the index 
    /// of the next element that is larger than item or, if there is no larger element, 
    /// the bitwise complement of Count.</returns>
    public int BinarySearch(T item)
    {
      return this.BinarySearch(0, this.Count, item, null);
    }

    /// <summary>
    /// Searches the entire sorted List for an element using the specified 
    /// comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="item">The object to locate. The value can be a null reference 
    /// (Nothing in Visual Basic) for reference types.</param>
    /// <param name="comparer">The IComparer(T) implementation to use when comparing elements.
    /// Or a null reference (Nothing in Visual Basic) to use the default comparer 
    /// Comparer(T)::Default.</param>
    /// <returns>The zero-based index of item in the sorted List, if item is found; 
    /// otherwise, a negative number that is the bitwise complement of the index 
    /// of the next element that is larger than item or, if there is no larger element, 
    /// the bitwise complement of Count.</returns>
    public int BinarySearch(T item, IComparer<T> comparer)
    {
      return this.BinarySearch(0, this.Count, item, comparer);
    }

    /// <summary>
    /// Searches the entire sorted List for an element using the specified 
    /// comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to search.</param>
    /// <param name="count">The length of the range to search.</param>
    /// <param name="item">The object to locate. The value can be a null reference 
    /// (Nothing in Visual Basic) for reference types.</param>
    /// <param name="comparer">The IComparer(T) implementation to use when comparing elements.
    /// Or a null reference (Nothing in Visual Basic) to use the default comparer 
    /// Comparer(T)::Default.</param>
    /// <returns>The zero-based index of item in the sorted List, if item is found; 
    /// otherwise, a negative number that is the bitwise complement of the index 
    /// of the next element that is larger than item or, if there is no larger element, 
    /// the bitwise complement of Count.</returns>
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
      if (index < 0) { throw new ArgumentOutOfRangeException("index"); }
      if (count < 0) { throw new ArgumentOutOfRangeException("count"); }

      if ((this.m_size - index) < count)
      {
        throw new ArgumentException("This combination of index and count is not valid");
      }

      return Array.BinarySearch<T>(this.m_items, index, count, item, comparer);
    }

    /// <summary>
    /// Determines whether an element is in the List.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <returns>true if item is found in the List; otherwise, false.</returns>
    public bool Contains(T item)
    {
      if (item == null)
      {
        for (int j = 0; j < this.m_size; j++)
        {
          if (this.m_items[j] == null)
          {
            return true;
          }
        }
        return false;
      }

      EqualityComparer<T> comparer = EqualityComparer<T>.Default;
      for (int i = 0; i < this.m_size; i++)
      {
        if (comparer.Equals(this.m_items[i], item))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines whether an element is in the List.
    /// </summary>
    /// <param name="item">The object to locate in the List. 
    /// The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
    /// <returns>true if item is found in the List; otherwise, false.</returns>
    bool IList.Contains(object item)
    {
      return (RhinoList<T>.IsCompatibleObject(item) && this.Contains((T)item));
    }

    /// <summary>
    /// Determines whether the List contains elements that match the 
    /// conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the elements to search for.</param>
    /// <returns>true if the List contains one or more elements that match the 
    /// conditions defined by the specified predicate; otherwise, false.</returns>
    public bool Exists(Predicate<T> match)
    {
      return (this.FindIndex(match) != -1);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the first occurrence within the entire List.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The first element that matches the conditions defined by the specified predicate, 
    /// if found; otherwise, the default value for type T.</returns>
    public T Find(Predicate<T> match)
    {
      if (match == null) { throw new ArgumentNullException("match"); }

      for (int i = 0; i < this.m_size; i++)
      {
        if (match(this.m_items[i]))
        {
          return this.m_items[i];
        }
      }
      return default(T);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the last occurrence within the entire List.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The last element that matches the conditions defined by the specified predicate, 
    /// if found; otherwise, the default value for type T.</returns>
    public T FindLast(Predicate<T> match)
    {
      if (match == null) { throw new ArgumentNullException("match"); }

      for (int i = this.m_size - 1; i >= 0; i--)
      {
        if (match(this.m_items[i]))
        {
          return this.m_items[i];
        }
      }
      return default(T);
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the elements to search for.</param>
    /// <returns>A ON_List(T) containing all the elements that match the conditions 
    /// defined by the specified predicate, if found; otherwise, an empty ON_List(T).</returns>
    public RhinoList<T> FindAll(Predicate<T> match)
    {
      if (match == null) { throw new ArgumentNullException("match"); }

      RhinoList<T> list = new RhinoList<T>(this.m_size);
      for (int i = 0; i < this.m_size; i++)
      {
        if (match(this.m_items[i]))
        {
          list.Add(this.m_items[i]);
        }
      }

      list.TrimExcess();
      return list;
    }

    /// <summary>
    /// Determines whether every element in the List matches the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions to check against the elements.</param>
    /// <returns>true if every element in the List matches the conditions defined by 
    /// the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
    public bool TrueForAll(Predicate<T> match)
    {
      if (match == null) { throw new ArgumentNullException("match"); }

      for (int i = 0; i < this.m_size; i++)
      {
        if (!match(this.m_items[i]))
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Performs the specified action on each element of the List.
    /// </summary>
    /// <param name="action">The Action(T) delegate to perform on each element of the List.</param>
    public void ForEach(Action<T> action)
    {
      if (action == null) { throw new ArgumentNullException("action"); }
      for (int i = 0; i < this.m_size; i++)
      {
        action(this.m_items[i]);
      }
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the zero-based index of the first 
    /// occurrence within the entire List.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first occurrence of an element that 
    /// matches the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindIndex(Predicate<T> match)
    {
      return this.FindIndex(0, this.m_size, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the zero-based index of the first 
    /// occurrence within the entire List.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first occurrence of an element that 
    /// matches the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindIndex(int startIndex, Predicate<T> match)
    {
      return this.FindIndex(startIndex, this.m_size - startIndex, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, 
    /// and returns the zero-based index of the first occurrence within the range of elements 
    /// in the List that extends from the specified index to the last element.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first occurrence of an element that 
    /// matches the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
      if (startIndex > this.m_size) { throw new ArgumentOutOfRangeException("count"); }
      if (count < 0) { throw new ArgumentOutOfRangeException("count"); }
      if (startIndex > (this.m_size - count)) { throw new ArgumentOutOfRangeException("count"); }

      if (match == null) { throw new ArgumentNullException("match"); }

      int num = startIndex + count;
      for (int i = startIndex; i < num; i++)
      {
        if (match(this.m_items[i]))
        {
          return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the zero-based index of the last 
    /// occurrence within the entire List.
    /// </summary>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last occurrence of an element that matches 
    /// the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindLastIndex(Predicate<T> match)
    {
      return this.FindLastIndex(this.m_size - 1, this.m_size, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the zero-based index of the last 
    /// occurrence within the entire List.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last occurrence of an element that matches 
    /// the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
      // avoid overflow
      if (startIndex == int.MaxValue)
        throw new ArgumentOutOfRangeException("startIndex", "startIndex must be less than Int32.MaxValue");

      return this.FindLastIndex(startIndex, startIndex + 1, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the 
    /// specified predicate, and returns the zero-based index of the last 
    /// occurrence within the entire List.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <param name="match">The Predicate(T) delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last occurrence of an element that matches 
    /// the conditions defined by match, if found; otherwise, �1.</returns>
    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
      if (match == null) { throw new ArgumentNullException("match"); }
      if (this.m_size == 0)
      {
        if (startIndex != -1)
        {
          throw new ArgumentOutOfRangeException("startIndex");
        }
      }
      else if (startIndex >= this.m_size)
      {
        throw new ArgumentOutOfRangeException("startIndex");
      }

      if (count < 0) { throw new ArgumentOutOfRangeException("count"); }
      if (((startIndex - count) + 1) < 0) { throw new ArgumentOutOfRangeException("startIndex"); }

      int num = startIndex - count;
      for (int i = startIndex; i > num; i--)
      {
        if (match(this.m_items[i]))
        {
          return i;
        }
      }
      return -1;
    }
    #endregion

    #region Sorting
    /// <summary>
    /// Sorts the elements in the entire List using the default comparer.
    /// </summary>
    public void Sort()
    {
      this.Sort(0, this.Count, null);
    }

    /// <summary>
    /// Sorts the elements in the entire list using the specified System.Comparison(T)
    /// </summary>
    /// <param name="comparer">The IComparer(T) implementation to use when comparing elements, 
    /// or a null reference (Nothing in Visual Basic) to use the default comparer Comparer(T).Default.</param>
    public void Sort(IComparer<T> comparer)
    {
      this.Sort(0, this.Count, comparer);
    }

    /// <summary>
    /// Sorts the elements in the entire list using the specified comparer.
    /// </summary>
    /// <param name="comparison">The System.Comparison(T) to use when comparing elements.</param>
    public void Sort(Comparison<T> comparison)
    {
      if (comparison == null) { throw new ArgumentNullException("comparison"); }
      if (this.m_size > 0)
      {
        IComparer<T> comparer = new FunctorComparer<T>(comparison);
        Array.Sort<T>(this.m_items, 0, this.m_size, comparer);
      }
    }

    /// <summary>
    /// Sorts the elements in a range of elements in list using the specified comparer.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to sort.</param>
    /// <param name="count">The length of the range to sort.</param>
    /// <param name="comparer">The IComparer(T) implementation to use when comparing 
    /// elements, or a null reference (Nothing in Visual Basic) to use the default 
    /// comparer Comparer(T).Default.</param>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
      if ((index < 0) || (count < 0))
        throw new ArgumentOutOfRangeException("index");

      if ((this.m_size - index) < count)
        throw new ArgumentException("index and count are not a valid combination");

      Array.Sort<T>(this.m_items, index, count, comparer);
      this.m_version++;
    }

    /// <summary>
    /// Sort this list based on a list of numeric keys of equal length. 
    /// The keys array will not be altered.
    /// </summary>
    /// <param name="keys">Numeric keys to sort with.</param>
    /// <remarks>This function does not exist on the DotNET List class. 
    /// David thought it was a good idea.</remarks>
    public void Sort(double[] keys)
    {
      if (keys == null) { throw new ArgumentNullException("keys"); }
      if (keys.Length != this.m_size)
      {
        throw new ArgumentException("Keys array must have same length as this List.");
      }

      //cannot sort 1 item or less...
      if (this.m_size < 2) { return; }

      //trim my internal array
      this.Capacity = this.m_size;

      //duplicate the keys array
      double[] copy_keys = (double[])keys.Clone();
      Array.Sort(copy_keys, this.m_items);

      m_version++;
    }

    /// <summary>
    /// Sort this list based on a list of numeric keys of equal length. 
    /// The keys array will not be altered.
    /// </summary>
    /// <param name="keys">Numeric keys to sort with.</param>
    /// <remarks>This function does not exist on the DotNET List class. 
    /// David thought it was a good idea.</remarks>
    public void Sort(int[] keys)
    {
      if (keys == null) { throw new ArgumentNullException("keys"); }
      if (keys.Length != this.m_size)
      {
        throw new ArgumentException("Keys array must have same length as this List.");
      }

      //cannot sort 1 item or less...
      if (this.m_size < 2) { return; }

      //trim my internal array
      this.Capacity = this.m_size;

      //duplicate the keys array
      int[] copy_keys = (int[])keys.Clone();
      Array.Sort(copy_keys, this.m_items);

      m_version++;
    }

    /// <summary>
    /// Utility class which ties together functionality in Comparer(T) and Comparison(T)
    /// </summary>
    private sealed class FunctorComparer<Q> : IComparer<Q>
    {
      //private Comparer<Q> c;
      private Comparison<Q> m_comparison;

      public FunctorComparer(Comparison<Q> comparison)
      {
        //this.c = Comparer<Q>.Default;
        m_comparison = comparison;
      }

      public int Compare(Q x, Q y)
      {
        return m_comparison(x, y);
      }
    }

    /// <summary>
    /// Reverses the order of the elements in the entire List.
    /// </summary>
    public void Reverse()
    {
      this.Reverse(0, this.Count);
    }

    /// <summary>
    /// Reverses the order of the elements in the specified range.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to reverse.</param>
    /// <param name="count">The number of elements in the range to reverse.</param>
    public void Reverse(int index, int count)
    {
      if ((index < 0) || (count < 0))
        throw new ArgumentOutOfRangeException("index");

      if ((this.m_size - index) < count)
        throw new ArgumentOutOfRangeException("index");

      Array.Reverse(this.m_items, index, count);
      this.m_version++;
    }
    #endregion

    #region Duplication and Conversion
    public ReadOnlyCollection<T> AsReadOnly()
    {
      return new ReadOnlyCollection<T>(this);
    }
    public RhinoList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
      if (converter == null) { throw new ArgumentNullException("converter"); }

      RhinoList<TOutput> list = new RhinoList<TOutput>(this.m_size);
      for (int i = 0; i < this.m_size; i++)
      {
        list.m_items[i] = converter(this.m_items[i]);
      }

      list.m_size = this.m_size;
      return list;
    }

    /// <summary>
    /// Copies the entire List to a compatible one-dimensional array, 
    /// starting at the beginning of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination 
    /// of the elements copied from List. The Array must have zero-based indexing.</param>
    public void CopyTo(T[] array)
    {
      this.CopyTo(array, 0);
    }

    /// <summary>
    /// Copies the entire List to a compatible one-dimensional array, 
    /// starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination 
    /// of the elements copied from List. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      Array.Copy(this.m_items, 0, array, arrayIndex, this.m_size);
    }

    /// <summary>
    /// Copies a range of elements from the List to a compatible one-dimensional array, 
    /// starting at the specified index of the target array.
    /// </summary>
    /// <param name="index">The zero-based index in the source List at which copying begins.</param>
    /// <param name="array">The one-dimensional Array that is the destination of the elements 
    /// copied from List. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <param name="count">The number of elements to copy.</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      if ((m_size - index) < count)
      {
        throw new ArgumentOutOfRangeException("index");
      }
      Array.Copy(this.m_items, index, array, arrayIndex, count);
    }

    /// <summary>
    /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements 
    /// copied from ICollection. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      if ((array != null) && (array.Rank != 1))
      {
        throw new ArgumentException("Multidimensional target arrays not supported");
      }
      try
      {
        Array.Copy(this.m_items, 0, array, arrayIndex, this.m_size);
      }
      catch (ArrayTypeMismatchException)
      {
        throw new ArgumentException("Invalid array type");
      }
    }
    #endregion
    #endregion

    #region Enumeration
    public IEnumerator GetEnumerator()
    {
      return new Enumerator((RhinoList<T>)this);
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator((RhinoList<T>)this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator((RhinoList<T>)this);
    }

    private class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
    {
      private RhinoList<T> list;
      private int index;
      private int version;
      private T current;

      internal Enumerator(RhinoList<T> list)
      {
        this.list = list;
        this.index = 0;
        this.version = list.m_version;
        this.current = default(T);
      }

      public void Dispose()
      {
        GC.SuppressFinalize(this);
      }

      public bool MoveNext()
      {
        RhinoList<T> list = this.list;
        if ((this.version == list.m_version) && (this.index < list.m_size))
        {
          this.current = list.m_items[this.index];
          this.index++;
          return true;
        }
        return this.MoveNextRare();
      }
      private bool MoveNextRare()
      {
        if (this.version != this.list.m_version)
        {
          throw new InvalidOperationException("State of RhinoList changed during enumeration");
        }

        this.index = this.list.m_size + 1;
        this.current = default(T);
        return false;
      }

      public T Current
      {
        get
        {
          return this.current;
        }
      }
      object IEnumerator.Current
      {
        get
        {
          if ((this.index == 0) || (this.index == (this.list.m_size + 1)))
          {
            throw new InvalidOperationException("Enum operation cannot happen");
          }
          return this.Current;
        }
      }
      void IEnumerator.Reset()
      {
        if (this.version != this.list.m_version)
        {
          throw new InvalidOperationException("State of RhinoList changed during enumeration");
        }
        this.index = 0;
        this.current = default(T);
      }
    }
    #endregion
  }

  /// <summary>
  /// Utility class for displaying ON_List contents in the VS debugger.
  /// </summary>
  internal class ListDebuggerDisplayProxy<T>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICollection<T> collection;

    public ListDebuggerDisplayProxy(ICollection<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");
      this.collection = collection;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
      get
      {
        T[] array = new T[this.collection.Count];
        this.collection.CopyTo(array, 0);
        return array;
      }
    }
  }

  public class Point3dList : RhinoList<Point3d>
  {
    public Point3dList()
      : base()
    {
    }
    /// <summary>
    /// Create a new pointlist with a preallocated initial capacity.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscurve.py' lang='py'/>
    /// </example>
    public Point3dList(int initialCapacity)
      : base(initialCapacity)
    {
    }
    public Point3dList(IEnumerable<Point3d> collection)
      : base(collection)
    {
    }

    /// <summary>
    /// Construct a new Point3dList from a given number of points.
    /// </summary>
    /// <param name="initialPoints">Points to add to the list.</param>
    public Point3dList(params Point3d[] initialPoints)
      : base()
    {
      if (initialPoints != null)
      {
        AddRange(initialPoints);
      }
    }

    internal static Point3dList FromNativeArray(Runtime.InteropWrappers.SimpleArrayPoint3d pts)
    {
      if (null == pts)
        return null;
      int count = pts.Count;
      Point3dList list = new Point3dList(count);
      if (count > 0)
      {
        IntPtr pNativeArray = pts.ConstPointer();
        UnsafeNativeMethods.ON_3dPointArray_CopyValues(pNativeArray, list.m_items);
        list.m_size = count;
      }
      return list;
    }

    /// <summary>
    /// Anything calling this function should not be modifying the contents of the array
    /// </summary>
    /// <param name="points"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    internal static Point3d[] GetConstPointArray(IEnumerable<Point3d> points, out int count)
    {
      count = 0;
      RhinoList<Point3d> pointlist = points as RhinoList<Point3d>;
      if (null != pointlist)
      {
        count = pointlist.m_size;
        return pointlist.m_items;
      }

      Point3d[] pointarray = points as Point3d[];
      if (null != pointarray)
      {
        count = pointarray.Length;
        return pointarray;
      }

      IList<Point3d> genericpointlist = points as IList<Point3d>;
      if (null != genericpointlist)
      {
        count = genericpointlist.Count;
        Point3d[] p = new Point3d[count];
        genericpointlist.CopyTo(p, 0);
        return p;
      }

      // couldn't figure out what this thing is, just use the iterator
      List<Point3d> list = new List<Point3d>();
      foreach (Point3d pt in points)
      {
        list.Add(pt);
      }
      count = list.Count;
      return list.ToArray();
    }

    #region Properties
    public BoundingBox BoundingBox
    {
      get
      {
        if (m_size == 0) { return BoundingBox.Empty; }

        double x0 = double.MaxValue;
        double x1 = double.MinValue;
        double y0 = double.MaxValue;
        double y1 = double.MinValue;
        double z0 = double.MaxValue;
        double z1 = double.MinValue;

        for (int i = 0; i < m_size; i++)
        {
          x0 = Math.Min(x0, m_items[i].X);
          x1 = Math.Max(x1, m_items[i].X);
          y0 = Math.Min(y0, m_items[i].Y);
          y1 = Math.Max(y1, m_items[i].Y);
          z0 = Math.Min(z0, m_items[i].Z);
          z1 = Math.Max(z1, m_items[i].Z);
        }

        return new BoundingBox(new Point3d(x0, y0, z0), new Point3d(x1, y1, z1));
      }
    }

    /// <summary>
    /// Find the index of the point that is closest to a test point in this list
    /// </summary>
    /// <param name="testPoint">point to compare against</param>
    /// <returns>index of closest point in the list on success. -1 on error</returns>
    public int ClosestIndex(Point3d testPoint)
    {
      return ClosestIndexInList(this, testPoint);
    }

    #region "Coordinate access"
    internal XAccess m_x_access;
    internal YAccess m_y_access;
    internal ZAccess m_z_access;

    public XAccess X
    {
      get
      {
        if (null == m_x_access)
          m_x_access = new XAccess(this);
        return m_x_access;
      }
    }
    public YAccess Y
    {
      get
      {
        if (null == m_y_access)
          m_y_access = new YAccess(this);
        return m_y_access;
      }
    }
    public ZAccess Z
    {
      get
      {
        if (null == m_z_access)
          m_z_access = new ZAccess(this);
        return m_z_access;
      }
    }

    /// <summary>
    /// Utility class for easy-access of x-components of points inside an ON_3dPointList.
    /// </summary>
    public class XAccess
    {
      private Point3dList m_owner;

      /// <summary>
      /// XAccess constructor. 
      /// </summary>
      internal XAccess(Point3dList owner)
      {
        if (owner == null) { throw new ArgumentNullException("owner"); }
        m_owner = owner;
      }

      /// <summary>
      /// Gets or sets the x-coordinate of the specified point.
      /// </summary>
      /// <param name="index">Index of point.</param>
      public double this[int index]
      {
        get { return m_owner.m_items[index].X; }
        set { m_owner.m_items[index].X = value; }
      }
    }

    /// <summary>
    /// Utility class for easy-access of x-components of points inside an ON_3dPointList.
    /// </summary>
    public class YAccess
    {
      private Point3dList m_owner;

      /// <summary>
      /// XAccess constructor. 
      /// </summary>
      internal YAccess(Point3dList owner)
      {
        if (owner == null) { throw new ArgumentNullException("owner"); }
        m_owner = owner;
      }

      /// <summary>
      /// Gets or sets the y-coordinate of the specified point.
      /// </summary>
      /// <param name="index">Index of point.</param>
      public double this[int index]
      {
        get { return m_owner.m_items[index].Y; }
        set { m_owner.m_items[index].Y = value; }
      }
    }

    /// <summary>
    /// Utility class for easy-access of z-components of points inside an ON_3dPointList.
    /// </summary>
    public class ZAccess
    {
      private Point3dList m_owner;

      /// <summary>
      /// XAccess constructor. 
      /// </summary>
      internal ZAccess(Point3dList owner)
      {
        if (owner == null) { throw new ArgumentNullException("owner"); }
        m_owner = owner;
      }

      /// <summary>
      /// Gets or sets the z-coordinate of the specified point.
      /// </summary>
      /// <param name="index">Index of point.</param>
      public double this[int index]
      {
        get { return m_owner.m_items[index].Z; }
        set { m_owner.m_items[index].Z = value; }
      }
    }
    #endregion
    #endregion

    #region methods
    /// <summary>
    /// Adds a ON_3dPoint to the end of the List with given x,y,z coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscurve.py' lang='py'/>
    /// </example>
    public void Add(double x, double y, double z)
    {
      Add(new Point3d(x, y, z));
    }

    /// <summary>
    /// Apply a transform to all the points in the list.
    /// </summary>
    /// <param name="xform">Transform to apply.</param>
    public void Transform(Transform xform)
    {
      for (int i = 0; i < Count; i++)
      {
        //David: changed this on April 3rd 2010, Transform acts on the point directly.
        m_items[i].Transform(xform);
      }
    }

    /// <summary>
    /// Find the index of the point in a list of points that is closest to a test point.
    /// </summary>
    /// <param name="list">A list of points.</param>
    /// <param name="testPoint">Point to compare against.</param>
    /// <returns>Index of closest point in the list on success or -1 on error.</returns>
    public static int ClosestIndexInList(IList<Point3d> list, Point3d testPoint)
    {
      if (null == list || !testPoint.IsValid)
        return -1;

      double min_d = double.MaxValue;
      int min_i = -1;
      int count = list.Count;
      for (int i = 0; i < count; i++)
      {
        Point3d p = list[i];
        double dSquared = (p.X - testPoint.X) * (p.X - testPoint.X) +
                   (p.Y - testPoint.Y) * (p.Y - testPoint.Y) +
                   (p.Z - testPoint.Z) * (p.Z - testPoint.Z);

        //quick abort in case of exact match
        if (dSquared == 0.0)
          return i;
        if (dSquared < min_d)
        {
          min_d = dSquared;
          min_i = i;
        }
      }

      return min_i;
    }

    /// <summary>
    /// Find the point in a list of points that is closest to a test point.
    /// </summary>
    /// <param name="list">A list of points.</param>
    /// <param name="testPoint">Point to compare against.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// list must contain at least one point and testPoint must be valid
    /// </exception>
    public static Point3d ClosestPointInList(IList<Point3d> list, Point3d testPoint)
    {
      if (list.Count < 1 || !testPoint.IsValid)
        throw new ArgumentException("list must contain at least one point and testPoint must be valid");
      int index = ClosestIndexInList(list, testPoint);
      return list[index];
    }
    #endregion
  }

  public class CurveList : RhinoList<Curve>
  {
    public CurveList()
      : base()
    {
    }
    public CurveList(int initialCapacity)
      : base(initialCapacity)
    {
    }
    public CurveList(IEnumerable<Curve> collection)
      : base(collection)
    {
    }

    #region Addition Overloads
    public void Add(Line line)
    {
      base.Add(new LineCurve(line));
    }
    public void Add(Circle circle)
    {
      base.Add(new ArcCurve(circle));
    }
    public void Add(Arc arc)
    {
      base.Add(new ArcCurve(arc));
    }

    //TODO!
    //public void Add(ON_3dPointList polyline)
    //{
    //  ...
    //}

    //TODO!
    //public void Add(ON_Ellipse ellipse)
    //{
    //  ...
    //}

    #endregion

    #region Insertion overloads
    public void Insert(int index, Line line)
    {
      base.Insert(index, new LineCurve(line));
    }
    public void Insert(int index, Circle circle)
    {
      base.Insert(index, new ArcCurve(circle));
    }
    public void Insert(int index, Arc arc)
    {
      base.Insert(index, new ArcCurve(arc));
    }

    //TODO!
    //public void Insert(int index, ON_3dPointList polyline)
    //{
    //  ...
    //}

    //TODO!
    //public void Insert(int index, ON_Ellipse ellipse)
    //{
    //  ...
    //}
    #endregion

    #region Geometry utilities
    /// <summary>
    /// Transform all the curves in this list. If at least a single transform failed 
    /// this function returns False.
    /// </summary>
    /// <param name="xform">Transformation to apply to all curves.</param>
    public bool Transform(Transform xform)
    {
      bool rc = true;

      foreach (Curve crv in this)
      {
        if (crv == null) { continue; }
        if (!crv.Transform(xform)) { rc = false; }
      }

      return rc;
    }
    #endregion
  }
}