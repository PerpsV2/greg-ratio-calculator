using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GregRatioCalculator
{
    public class RecipeGraph
    {
        public List<Recipe> recipes { get; }

        public RecipeGraph() { 
            recipes = new List<Recipe>();
        }

        public void AddRecipe(Recipe recipe) => recipes.Add(recipe);
        public bool RemoveRecipe(Recipe recipe)
        {
            for (int i = 0; i < recipes.Count(); ++i)
                recipes[i].inputRecipes.Remove(recipe);
            return recipes.Remove(recipe);
        }

        public void BindInputRecipe(Recipe recipe, Recipe inputRecipe) => recipe.AddInputRecipe(inputRecipe);
        public bool ContainsRecipe(string name) => recipes.Where(x => x.name == name).Count() > 0;
        public Recipe? GetRecipe(string name)
        {
            try {
                return recipes.Single(x => x.name == name);
            } 
            catch {
                return null;
            }
        }

        public List<Recipe> Traverse(Recipe rootRecipe)
        {
            List<Recipe> beenTo = new List<Recipe>();
            Console.WriteLine($"{rootRecipe.name} {rootRecipe.machineVoltage} {rootRecipe.recipeVoltage} {rootRecipe.time}s");
            foreach (var recipe in rootRecipe.inputRecipes) {
                beenTo.Concat(Traverse(recipe));
            }
            return beenTo;
        }

        public List<Recipe> CalculateRatios(Recipe rootRecipe, float amount)
        {
            Console.WriteLine($"{amount}x {rootRecipe.name}");

            // for each input of the current recipe calculate the amount used per second
            Dictionary<string, float> usedPerSecond = new Dictionary<string, float>();
            foreach (var input in rootRecipe.inputs)
                usedPerSecond[input.name] = input.amount / rootRecipe.GetOverclockedTime();
            
            // loop through every input recipe
            foreach (var child in rootRecipe.inputRecipes)
            {
                // calculate the output amount used per second
                Dictionary<string, float> producedPerSecond = new Dictionary<string, float>();
                foreach (var output in child.outputs)
                    producedPerSecond[output.name] = output.amount / child.GetOverclockedTime();

                // calculate the minimum amount of the input recipe to run the current recipe 'amount' times
                float max = 0;
                foreach (var resource in usedPerSecond)
                    if (producedPerSecond.ContainsKey(resource.Key))
                    {
                        float childAmount = usedPerSecond[resource.Key] / producedPerSecond[resource.Key] * amount;
                        if (childAmount > max)
                            max = childAmount;
                    }

                // input recipe and current recipe share no resources
                if (max <= 0) return new List<Recipe>();
                CalculateRatios(child, max);
            }
            return new List<Recipe>();
        }
    }
}
