using Microsoft.EntityFrameworkCore;
using Observability.OpenTelemetry.WebAPI;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.Diagnostics;
using static Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer("Server=BARANPC\\SQLEXPRESS;Database=OpenTelemetryDb;integrated security=True; TrustServerCertificate=True;");
});

builder.Services
    .AddOpenTelemetry()//api tarafından yapılan istekleri tanımlamayı sağlıyor
    .WithTracing(configure =>
    {
        configure
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebAPI"))//basit tanımlamalar configuresource olur ama o detaylı
        .AddAspNetCoreInstrumentation()//api tarafında yapılan istekleri  trace etmeyi sağlıyor
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.Filter = (provider, command) =>
            {
                return true;
            };

            options.EnrichWithIDbCommand = (activity, command) =>
            {
                activity.SetTag("db.statement", command.CommandText);
            };
        })
        .AddConsoleExporter()//burada yaptığın herşeyi console exportla
        .AddOtlpExporter();
    });

builder.Services.AddTransient<ReqAndResActivityBodyMiddleware>();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<ProductRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {

        Activity.Current!.SetStatus(ActivityStatusCode.Error);
        Activity.Current!.AddEvent(new ActivityEvent("error"));//hatanın tam konumu
        Activity.Current!.AddTag("error.message", ex.Message);
        Activity.Current!.AddTag("error.stack.trace", ex.StackTrace);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(ex.Message);
    }
});

//app.UseMiddleware<ReqAndResActivityBodyMiddleware>();
app.UseWhen(
    context =>
        context.Request.Path.StartsWithSegments("/exception-api") ||
        context.Request.Path.StartsWithSegments("/hello-world") ||
        context.Request.Path.StartsWithSegments("/create-product") ||
        context.Request.Path.StartsWithSegments("/get-todo"),
    branch =>
    {
        branch.UseMiddleware<ReqAndResActivityBodyMiddleware>();
    });

app.MapGet("/exception-api", () =>
{
    //Activity.Current //şuanki takip edilen activitye ulaşır bu hali ile

    Activity.Current!.AddTag("user.id", "1");
    throw new ArgumentException("my exception");
    return "hello world! 1 ";
});
app.MapGet("/hello-world", () => "Hello World! 2");




app.MapPost("/create-product", async (CreateDto request,ProductService productService) =>
{
    await productService.CreateAsync(request);
    return Results.Ok(new
    {
        message = "create is successfully"
    });
});

app.MapGet("/get-todo", async (HttpClient httpclient) =>
{
    var message = await httpclient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
    if (message.IsSuccessStatusCode)
    {
        var content = await message.Content.ReadFromJsonAsync<object>();
        return Results.Ok(content);
    }

    return Results.BadRequest("something went wrong");

});

app.Run();


