using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{
    private List<KeyValuePair<T, int>> elements = new List<KeyValuePair<T, int>>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add(new KeyValuePair<T, int>(item, priority));
        elements.Sort((x, y) => x.Value.CompareTo(y.Value)); // Keep elements sorted by priority
    }

    public T Dequeue()
    {
        var item = elements[0];
        elements.RemoveAt(0);
        return item.Key;
    }

    public bool Contains(T item)
    {
        return elements.Exists(e => EqualityComparer<T>.Default.Equals(e.Key, item));
    }
}

