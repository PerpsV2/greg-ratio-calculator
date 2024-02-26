using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GregRatioCalculator
{
    public struct Resource
    {
        public string name { get; set; }
        public int amount { get; set; }

        public Resource(string name, int amount)
        {
            this.name = name;
            this.amount = amount;
        }
    }

    public enum VoltageTier
    {
        ULV, LV, MV, HV, EV, IV, LUV, ZPM, UV, UHV,
        
        max_voltage_tier
    }

    public class Recipe
    {
        public string name { get; set; }
        public VoltageTier machineVoltage { get; set; }
        public VoltageTier recipeVoltage { get; set; }
        public float time { get; set; }
        public List<Resource> inputs { get; set; }
        public List<Resource> outputs { get; set; }
        public List<Recipe> inputRecipes { get; }

        public Recipe()
        {
            name = string.Empty;
            machineVoltage = VoltageTier.max_voltage_tier;
            recipeVoltage = VoltageTier.max_voltage_tier;
            time = -1;
            inputs = new List<Resource>();
            outputs = new List<Resource>();
            inputRecipes = new List<Recipe>();
        }

        public Recipe(string name, VoltageTier machineVoltage, VoltageTier recipeVoltage, float time, List<Recipe>? inputRecipes = null)
        {
            if (machineVoltage < recipeVoltage)
                throw new Exception("Machine voltage is less than recipe voltage");
            this.name = name;
            if (machineVoltage == VoltageTier.ULV) machineVoltage = VoltageTier.LV;
            if (recipeVoltage == VoltageTier.ULV) recipeVoltage = VoltageTier.LV;
            this.machineVoltage = machineVoltage;
            this.recipeVoltage = recipeVoltage;
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
            return Math.Max(time / (int)Math.Pow(2, Math.Max(machineVoltage - recipeVoltage, 0)), 0.05f);
        }
    }
}
