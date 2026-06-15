using System.Diagnostics;
using static Product;

internal sealed class ProductService(ProductRepository productRepository)
{
    public async Task CreateAsync(CreateDto request)
    {
        Activity.Current!.AddEvent(new ActivityEvent("create service"));
        Product product = new()
        {
            Name = request.FirstName,
            Price = request.price
        };
        await productRepository.CreateAsync(product);
        Console.WriteLine("bla bla");

    }
}

