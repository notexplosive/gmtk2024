namespace GMTK24.Model;

public class GenerateResourcePerSecondRule : InventoryRule
{
    private string _ingredientResourceName;
    private string _resultResourceName;
    private float _amountPerSecond;

    public GenerateResourcePerSecondRule(string ingredientResourceName, string resultResourceName, float amountPerSecond = 1f)
    {
        _ingredientResourceName = ingredientResourceName;
        _resultResourceName = resultResourceName;
        _amountPerSecond = amountPerSecond;
    }

    public override void Run(Inventory inventory, float dt)
    {
        var input = inventory.GetResource(_ingredientResourceName);
        var output = inventory.GetResource(_resultResourceName);
        
        output.Add(input.Quantity * dt * _amountPerSecond);
    }

    public override string Description(Inventory inventory)
    {
        var input = inventory.GetResource(_ingredientResourceName);
        var output = inventory.GetResource(_resultResourceName);

        return $"Every {input.InlineTextIcon()} generates {_amountPerSecond:N0}{output.InlineTextIcon()} per second.";
    }
}
