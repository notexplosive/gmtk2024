using System;

namespace GMTK24.Model;

public class Resource
{
    public Resource(string name, int startingAmount = 0)
    {
        Name = name;
        Id = name.GetHashCode();
        Quantity = startingAmount;
        Capacity = startingAmount;
    }

    public float Capacity { get; private set; }
    public float Quantity { get; private set; }
    public int Id { get; }
    public string Name { get; }
    public bool IsAtCapacity => Quantity >= Capacity;

    public string Status()
    {
        return $"{(int) Quantity} / {Capacity}";
    }

    public void Add(float delta)
    {
        Quantity = Math.Clamp(Quantity + delta, 0, Capacity);
    }

    public void AddCapacity(float delta)
    {
        Capacity += delta;
    }

    public void Consume(int amount)
    {
        Add(-amount);
    }
}
