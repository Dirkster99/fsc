﻿namespace FolderBrowser.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Source: https://stackoverflow.com/questions/5487927/expand-wpf-treeview-to-support-sorting
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SortableObservableCollection<T> : ObservableCollection<T>
	{
		/// <summary>
		/// Class constructor.
		/// </summary>
		public SortableObservableCollection() : base() { }

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="l"></param>
		public SortableObservableCollection(List<T> l) : base(l) { }

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="l"></param>
		public SortableObservableCollection(IEnumerable<T> l) : base(l) { }

		#region Sorting

		/// <summary>
		/// Sorts the items of the collection in ascending order according to a key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="keySelector">A function to extract a key from an item.</param>
		public void Sort<TKey>(Func<T, TKey> keySelector)
		{
			InternalSort(Items.OrderBy(keySelector));
		}

		/// <summary>
		/// Sorts the items of the collection in descending order according to a key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="keySelector">A function to extract a key from an item.</param>
		public void SortDescending<TKey>(Func<T, TKey> keySelector)
		{
			InternalSort(Items.OrderByDescending(keySelector));
		}

		/// <summary>
		/// Sorts the items of the collection in ascending order according to a key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="keySelector">A function to extract a key from an item.</param>
		/// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
		public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			InternalSort(Items.OrderBy(keySelector, comparer));
		}

		/// <summary>
		/// Moves the items of the collection so that their orders are the same as those of the items provided.
		/// </summary>
		/// <param name="sortedItems">An <see cref="IEnumerable{T}"/> to provide item orders.</param>
		private void InternalSort(IEnumerable<T> sortedItems)
		{
			var sortedItemsList = sortedItems.ToList();

			foreach (var item in sortedItemsList)
			{
				Move(IndexOf(item), sortedItemsList.IndexOf(item));
			}
		}

		#endregion // Sorting
	}
}
