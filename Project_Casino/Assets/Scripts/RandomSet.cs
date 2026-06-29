using System;
using System.Collections.Generic;

public class RandomSet<T>
{
    private readonly List<T> _items = new List<T>();
    private readonly Random _random = new Random();

    public int Count => _items.Count;

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Add(T[] items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _items.AddRange(items);
    }

    public T Pop()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        int index = _random.Next(_items.Count);
        T value = _items[index];
        _items.RemoveAt(index);
        return value;
    }

    public T[] Pop(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count > _items.Count)
            throw new InvalidOperationException("Not enough elements in the collection.");

        T[] result = new T[count];

        for (int i = 0; i < count; i++)
        {
            int index = _random.Next(_items.Count);
            result[i] = _items[index];
            _items.RemoveAt(index);
        }

        return result;
    }

    public T Peek()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        int index = _random.Next(_items.Count);
        return _items[index];
    }
}
