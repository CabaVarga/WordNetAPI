using System;
using System.Collections;
using System.Collections.Generic;

namespace LAIR.Collections.Generic
{
    /// <summary>
    /// A set of unique elements. Drop-in replacement for the <c>LAIR.Collections.Generic.Set&lt;T&gt;</c>
    /// type formerly provided by the external LAIR.Collections DLL. Backed by <see cref="HashSet{T}"/>.
    /// Only the members actually used by WordNetEngine and SynSet are implemented.
    /// </summary>
    public class Set<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _items;
        private bool _isReadOnly;

        /// <summary>
        /// Gets or sets whether this set is read-only. When true, mutating operations
        /// (<see cref="Add"/> and <see cref="AddRange"/>) throw <see cref="InvalidOperationException"/>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        /// <summary>
        /// Gets the number of elements in this set.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Creates an empty set.
        /// </summary>
        public Set()
        {
            _items = new HashSet<T>();
        }

        /// <summary>
        /// Creates an empty set. The <paramref name="capacity"/> hint is accepted for API
        /// compatibility but not forwarded (the net40 HashSet lacks a capacity constructor).
        /// </summary>
        public Set(int capacity)
        {
            _items = new HashSet<T>();
        }

        /// <summary>
        /// Creates a set populated with the elements of <paramref name="elements"/>.
        /// </summary>
        public Set(ICollection<T> elements)
        {
            _items = new HashSet<T>(elements);
        }

        /// <summary>
        /// Creates an empty set. The <paramref name="throwExceptionOnDuplicateAdd"/> parameter
        /// is accepted for API compatibility but not enforced; duplicates are silently ignored,
        /// matching <see cref="HashSet{T}"/> semantics.
        /// </summary>
        public Set(bool throwExceptionOnDuplicateAdd)
        {
            _items = new HashSet<T>();
        }

        /// <summary>
        /// Adds an element to this set.
        /// </summary>
        /// <returns>True if the element was added; false if it was already present.</returns>
        public bool Add(T item)
        {
            ThrowIfReadOnly();
            return _items.Add(item);
        }

        /// <summary>
        /// Adds a range of elements to this set. Duplicates are silently skipped.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            ThrowIfReadOnly();
            foreach (T item in items)
                _items.Add(item);
        }

        /// <summary>
        /// Checks whether this set contains <paramref name="item"/>.
        /// </summary>
        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowIfReadOnly()
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Set is read-only.");
        }
    }
}
