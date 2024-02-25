using GregRatioCalculator;
using System.Runtime.CompilerServices;

RecipeGraph recipeGraph = new RecipeGraph();
Recipe? current = null;

void PrintInstructions()
{
    Console.WriteLine("Add a new recipe to current   - add <name> <machine voltage> <recipe voltage> <time (seconds)>");
    Console.WriteLine("Add existing recipe to curr.  - add <name>");
    Console.WriteLine("Edit an existing recipe       - edit <name> <machine voltage> <recipe voltage> <time (seconds)>");
    Console.WriteLine("Remove a recipe by name       - remove <name>");
    Console.WriteLine("Select a recipe by name       - select <name>");
    Console.WriteLine("Select global scope           - select");
    Console.WriteLine("List all recipes from current - list");
    Console.WriteLine("List all recipes in graph     - listall");
    Console.WriteLine("Add input to current recipe   - i <amount> <name> ");
    Console.WriteLine("Add output to current recipe  - o <amount> <name> ");
    Console.WriteLine("Remove input from current     - ri <name>");
    Console.WriteLine("Remove output from current    - ro <name>");
    Console.WriteLine("Print recipe i/o              - io");
    Console.WriteLine("Calculate ratios from current - calc <amount>");
    Console.WriteLine("Calculate overclocked time    - otime");
}

void AddRecipe(string[] inputData) 
{
    if (inputData.Length != 5 && inputData.Length != 2) return; // make sure the command has the correct number of arguments

    string name = inputData[1];

    // add a recipe to current
    if (recipeGraph.ContainsRecipe(name))
    {
        if (current != null && inputData.Length == 2) 
            recipeGraph.BindInputRecipe(current, recipeGraph.GetRecipe(name) ?? current);
    }
    // add a recipe to global space
    else if (inputData.Length == 5)
    {
        // make sure voltage is valid
        if (!VoltageTier.TryParse(inputData[2].ToUpper(), out VoltageTier machineVoltage)) return;
        if (!VoltageTier.TryParse(inputData[3].ToUpper(), out VoltageTier recipeVoltage)) return;

        if (machineVoltage < recipeVoltage)
        {
            Console.WriteLine("Recipe has insufficient voltage");
            return;
        }

        // make sure recipe time is valid and a positive number
        if (!float.TryParse(inputData[4], out float recipeTime)) return;
        if (recipeTime < 0) return;

        Recipe recipe = new Recipe(inputData[1], machineVoltage, recipeVoltage, recipeTime);
        recipeGraph.AddRecipe(recipe);
        if (current != null) recipeGraph.BindInputRecipe(current, recipe);
    }
}
void EditRecipe(string[] inputData)
{
    if (inputData.Length != 5) return;

    // make sure name exists
    Recipe? existingRecipe = recipeGraph.GetRecipe(inputData[1]);
    if (existingRecipe == null) return;

    // make sure voltage is valid
    if (!VoltageTier.TryParse(inputData[2].ToUpper(), out VoltageTier machineVoltage)) return;
    if (!VoltageTier.TryParse(inputData[3].ToUpper(), out VoltageTier recipeVoltage)) return;

    if (machineVoltage < recipeVoltage)
    {
        Console.WriteLine("Recipe has insufficient voltage");
        return;
    }

    // make sure recipe time is valid and a positive number
    if (!float.TryParse(inputData[4], out float recipeTime)) return;
    if (recipeTime < 0) return;

    existingRecipe.machineVoltage = machineVoltage;
    existingRecipe.recipeVoltage = recipeVoltage;
    existingRecipe.time = recipeTime;
}
void RemoveRecipe(string[] inputData)
{
    if (inputData.Length != 2) return;
    Recipe? existingRecipe = recipeGraph.GetRecipe(inputData[1]);
    if (existingRecipe != null)
    {
        if (!recipeGraph.RemoveRecipe(existingRecipe))
            Console.WriteLine("Unable to remove recipe " + inputData[1]);
        else if (current == existingRecipe) current = null;
    }
}
void SelectRecipe(string[] inputData)
{
    if (inputData.Length == 1) current = null; // set the current to null if command doesnt have any arguments
    if (inputData.Length == 2) current = recipeGraph.GetRecipe(inputData[1]);
}
void ListRecipes()
{
    // if current is global, list all recipes and return
    if (current == null)
    {
        ListAllRecipes();
        return;
    }

    Console.WriteLine($" --- LISTING RECIPES FROM {current.name} --- ");
    // traverse graph from the current point
    List<Recipe> recipes = recipeGraph.Traverse(current);
    foreach (var recipe in recipes)
        Console.WriteLine($"{recipe.name}: {recipe.machineVoltage} {recipe.recipeVoltage} {recipe.time}s");
}
void ListAllRecipes()
{
    Console.WriteLine($" --- LISTING ALL RECIPES --- ");
    foreach (var recipe in recipeGraph.recipes)
        Console.WriteLine($"{recipe.name}: {recipe.machineVoltage} {recipe.recipeVoltage} {recipe.time}s");
}
void AddInput(string[] inputData)
{
    if (inputData.Length != 3 || current == null) return;
    if (!int.TryParse(inputData[1], out int amount)) return;
    current.inputs.Add(new Resource(inputData[2], amount));
}
void RemoveInput(string[] inputData)
{
    if (inputData.Length != 2 || current == null) return;
    current.inputs = current.inputs.Where(x => x.name != inputData[1]).ToList();
}
void AddOutput(string[] inputData)
{
    if (inputData.Length != 3 || current == null) return;
    if (!int.TryParse(inputData[1], out int amount)) return;
    current.outputs.Add(new Resource(inputData[2], amount));
}
void RemoveOutput(string[] inputData)
{
    if (inputData.Length != 2 || current == null) return;
    current.outputs = current.outputs.Where(x => x.name != inputData[1]).ToList();
}
void PrintRecipeInfo()
{
    if (current == null) return;
    // get max string length of input resource
    int maxInputNameLength = Math.Max(current.inputs
        .Aggregate("", (max, cur) => max.Length > cur.name.Length + cur.amount.ToString().Length + 1 
        ? max : cur.name + " " + cur.amount.ToString()).Length, 6);
    // get max string length of output resource
    int maxOutputNameLength = Math.Max(current.outputs
        .Aggregate("", (max, cur) => max.Length > cur.name.Length + cur.amount.ToString().Length + 1 
        ? max : cur.name + " " + cur.amount.ToString()).Length, 6);
    // print table header
    Console.WriteLine("INPUT ".PadRight(maxInputNameLength, ' ') + "|" + "OUTPUT".PadRight(maxOutputNameLength, ' '));
    Console.WriteLine(new string('-', maxInputNameLength) + "+" + new string('-', maxOutputNameLength));
    for (int i = 0; i < Math.Max(current.inputs.Count(), current.outputs.Count()); ++i)
    {
        // print current input/output resource and amount with padding to align with header
        Console.WriteLine((i < current.inputs.Count() ? current.inputs[i].name + " " + current.inputs[i].amount.ToString() : "").PadRight(maxInputNameLength, ' ') + "|" +
            (i < current.outputs.Count() ? current.outputs[i].name + " " + current.outputs[i].amount.ToString() : "").PadRight(maxOutputNameLength, ' '));
    }
}
void CalcOverclockTime()
{
    if (current == null) return;
    Console.WriteLine(current.GetOverclockedTime());
}
void CalcRatios(string[] inputData)
{
    if (inputData.Length != 2 || current == null) return;
    if (!int.TryParse(inputData[1], out int multiplier)) return;
    recipeGraph.CalculateRatios(current, multiplier);
}

Console.WriteLine("Type help for instructions");
while (true)
{
    Console.Write($"{current?.name ?? "~"}> ");
    string input = Console.ReadLine() ?? "";
    string[] inputData = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    switch(inputData[0])
    {
        case "help"   : PrintInstructions();     break;
        case "add"    : AddRecipe(inputData);    break;
        case "edit"   : EditRecipe(inputData);   break;
        case "remove" : RemoveRecipe(inputData); break;
        case "select" : SelectRecipe(inputData); break;
        case "list"   : ListRecipes();           break;
        case "listall": ListAllRecipes();        break;
        case "i"      : AddInput(inputData);     break;
        case "o"      : AddOutput(inputData);    break;
        case "ri"     : RemoveInput(inputData);  break;
        case "ro"     : RemoveOutput(inputData); break;
        case "info"   : PrintRecipeInfo();       break;
        case "calc"   : CalcRatios(inputData);   break;
        case "otime"  : CalcOverclockTime();     break;
    }
}