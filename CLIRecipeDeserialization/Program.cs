// See https://aka.ms/new-console-template for more information

using Syndaryl.Minecraft.JSON;

// Console.WriteLine("Hello, World!");


// demo of a Recipe object
var recipe = new Recipe(type: "minecraft:crafting_shaped", category: "misc",
    key: new Dictionary<string, RecipeComponent>()
    {
        { "A", RecipeItem.CreateInstance("minecraft:andesite") },
        { "B", RecipeTag.CreateInstance("c:nuggets/iron") }
    }, pattern: new()
    {
        "BA",
        "AB"
    }, result: RecipeItem.CreateInstance(
        "minecraft:andesite_alloy",
        1
    ), name:"recipe:test.nojson");

List<Recipe> cliRecipes = new List<Recipe>(args.Length);
cliRecipes.Add(recipe);

if (args.Length > 0)
{

    // if there are arguments, treat them as file paths
    foreach (var arg in args)
    {
        using (var stream = File.OpenRead(arg))
            try
            {
                var r = Recipe.ReadFromStream(stream);
                r.Name = arg;
                cliRecipes.Add(r);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading recipe from {arg}: {ex.Message}");
            }
    }
}
    
    
Console.WriteLine("\r\n\r\nCLI Recipe Deserialization\r\n");
foreach (var cliRecipe in cliRecipes)
{
    Console.WriteLine(cliRecipe);
    Console.WriteLine();
}
Console.ReadKey();
