namespace FileSystemModels.ViewModels.Collections.Generics
{
	using System.Collections.Generic;

	/// <summary>
	/// Class implements a sortable observable disctionary object that can be used to
	/// key templated items (of any class) with a string and bind them to a view.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SortableObservableDictionaryCollection<T> : SortableObservableCollection<T>
	{
		#region fields
		private readonly Dictionary<string, T> _dictionary = null;
		#endregion fields

		#region constructors
		/// <summary>
		/// class constructor
		/// </summary>
		public SortableObservableDictionaryCollection()
		{
			_dictionary = new Dictionary<string, T>();
		}
		#endregion constructors

		#region methods
		/// <summary>
		/// This method is not implemented and will throw a <seealso cref="System.NotSupportedException"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool AddItem(T item)
		{
			throw new System.NotSupportedException("Use alternative API: AddItem(string, T)");
		}

		/// <summary>
		/// Adds an item with the key as string and will throw an exception if the ToLower()
		/// string is already present in the collection of keys.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool AddItem(string key, T item)
		{
			if (string.IsNullOrEmpty(key) == true)
				_dictionary.Add(string.Empty, item);
			else
				_dictionary.Add(key.ToLower(), item);

			this.Add(item);

			return true;
		}

		/// <summary>
		/// This method is not implemented and will throw a <seealso cref="System.NotSupportedException"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool RemoveItem(T item)
		{
			throw new System.NotSupportedException("Use alternative API: RemoveItem(string, T)");
		}

		/// <summary>
		/// Removes the item with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool RemoveItem(string key, T item)
		{
			_dictionary.Remove(key.ToLower());
			this.Remove(item);

			return true;
		}

		/// <summary>
		/// Attempts to find a given item by its key value.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public T TryGet(string key)
		{
			T o;

			if (_dictionary.TryGetValue(key.ToLower(), out o))
				return o;

			return default(T);
		}

		/// <summary>
		/// Removes all items from the current collections.
		/// </summary>
		public new void Clear()
		{
			_dictionary.Clear();
			base.Clear();
		}
		#endregion methods
	}
}
