using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Syndaryl.Minecraft.JSON
{
    public class Recipe
    {
        [JsonIgnore]
        public string Name { get; set; }

        [Required, JsonPropertyName("type")]
        public string Type { get; set; } = "minecraft:crafting";
        
        [JsonPropertyName("category")]
        public string Category { get; set; } = "misc";
        
        [JsonPropertyName("key")]
        public Dictionary<string, RecipeComponent> Key { get; set; } = new Dictionary<string, RecipeComponent>();

        [JsonPropertyName("pattern")]
        public List<string> Pattern { get; set; } = new List<string>();

        [Required, JsonPropertyName("result")]
        public RecipeComponent Result { get; set; }

        public override string ToString()
        {
            string keyString = string.Join(", ", Key.Select(kvp => $"{kvp.Key}: {kvp.Value.Id}"));
            string patternString = string.Join(", ", Pattern);
            string resultString = Result.ToString();
            return $"Name: {Name}, Type: {Type}, Category: {Category}, \r\n\tKey: {keyString}, \r\n\tPattern: {patternString}, \r\n\tResult: {resultString}";
        }

        public Recipe(string type, string category, Dictionary<string, RecipeComponent> key, List<string> pattern, RecipeItem result, string name="recipe:empty")
        {
            Type = type;
            Category = category;
            Key = key;
            Pattern = pattern;
            Result = result;
            Name = name;
        }

        public static Recipe Empty => new(type: "minecraft:crafting", category: "misc",
            key: new Dictionary<string, RecipeComponent>(), pattern: new List<string>(), result: RecipeItem.Empty);

        public Recipe()
        {
            Type = Recipe.Empty.Type;
            Category = Recipe.Empty.Category;
            Key = Recipe.Empty.Key;
            Pattern = Recipe.Empty.Pattern;
            Result = Recipe.Empty.Result;
            Name = "recipe:empty";
        }

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(),
                new RecipeComponentConverter()
            },
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
        };

        public static Recipe ReadFromStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            
            Recipe result;
            try
            {
                result = JsonSerializer.Deserialize<Recipe>(stream, options) ?? Recipe.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            if (result == Recipe.Empty)
            {
                throw new InvalidOperationException("Deserialization failed.");
            }

        
            return result;
        }
    }

    public interface IRecipeComponent
    {
        public string Id { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public  class RecipeComponent : IRecipeComponent
    {
        public RecipeComponent()
        {

        }

        [JsonConstructor]
        public RecipeComponent(string id, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            if (!id.Contains(':'))
                throw new ArgumentException("Value must contain a namespace and name.", nameof(id));
            Id = id;
            string[] parts = id.Split(':');
            Namespace = parts[0];
            Name = parts[1];
            Count = count;
        }
        public static readonly RecipeComponent Empty = new RecipeComponent()
        {
            Id = "minecraft:air",
            Namespace = "minecraft",
            Name = "air",
            Count = 1
        };
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;
        public static RecipeComponent CreateInstance(string id, int count=1)
        {
            return new RecipeComponent(id, count);
        }

        public override string ToString()
        {
            return $"RecipeComponent(Id: {Id}, Namespace: {Namespace}, Name: {Name}, Count: {Count})";
        }
    }

    public class RecipeItem : RecipeComponent
    {
        internal new static readonly RecipeItem Empty = RecipeItem.CreateInstance("minecraft:air", 1);
        
        //[JsonPropertyName("item")]
        //public new string Id { get; set; } = string.Empty;
        

        public RecipeItem(string id) : base(id)
        {
        }

        public RecipeItem(string id, int count) : base(id, count)
        {
        }

        public static RecipeItem CreateInstance(string id, int count=1)
        {
            return new RecipeItem(id, count);
        }

        public override string ToString()
        {
            return $"RecipeItem(Id: {Id}, Namespace: {Namespace}, Name: {Name}, Count: {Count})";
        }
    }

    public class RecipeTag : RecipeComponent
    {

        private RecipeTag(string id) : base(id)
        {
        }
        private RecipeTag(string id, int count) : base(id, count)
        {
        }
        //[JsonPropertyName("tag")]
        //public new string Id { get; set; } = string.Empty;

        public static RecipeTag CreateInstance(string id, int count=1)
        {
            var result = new RecipeTag(id)
            {
                Count = count
            };
            return result;
        }

        public new static readonly RecipeItem Empty = RecipeItem.CreateInstance("#minecraft:invalid", 1);

        public override string ToString()
        {
            return $"RecipeTag(Id: {Id}, Namespace: {Namespace}, Name: {Name}, Count: {Count})";
        }
    }

    public class RecipeComponentConverter : JsonConverter<RecipeComponent>
    {
        public override RecipeComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var count = 1;
            if ( root.TryGetProperty("count", out var countElement) )
                count = countElement.GetInt32();
            var id = string.Empty;
            // Check which field is present
            if (root.TryGetProperty("item", out var itemProp))
            { 
                id = itemProp.GetString() ?? "";
                Console.WriteLine($"Item: {id}");
                return RecipeItem.CreateInstance(
                    id,
                    count
                );
            }
            else if (root.TryGetProperty("tag", out var tagProp))
            {
                id = tagProp.GetString() ?? "";
                Console.WriteLine($"Tag: {id}");
                return RecipeTag.CreateInstance(
                    id,
                    count
                );
            }
            else if (root.TryGetProperty("id", out var idProp))
            {
                id = idProp.GetString() ?? "";
                Console.WriteLine($"Id: {id}");
                return RecipeComponent.CreateInstance(
                    id,
                    count
                );
            }

            throw new JsonException("Unknown RecipeComponent type");
        }

        public override void Write(Utf8JsonWriter writer, RecipeComponent value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("count", value.Count);

            switch (value)
            {
                case RecipeItem:
                    writer.WriteString("item", value.Id);
                    break;
                case RecipeTag:
                    writer.WriteString("tag", value.Id);
                    break;
                default:
                    writer.WriteString("id", value.Id);
                    break;
            }

            writer.WriteEndObject();
        }
    }

}