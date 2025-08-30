using DemoEcommerce.Application;
using DemoEcommerce.Application.Implementations;
using DemoEcommerce.Application.Interfaces;
using DemoEcommerce.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddSingleton(typeof(InMemoryVectorStores), new InMemoryVectorStores());

builder.Services.AddAutoMapper(options => options.AddProfile<MappingProfile>());
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(builder.Configuration["FrontendUrl"])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.SeedFromJsonAsync(db, Path.Combine(app.Environment.ContentRootPath, "seed.json"));

    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    await Task.Run(productService.EmbedPendingItems).ContinueWith((task) => productService.PopulateVectorStores());
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
