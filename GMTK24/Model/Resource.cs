using System;

namespace GMTK24.Model;

public class Resource
{
    public Resource(string? iconNameWithBacker, string? iconNameNoBacker, string name, bool hasCapacity, int startingAmount = 0)
    {
        IconNameWithBacker = iconNameWithBacker;
        IconNameNoBacker = iconNameNoBacker;
        Name = name;
        HasCapacity = hasCapacity;
        Id = name.GetHashCode();
        Quantity = startingAmount;
        Capacity = startingAmount;
    }

    public string? IconNameWithBacker { get; set; }
    public string? IconNameNoBacker { get; }
    public float Capacity { get; private set; }
    public float Quantity { get; private set; }
    public int Id { get; }
    public string Name { get; }
    public bool HasCapacity { get; }
    public bool IsAtCapacity => Quantity >= Capacity;

    public string Status()
    {
        if (HasCapacity)
        {
            return $"{(int) Quantity} / {Capacity}";
        }

        return $"{(int) Quantity}";
    }

    public string InlineTextIcon(float scale = 1f)
    {
        return $"[resourceTexture({IconNameNoBacker},{scale})]";
    }

    public void Add(float delta)
    {
        if (HasCapacity)
        {
            Quantity = Math.Clamp(Quantity + delta, 0, Capacity);
        }
        else
        {
            Quantity += delta;
            if (Quantity <= 0)
            {
                Quantity = 0;
            }
        }
    }

    public void AddCapacity(float delta)
    {
        Capacity += delta;
    }

    public void Consume(float amount)
    {
        Add(-amount);
    }
}
