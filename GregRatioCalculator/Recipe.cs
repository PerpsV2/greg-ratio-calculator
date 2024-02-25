using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GregRatioCalculator
{
    struct Resource
    {
        public string name { get; set; }
        public int amount { get; set; }

        public Resource(string name, int amount)
        {
            this.name = name;
            this.amount = amount;
        }
    }

    enum VoltageTier
    {
        ULV, LV, MV, HV, EV, IV, LUV, ZPM, UV, UHV,
        
        max_voltage_tier
    }

    internal class Recipe
    {
        public string name { get; set; }
        public VoltageTier voltageTier { get; set; }
        public float time { get; set; }
        public List<Resource> inputs { get; set; }
        public List<Resource> outputs { get; set; }
        public List<Recipe> inputRecipes { get; }

        public Recipe(string name, VoltageTier voltageTier, float time, List<Recipe>? inputRecipes = null)
        {
            this.name = name;
            this.voltageTier = voltageTier;
            this.time = time;
            inputs = new List<Resource>();
            outputs = new List<Resource>();
            this.inputRecipes = inputRecipes ?? new List<Recipe>();
        }

        public void AddInputRecipe(Recipe recipe)
        {
            if (!inputRecipes.Contains(recipe))
                inputRecipes.Add(recipe);
        }

        public float GetOverclockedTime()
        {
            return Math.Max(time / (int)Math.Pow(2, Math.Max((int)voltageTier - 1, 0)), 0.05f);
        }
    }
}
