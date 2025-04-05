using System.ComponentModel.DataAnnotations;

namespace Syndaryl.Minecraft.JSON
{
    public record Recipe
    {

        [Required]
        public string Type { get; set; } = "minecraft:crafting";
        
        [Required]
        public string Category { get; set; } = "misc";
        
        [Required]
        public Dictionary<string, IRecipeComponent> Key { get; set; } = new Dictionary<string, IRecipeComponent>();

        [Required]
        public List<string> Pattern { get; set; } = new List<string>();

        [Required]
        public List<RecipeItem> Result { get; set; } = new List<RecipeItem>();

        public override string ToString()
        {
            string keyString = string.Join(", ", Key.Select(kvp => $"{kvp.Key}: {kvp.Value.Id}"));
            string patternString = string.Join(", ", Pattern);
            string resultString = string.Join(", ", Result.Select(r => r.ToString()));
            return $"Type: {Type}, Category: {Category}, \r\n\tKey: {keyString}, \r\n\tPattern: {patternString}, \r\n\tResult: {resultString}";
        }
    }
}

namespace Syndaryl.Minecraft.JSON
{
    public interface IRecipeComponent
    {
        public string Id { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class RecipeItem : IRecipeComponent
    {
        internal static readonly RecipeItem Empty = RecipeItem.CreateInstance("minecraft:air", 1);

        private RecipeItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            if (!id.Contains(':'))
                throw new ArgumentException("Value must contain a namespace and name.", nameof(id));
            Id = id;
            string[] parts = id.Split(':');
            Namespace = parts[0];
            Name = parts[1];
            Count = 1;
        }

        private RecipeItem(string id, int count) : this(id)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0.");
            {

                Count = count;
            }
        }

        public static RecipeItem CreateInstance(string id)
        {
            return new RecipeItem(id);
        }

        public static RecipeItem CreateInstance(string id, int count)
        {
            return new RecipeItem(id, count);
        }

        public string Id { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }

        public int Count
        {
            get => 1;
            set { }
        }

        public override string ToString()
        {
            return $"RecipeItem(Id: {Id}, Namespace: {Namespace}, Name: {Name}, Count: {Count})";
        }
    }

    public class RecipeTag : IRecipeComponent
    {
        private RecipeTag(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            if (!id.Contains(':'))
                throw new ArgumentException("Value must contain a namespace and name.", nameof(id));
            Id = id;
            string[] parts = id.Split(':');
            Namespace = parts[0];
            Name = parts[1];
            Count = 1;
        }

        public static RecipeTag CreateInstance(string id)
        {
            return new RecipeTag(id);
        }

        public string Id { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }

        public int Count
        {
            get => 1;
            set { }
        }
        
        public override string ToString()
        {
            return $"RecipeTag(Id: {Id}, Namespace: {Namespace}, Name: {Name}, Count: {Count})";
        }
    }
}