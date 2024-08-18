namespace GMTK24.Model;

public class ConvertResourceRule : InventoryRule
{
    private readonly int _amountOfIngredient;
    private readonly int _amountOfResult;
    private readonly string _ingredientResourceName;
    private readonly string _resultResourceName;

    public ConvertResourceRule(string ingredientResourceName, int amountOfIngredient, string resultResourceName,
        int amountOfResult)
    {
        _ingredientResourceName = ingredientResourceName;
        _amountOfIngredient = amountOfIngredient;
        _resultResourceName = resultResourceName;
        _amountOfResult = amountOfResult;
    }

    public override void Run(Inventory inventory, float dt)
    {
        var input = inventory.GetResource(_ingredientResourceName);
        var output = inventory.GetResource(_resultResourceName);

        var scaledInput = dt * _amountOfIngredient;
        var scaledOutput = dt * _amountOfResult;

        if (input.Quantity >= scaledInput && !output.IsAtCapacity)
        {
            input.Consume(scaledInput);
            output.Add(scaledOutput);
        }
    }

    public override string Description(Inventory inventory)
    {
        var input = inventory.GetResource(_ingredientResourceName);
        var output = inventory.GetResource(_resultResourceName);

        return
            $"Convert {_amountOfIngredient}{input.InlineTextIcon()} to {_amountOfResult}{output.InlineTextIcon()} per second.";
    }
}