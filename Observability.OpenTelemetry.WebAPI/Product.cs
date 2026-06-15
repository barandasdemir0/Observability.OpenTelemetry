internal sealed class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    internal record CreateDto(
    string FirstName,
    decimal price);

}

