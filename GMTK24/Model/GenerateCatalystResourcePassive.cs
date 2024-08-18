namespace GMTK24.Model;

public class GenerateCatalystResourcePassive : InventoryRule
{
    private string _catalystName;
    private string _outputName;
    private int _outputAmount;

    public GenerateCatalystResourcePassive(string catalystName, string outputName, int outputAmount)
    {
        _catalystName = catalystName;
        _outputName = outputName;
        _outputAmount = outputAmount;
    }

    public override void Run(Inventory inventory, float dt)
    {
        var catalyst = inventory.GetResource(_catalystName);
        var output = inventory.GetResource(_outputName);
        
        var factor = catalyst.Quantity * dt;
        var scaledOutput = factor * _outputAmount;

        if (!output.IsAtCapacity)
        {
            output.Add(scaledOutput);
        }
    }

    public override string Description(Inventory inventory)
    {
        var catalyst = inventory.GetResource(_catalystName);
        var output = inventory.GetResource(_outputName);
        
        return
            $"Generate {_outputAmount}{output.InlineTextIcon()} per {catalyst.InlineTextIcon()} per second.";
    }
}
