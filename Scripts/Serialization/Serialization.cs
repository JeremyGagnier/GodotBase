using System.Reflection;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Godot;

#nullable enable

public static class Serialization
{
    private static readonly BindingFlags anyStatic =
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    private static bool HasJsonIncludeAttribute(MemberInfo member)
    {
        return member.GetCustomAttributes(typeof(JsonIncludeAttribute), false).Any();
    }

    public static string SerializeStaticClass<T>()
    {
        /// <summary>
        /// Attempts to serialize a static class into json, searching for properties and fields with the JsonInclude
        /// attribute.
        /// </summary>
        Dictionary<string, string> result = new();

        // Properties
        IEnumerable<PropertyInfo> privateSerializableProperties = typeof(T)
            .GetProperties(anyStatic)
            .Where(HasJsonIncludeAttribute);
        foreach (PropertyInfo property in privateSerializableProperties)
        {
            result[property.Name] = JsonSerializer.Serialize(property.GetValue(null));
        }

        // Fields
        IEnumerable<FieldInfo> privateSerializableFields = typeof(T)
            .GetFields(anyStatic)
            .Where(HasJsonIncludeAttribute);
        foreach (FieldInfo field in privateSerializableFields)
        {
            result[field.Name] = JsonSerializer.Serialize(field.GetValue(null));
        }
        
        return JsonSerializer.Serialize(result);
    }

    public static void DeserializeStaticClass<T>(string json)
    {
        /// <summary>
        /// Attempts to deserialize json input and set static fields in class T, searching for properties with the
        /// JsonInclude attribute.
        /// </summary>
        Dictionary<string, string>? memberNameAndValue = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (memberNameAndValue == null)
        {
            GD.PrintErr("DeserializeStaticClass: Unable to deserialize JSON to a dictionary.");
            return;
        }

        // Properties
        IEnumerable<PropertyInfo> privateSerializableProperties = typeof(T)
            .GetProperties(anyStatic)
            .Where(HasJsonIncludeAttribute);
        foreach (PropertyInfo property in privateSerializableProperties)
        {
            if (memberNameAndValue.ContainsKey(property.Name))
            {
                object? value = JsonSerializer.Deserialize(memberNameAndValue[property.Name], property.PropertyType);
                property.SetValue(null, value);
            }
            else
            {
                GD.PrintErr($"DeserializeStaticClass: property {property.Name} not found in the input JSON.");
            }
        }
        
        // Fields
        IEnumerable<FieldInfo> privateSerializableFields = typeof(T)
            .GetFields(anyStatic)
            .Where(HasJsonIncludeAttribute);
        foreach (FieldInfo field in privateSerializableFields)
        {
            if (memberNameAndValue.ContainsKey(field.Name))
            {
                object? value = JsonSerializer.Deserialize(memberNameAndValue[field.Name], field.FieldType);
                field.SetValue(null, value);
            }
            else
            {
                GD.PrintErr($"DeserializeStaticClass: field {field.Name} not found in the input JSON.");
            }
        }
    }
}
