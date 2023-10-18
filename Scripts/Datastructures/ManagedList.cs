using Godot;
using System;
using System.Collections.Generic;

#nullable enable

public class ManagedList<T>: IEnumerable<T>
{
	// The current implementation leaks memory since it's not given back when elements are removed. This can be
	// mitigated by moving to a sparse list implementation, where the list would be subdivided into groups with higher
	// element density as needed.
	private readonly List<T?> values;
	private readonly Action<int, T>? onAdd;
	private readonly Action<int, T>? onRemove;

	public int Count { get { return values.Count; } }

	public ManagedList(List<T?> values, Action<int, T>? onAdd = null, Action<int, T>? onRemove = null)
	{
		this.values = values;
		this.onAdd = onAdd;
		this.onRemove = onRemove;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return values.GetEnumerator();
	}
	
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

	public int Add(T value)
	{
		/// <summary>
		/// Adds an element to the managed list. The onAdd function is triggered after the value has been added.
		/// </summary>
		/// <returns>
		/// The id of the element in the list. This is used to ensure only one reference exists, allows serialization
		/// without having to convert references, 
		int retVal = values.Count;
		values.Add(value);
        onAdd?.Invoke(retVal, value);
        return retVal;
	}

	public void Remove(int idx)
	{
		T? value = values[idx];
		if (value != null)
		{
			onRemove?.Invoke(idx, value);
			values[idx] = default;
		}
	}

	public T? Get(int idx)
	{
		return values[idx];
	}
}
