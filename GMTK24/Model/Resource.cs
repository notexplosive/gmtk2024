namespace GMTK24.Model;

public class Resource
{
    public Resource(string name)
    {
        Id = name.GetHashCode();
        Quantity = 0;
        Capacity = 0;
    }

    public int Capacity { get; set; }
    public int Quantity { get; set; }
    public int Id { get; }

    public string Status()
    {
        return $"{Quantity} / {Capacity}";
    }
}
