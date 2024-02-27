using GregRatioCalculator;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Reflection;

RecipeGraph recipeGraph = new RecipeGraph();
Recipe? current = null;

void PrintInstructions()
{
    Console.WriteLine("Add a new recipe to current   - add <name> <machine voltage> <recipe voltage> <time (seconds)>");
    Console.WriteLine("Add existing recipe to curr.  - add <name>");
    Console.WriteLine("Edit the current recipe       - edit <name> <machine voltage> <recipe voltage> <time (seconds)>");
    Console.WriteLine("Rename the current recipe     - edit <name>");
    Console.WriteLine("Remove a recipe from current  - remove <name>");
    Console.WriteLine("Delete a recipe by name       - delete <name>");
    Console.WriteLine("Select a recipe by name       - select <name>");
    Console.WriteLine("Select global scope           - select");
    Console.WriteLine("List all recipes from current - list");
    Console.WriteLine("List all recipes in graph     - listall");
    Console.WriteLine("Add input to current recipe   - in <amount> <name> ");
    Console.WriteLine("Add output to current recipe  - out <amount> <name> ");
    Console.WriteLine("Remove input from current     - rin <name>");
    Console.WriteLine("Remove output from current    - rout <name>");
    Console.WriteLine("Print recipe i/o              - io");
    Console.WriteLine("List all resource names       - listres");
    Console.WriteLine("Rename a resource             - editres <old name> <new name>");
    Console.WriteLine("Calculate ratios from current - calc <amount>");
    Console.WriteLine("Calculate overclocked time    - otime");
    Console.WriteLine("Save recipe graph to file     - save <file name>");
    Console.WriteLine("Load recipe graph from file   - load <file name>");
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
    if ((inputData.Length != 5 && inputData.Length != 2) || current == null) return;

    // rename the current recipe
    current.name = inputData[1];
    if (inputData.Length == 2) return;
    // continue if the edit command does more than just rename

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

    current.machineVoltage = machineVoltage;
    current.recipeVoltage = recipeVoltage;
    current.time = recipeTime;
}
void RemoveRecipe(string[] inputData)
{
    if (inputData.Length != 2 || current == null) return;
    Recipe? existingRecipe = recipeGraph.GetRecipe(inputData[1]);
    if (existingRecipe == null) return;
    
    if (!current.inputRecipes.Remove(existingRecipe))
        Console.WriteLine("Unable to remove recipe " + inputData[1]);
}
void DeleteRecipe(string[] inputData)
{
    if (inputData.Length != 2) return;
    Recipe? existingRecipe = recipeGraph.GetRecipe(inputData[1]);
    if (existingRecipe == null) return;

    if (!recipeGraph.RemoveRecipe(existingRecipe))
        Console.WriteLine("Unable to delete recipe " + inputData[1]);
    else if (current == existingRecipe) current = null;
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
void ListAllResources()
{
    Console.WriteLine(" --- LISTING ALL RESOURCES ---");
    List<Resource> resources = recipeGraph.GetAllResourceTypes();
    resources = resources.GroupBy(x => x.name).Select(x => x.First()).OrderBy(x => x.name).ToList();
    foreach(var resource in resources)
        Console.WriteLine(" " + resource.name);
}
void RenameResource(string[] inputData)
{
    if (inputData.Length != 3) return;
    foreach(var recipe in recipeGraph.recipes)
    {
        for (int i = 0; i < recipe.inputs.Count(); ++i)
            if (recipe.inputs[i].name == inputData[1])
                recipe.inputs[i] = new Resource(inputData[2], recipe.inputs[i].amount);

        for (int i = 0; i < recipe.outputs.Count(); ++i)
            if (recipe.outputs[i].name == inputData[1])
                recipe.outputs[i] = new Resource(inputData[2], recipe.outputs[i].amount);
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
    if (!float.TryParse(inputData[1], out float multiplier)) return;
    List<RecipeRatio> ratios = recipeGraph.CalculateRatios(current, multiplier);
    int maxRatioAmountStringLength = ratios
        .Aggregate("", (max, cur) => max.Length > ((int)cur.amount).ToString().Length ? max : ((int)cur.amount).ToString()).Length;
    foreach(var ratio in ratios)
    {
        string ratioAmountStr = ((decimal)ratio.amount).ToString("0.000").PadLeft(maxRatioAmountStringLength + 5, ' ');
        Console.WriteLine($"{ratioAmountStr}x of {ratio.recipe.name}");
    }
}
void SaveGraph(string[] inputData)
{
    // make sure the command has the correct number of parameters
    if (inputData.Length != 2) return;

    // validate file name and location
    string filename = inputData[1] + ".xml";
    if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

    string? directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
    Console.WriteLine("Saving to directory " + directory);
    if (directory == null) return;

    string filePath = Path.Combine(directory, filename);
    Console.WriteLine("Saving to " + filePath);
    
    // check if the file exists...
    if (File.Exists(filePath))
    {
        // if it does prompt user to overwrite it
        Console.Write("File already exists. Overwrite it? [y/n]: ");
        if (Console.ReadLine() == "y")
            File.Create(filePath).Close();
        else return;
    }
    else
    {
        // otherwise create a new file at this location
        Console.WriteLine("Creating file");
        File.Create(filePath).Close();
    }

    // serialize the recipe graph to the newly created file
    XmlSerializer ser = new XmlSerializer(typeof(RecipeGraph));
    TextWriter writer = new StreamWriter(filePath);

    Console.WriteLine("Serializing recipe graph");
    ser.Serialize(writer, recipeGraph);

    writer.Close();
    Console.WriteLine("Successfully saved recipe graph");
}
void LoadGraph(string[] inputData)
{
    // make sure the command has the correct number of parameters
    if (inputData.Length != 2) return;

    // validate file name and location
    string filename = inputData[1] + ".xml";
    if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

    string? directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
    Console.WriteLine("Loading from directory " + directory);
    if (directory == null) return;

    string filePath = Path.Combine(directory, filename);
    // check if the current directory contains the file
    if (Directory.GetFiles(directory).Contains(filePath))
    {
        Console.WriteLine("Loading from " + filePath);
        XmlSerializer ser = new XmlSerializer(typeof(RecipeGraph));
        using (Stream reader = new FileStream(filePath, FileMode.Open))
        {
            // load contents of file into recipeGraph
            RecipeGraph? graph = (RecipeGraph?)ser.Deserialize(reader);
            if (graph == null) Console.WriteLine("Unable to read from file");
            else
            {
                recipeGraph = graph;
                Console.WriteLine("Successfully loaded recipe graph");
            }
        }
    }
    else Console.WriteLine("File could not be found");
}

Console.WriteLine("Type help for instructions");
while (true)
{
    Console.Write($"{current?.name ?? "~"}> ");
    string input = Console.ReadLine() ?? "";
    string[] inputData = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (inputData.Length <= 0) continue;
    switch(inputData[0])
    {
        case "help"   : PrintInstructions();       break;
        case "add"    : AddRecipe(inputData);      break;
        case "edit"   : EditRecipe(inputData);     break;
        case "remove" : RemoveRecipe(inputData);   break;
        case "delete" : DeleteRecipe(inputData);   break;
        case "select" : SelectRecipe(inputData);   break;
        case "list"   : ListRecipes();             break;
        case "listall": ListAllRecipes();          break;
        case "in"     : AddInput(inputData);       break;
        case "out"    : AddOutput(inputData);      break;
        case "rin"    : RemoveInput(inputData);    break;
        case "rout"   : RemoveOutput(inputData);   break;
        case "io"     : PrintRecipeInfo();         break;
        case "listres": ListAllResources();        break;
        case "editres": RenameResource(inputData); break;
        case "calc"   : CalcRatios(inputData);     break;
        case "otime"  : CalcOverclockTime();       break;
        case "save"   : SaveGraph(inputData);      break;
        case "load"   : LoadGraph(inputData);      break;
    }
}