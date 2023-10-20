using Godot;
using System;
using System.Collections.Generic;

#nullable enable

/// <summary>
/// ManagedList abstracts away a data oriented pattern of storing indices instead of references and can be set up to
/// trigger additional code when elements are added or removed from the list. An ideal use case of a managed list is
/// for storing small elements that are frequently iterated over. This is more efficient than random access and makes
/// better use of the CPU's memory bandwidth.
/// </summary>
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
        return values.GetEnumerator();
    }

	/// <summary>
	/// Adds an element to the managed list. The onAdd function is triggered after the value has been added.
	/// </summary>
	/// <returns>
	/// The id of the element in the list. This is used to ensure only one reference exists and allows serialization
	/// without having to convert references.
	/// </returns>
	public int Add(T value)
	{
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
