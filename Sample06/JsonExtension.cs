using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace Sample06;

public static class JsonExtension
{
    private const string LiteralDelimiter = "```";
    private const string JsonPrefix = "json";

    public static T ExtractJson<T>(this ChatMessageContent content)
    {
        if (content == null) return default;

        var text = content.Content;

        // Search for initial literal delimiter: ```
        int startIndex = text.IndexOf(LiteralDelimiter, System.StringComparison.Ordinal);
        if (startIndex < 0)
        {
            // No initial delimiter, return entire expression.
            return default;
        }

        startIndex += LiteralDelimiter.Length;

        // Accommodate "json" prefix, if present.
        if (JsonPrefix.Equals(text.Substring(startIndex, JsonPrefix.Length), System.StringComparison.OrdinalIgnoreCase))
        {
            startIndex += JsonPrefix.Length;
        }

        // Locate final literal delimiter
        int endIndex = text.IndexOf(LiteralDelimiter, startIndex, System.StringComparison.OrdinalIgnoreCase);
        if (endIndex < 0)
        {
            endIndex = text.Length;
        }

        // Extract JSON
        var json = text.Substring(startIndex, endIndex - startIndex);
        return JsonSerializer.Deserialize<T>(json);
    }

    public static JsonNode OptionDescriptionSupport(JsonSchemaExporterContext context, JsonNode schema)
    {
        // Determine if a type or property and extract the relevant attribute provider.
        ICustomAttributeProvider? attributeProvider = context.PropertyInfo is not null
            ? context.PropertyInfo.AttributeProvider
            : context.TypeInfo.Type;

        // Look up any description attributes.
        DescriptionAttribute? descriptionAttr = attributeProvider?
            .GetCustomAttributes(inherit: true)
            .Select(attr => attr as DescriptionAttribute)
            .FirstOrDefault(attr => attr is not null);

        // Apply description attribute to the generated schema.
        if (descriptionAttr != null)
        {
            if (schema is not JsonObject jObj)
            {
                // Handle the case where the schema is a Boolean.
                JsonValueKind valueKind = schema.GetValueKind();
                Debug.Assert(valueKind is JsonValueKind.True or JsonValueKind.False);
                schema = jObj = new JsonObject();
                if (valueKind is JsonValueKind.False)
                {
                    jObj.Add("not", true);
                }
            }

            jObj.Insert(0, "description", descriptionAttr.Description);
        }

        return schema;
    }
}
