using System.Reflection;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Godot;

#nullable enable

public static class Serialization
{
    public static void DeserializeStaticClass<T>(string json)
    {
        /// <summary>
        /// Attempts to deserialize json input and set static fields in class T.
        /// </summary>
        Dictionary<string, object?>? fieldNameAndValue = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
        if (fieldNameAndValue == null)
        {
            GD.PrintErr("DeserializeStaticClass: Unable to deserialize JSON to a dictionary.");
            return;
        }

        //PropertyInfo[] publicProperties = typeof(UI).GetProperties(BindingFlags.Public | BindingFlags.Static);
        IEnumerable<PropertyInfo> privateSerializableProperties = typeof(UI)
            .GetProperties(BindingFlags.Static)
            .Where(
                (PropertyInfo property) =>
                    property.GetCustomAttributes(typeof(JsonSerializableAttribute), false).Any()
            );
        //IEnumerable<PropertyInfo> allSerializableProperties = publicProperties.Concat(privateSerializableProperties);
        foreach (PropertyInfo property in privateSerializableProperties)
        {
            if (fieldNameAndValue.ContainsKey(property.Name))
            {
                // Null in the first argument symbolizes that the value is being set on the static class.
                property.SetValue(null, fieldNameAndValue[property.Name]);
            }
            else
            {
                GD.PrintErr($"DeserializeStaticClass: property {property.Name} not found in the input JSON.");
            }
        }
        
        //FieldInfo[] publicFields = typeof(UI).GetFields(BindingFlags.Public | BindingFlags.Static);
        IEnumerable<FieldInfo> privateSerializableFields = typeof(UI)
            .GetFields(BindingFlags.Static)
            .Where(
                (FieldInfo field) =>
                    field.GetCustomAttributes(typeof(JsonSerializableAttribute), false).Any()
            );
        //IEnumerable<FieldInfo> allSerializableFields = publicFields.Concat(privateSerializableFields);
        foreach (FieldInfo field in privateSerializableFields)
        {
            if (fieldNameAndValue.ContainsKey(field.Name))
            {
                field.SetValue(null, fieldNameAndValue[field.Name]);
            }
            else
            {
                GD.PrintErr($"DeserializeStaticClass: field {field.Name} not found in the input JSON.");
            }
        }
    }
}
