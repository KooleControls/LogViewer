using NJsonSchema;
using NJsonSchema.Generation;

namespace LogViewer.Config.Schema
{
    public class ColorSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            if (context.Type == typeof(Color))
            {
                context.Schema.Type = JsonObjectType.Integer;
                context.Schema.Format = "color-hex"; // Optional: custom format label
                context.Schema.Example = "#FF0000"; // Example value
            }
        }
    }
}