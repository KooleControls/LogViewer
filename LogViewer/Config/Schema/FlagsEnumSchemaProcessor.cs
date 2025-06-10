using NJsonSchema;
using NJsonSchema.Generation;

namespace LogViewer.Config.Schema
{
    public class FlagsEnumSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            if (context.Type.IsEnum && context.Type.IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                var enumNames = Enum.GetNames(context.Type);

                context.Schema.Type = JsonObjectType.String;
                context.Schema.Description = (context.Schema.Description ?? "") + " (Comma-separated flags allowed)";
                context.Schema.Example = string.Join(", ", enumNames);
                context.Schema.Enumeration.Clear();

                // Build regex pattern
                var escapedNames = enumNames.Select(System.Text.RegularExpressions.Regex.Escape);
                var pattern = $"^({string.Join("|", escapedNames)})(,\\s*({string.Join("|", escapedNames)}))*$";
                context.Schema.Pattern = pattern;
            }
        }
    }
}