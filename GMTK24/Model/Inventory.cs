using System.Collections.Generic;
using ExplogineMonoGame;
using GMTK24.Config;

namespace GMTK24.Model;

public class Inventory
{
    private readonly List<Resource> _resources = new();

    public void ApplyDeltas(List<ResourceDelta> deltas, float factor = 1f)
    {
        foreach (var delta in deltas)
        {
            var foundResource = GetResource(delta.ResourceName);

            if (delta.AffectCapacity)
            {
                foundResource.AddCapacity(delta.Amount * factor);
            }
            else
            {
                foundResource.Add(delta.Amount * factor);
            }
        }
    }

    private Resource GetResource(string name)
    {
        var foundResource = _resources.Find(a => a.Name == name);

        if (foundResource == null)
        {
            Client.Debug.LogWarning($"No resource found called {name}");
            return new Resource(name);
        }

        return foundResource;
    }

    public IEnumerable<Resource> AllResources()
    {
        return _resources;
    }

    public void AddResource(Resource resource)
    {
        _resources.Add(resource);
    }

    public void ResourceUpdate(float dt)
    {
        var foodCostOfOnePerson = 10;
        
        var population = GetResource("Population");
        var inspiration = GetResource("Inspiration");
        var food = GetResource("Food");
        
        var populationSeconds = population.Quantity * dt;

        inspiration.Add(populationSeconds);

        if (food.Quantity > foodCostOfOnePerson && !population.IsAtCapacity)
        {
            food.Consume(foodCostOfOnePerson);
            population.Add(1);
        }
    }
}
