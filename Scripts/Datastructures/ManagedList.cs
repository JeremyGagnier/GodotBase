using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

/// <summary>
/// This converter is necessary because JsonSerializer cannot serialize ManagedList because it is an IEnumerable.
/// </summary>
public class ManagedListJsonConverter<T> : JsonConverter<ManagedList<T>>
{
	public override ManagedList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		List<T?> values = JsonSerializer.Deserialize<List<T?>>(reader.GetString()!)!;
		ManagedList<T> managedList = new();
		// This is flaky because if values is renamed it will break.
		typeof(ManagedList<T>).GetField("values")!.SetValue(managedList, values);
		return managedList;
	}

	public override void Write(Utf8JsonWriter writer, ManagedList<T> managedList, JsonSerializerOptions options)
	{
		// This is flaky because if values is renamed it will break.
		List<T?> values = (List<T?>)typeof(ManagedList<T>).GetField("values")!.GetValue(managedList)!;
		string valuesString = JsonSerializer.Serialize(values);
		writer.WriteStringValue(valuesString);
	}
}

/// <summary>
/// ManagedList abstracts away a data oriented pattern of storing indices instead of references. An ideal use case of a
/// managed list is for storing small elements that are frequently iterated over. This is more efficient than random
/// access and makes better use of the CPU's memory bandwidth.
/// Another advantage of using indices is that they are directly serializable, whereas references are not.
/// </summary>
// Looks like this line complains at runtime because the generic type isn't known, but it works anyways??
[JsonConverter(typeof(ManagedListJsonConverter<>))]
public class ManagedList<T>: IEnumerable<T>
{
	// The current implementation leaks memory since it's not given back when elements are removed. This can be
	// mitigated by moving to a sparse list implementation, where the list would be subdivided into groups with higher
	// element density as needed.
	private readonly List<T?> values = new();

	public int Count { get { return values.Count; } }

	public IEnumerator<T> GetEnumerator()
	{
		return values.GetEnumerator();
	}

	public ManagedList() {}
	
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return values.GetEnumerator();
    }

	/// <summary>
	/// Adds an element to the managed list.
	/// </summary>
	/// <returns>
	/// The id of the element in the list. This is used to ensure only one reference exists and allows serialization
	/// without having to convert references.
	/// </returns>
	public int Add(T value)
	{
		int retVal = values.Count;
		values.Add(value);
        return retVal;
	}

	public void Remove(int idx)
	{
		T? value = values[idx];
		if (value != null)
		{
			values[idx] = default;
		}
	}

	public T? Get(int idx)
	{
		return values[idx];
	}

	public void Clear()
	{
		values.Clear();
	}
}
