namespace GMTK24.Model;

public class ConvertResourceRule : InventoryRule
{
    private readonly string _ingredientResourceName;
    private readonly int _amountOfIngredient;
    private readonly string _resultResourceName;
    private readonly int _amountOfResult;

    public ConvertResourceRule(string ingredientResourceName, int amountOfIngredient, string resultResourceName, int amountOfResult)
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

        if (input.Quantity >= _amountOfIngredient && !output.IsAtCapacity)
        {
            input.Consume(_amountOfIngredient);
            output.Add(_amountOfResult);
        }
    }

    public override string Description(Inventory inventory)
    {
        var input = inventory.GetResource(_ingredientResourceName);
        var output = inventory.GetResource(_resultResourceName);

        return
            $"When you have {_amountOfIngredient}{input.InlineTextIcon()}, it becomes {_amountOfResult}{output.InlineTextIcon()}.";
    }
}
