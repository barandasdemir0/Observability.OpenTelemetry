using System.Diagnostics;
using static Product;

internal sealed class ProductRepository(ApplicationDbContext context)
{
    public async Task CreateAsync(Product request)
    {
        Activity.Current!.AddEvent(new ActivityEvent("create repository"));
        context.Add(request);
        await context.SaveChangesAsync();
    }
}

