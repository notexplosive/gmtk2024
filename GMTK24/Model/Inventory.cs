using System.Collections.Generic;
using System.Text;
using ExplogineMonoGame;
using GMTK24.Config;

namespace GMTK24.Model;

public class Inventory
{
    private readonly List<Resource> _resources = new();
    private readonly List<InventoryRule> _rules = new();

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

    public Resource GetResource(string name)
    {
        var foundResource = _resources.Find(a => a.Name == name);

        if (foundResource == null)
        {
            Client.Debug.LogWarning($"No resource found called {name}");
            return new Resource(null, null, name, false);
        }

        return foundResource;
    }

    public IEnumerable<Resource> AllResources()
    {
        return _resources;
    }

    public Resource AddResource(Resource resource)
    {
        _resources.Add(resource);
        return resource;
    }

    public void ResourceUpdate(float dt)
    {
        foreach (var rule in _rules)
        {
            rule.Run(this, dt);
        }
    }

    public bool CanAfford(List<ResourceDelta> costs)
    {
        foreach (var cost in costs)
        {
            var resource = GetResource(cost.ResourceName);

            if (resource.Quantity < cost.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public string DisplayRules()
    {
        var stringBuilder = new StringBuilder();

        foreach (var rule in Rules())
        {
            stringBuilder.AppendLine($"- {rule.Description(this)}");
        }

        return stringBuilder.ToString();
    }

    public void AddRule(InventoryRule convertResourceRule)
    {
        _rules.Add(convertResourceRule);
    }

    public IEnumerable<InventoryRule> Rules()
    {
        return _rules;
    }
}

public abstract class InventoryRule
{
    public abstract void Run(Inventory inventory, float dt);

    public abstract string Description(Inventory inventory);
}
