using LogViewer.Config.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema.NewtonsoftJson.Generation;
using NJsonSchema.Generation;

namespace LogViewer.Config.Schema
{
    public class SchemaBuilder : ISchemaBuilder
    {
        public string GetSchema()
        {
            var schema = NJsonSchema.JsonSchema.FromType<LogViewerConfig>(CreateSchemaGeneratorSettings());
            return schema.ToJson();
        }

        private JsonSchemaGeneratorSettings CreateSchemaGeneratorSettings()
        {
            return new NewtonsoftJsonSchemaGeneratorSettings
            {
                SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter() }
                },
                SchemaProcessors =
            {
                new ColorSchemaProcessor() ,
                new FlagsEnumSchemaProcessor()
            }
            };
        }
    }
}