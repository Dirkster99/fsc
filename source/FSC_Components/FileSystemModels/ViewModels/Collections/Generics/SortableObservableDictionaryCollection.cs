namespace FileSystemModels.ViewModels.Collections.Generics
{
    using System.Collections.Generic;

    public class SortableObservableDictionaryCollection<T> : SortableObservableCollection<T>
    {
        Dictionary<string, T> _dictionary = null;

        public SortableObservableDictionaryCollection()
        {
            _dictionary = new Dictionary<string, T>();
        }

        public bool AddItem(T item)
        {
            throw new System.NotSupportedException("Use alternative API: AddItem(string, T)");
        }

        public bool AddItem(string key, T item)
        {
            if (string.IsNullOrEmpty(key) == true)
                _dictionary.Add(string.Empty, item);
            else
                _dictionary.Add(key.ToLower(), item);

            this.Add(item);

            return true;
        }

        public bool RemoveItem(T item)
        {
            throw new System.NotSupportedException("Use alternative API: RemoveItem(string, T)");
        }

        public bool RemoveItem(string key, T item)
        {
            _dictionary.Remove(key.ToLower());
            this.Remove(item);

            return true;
        }

        public T TryGet(string key)
        {
            T o;

            if (_dictionary.TryGetValue(key.ToLower(), out o))
                return o;

            return default(T);
        }

        public new void Clear()
        {
            _dictionary.Clear();
            base.Clear();
        }
    }
}
