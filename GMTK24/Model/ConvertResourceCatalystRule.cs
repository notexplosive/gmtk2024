namespace GMTK24.Model;

public class ConvertResourceCatalystRule : InventoryRule
{
    private readonly string _catalystResourceName;
    private readonly int _inputAmount;
    private readonly string _inputResourceName;
    private readonly int _outputAmount;
    private readonly string _outputResourceName;

    public ConvertResourceCatalystRule(string catalystResourceName, int inputAmount, string inputResourceName, int outputAmount, string outputResourceName)
    {
        _catalystResourceName = catalystResourceName;
        _inputAmount = inputAmount;
        _inputResourceName = inputResourceName;
        _outputAmount = outputAmount;
        _outputResourceName = outputResourceName;
    }

    public override void Run(Inventory inventory, float dt)
    {
        var catalyst = inventory.GetResource(_catalystResourceName);
        var output = inventory.GetResource(_outputResourceName);
        var input = inventory.GetResource(_inputResourceName);

        var factor = catalyst.Quantity * dt;
        var scaledInput = factor * _inputAmount;
        var scaledOutput = factor * _outputAmount;

        if (input.Quantity >= scaledInput && !output.IsAtCapacity)
        {
            input.Consume(scaledInput);
            output.Add(scaledOutput);
        }
    }

    public override string Description(Inventory inventory)
    {
        var catalyst = inventory.GetResource(_catalystResourceName);
        var input = inventory.GetResource(_inputResourceName);
        var output = inventory.GetResource(_outputResourceName);

        return $"Convert {_inputAmount}{input.InlineTextIcon()} to {_outputAmount}{output.InlineTextIcon()} per {catalyst.InlineTextIcon()} per second.";
    }
}
