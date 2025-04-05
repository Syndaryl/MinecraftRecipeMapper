// See https://aka.ms/new-console-template for more information

using Syndaryl.Minecraft.JSON;

// Console.WriteLine("Hello, World!");


// demo of a Recipe object
var recipe = new Recipe
{
    Type = "minecraft:crafting_shaped",
    Category = "misc",
    Key = new Dictionary<string, IRecipeComponent>()
    {
        { "A", RecipeItem.CreateInstance("minecraft:andesite") },
        { "B", RecipeTag.CreateInstance("c:nuggets/iron") }
    },
    Pattern = new()
    {
        "BA",
        "AB"
    },
    Result = new()
    {
        RecipeItem.CreateInstance (
            "minecraft:andesite_alloy",
            1
        )
    }
};

