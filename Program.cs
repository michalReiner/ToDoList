using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הוספת DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
                     new MySqlServerVersion(new Version(8, 0, 32)))); // שימי לב שהגרסה מתאימה לגרסת MySQL שלך

// הגדרת CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת תמיכה ביומנים (Logging)
builder.Services.AddLogging();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// הסרת הודעת טקסט והגדרת ברירת מחדל ל-Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseCors();

// הגדרת Routes עם יומנים
app.MapGet("/items", async (ToDoDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Received request to get all items.");
    var items = await db.Items.ToListAsync();
    logger.LogInformation($"Retrieved {items.Count} items.");
    return items;
});

app.MapPost("/items", async (ToDoDbContext db, Item newItem, ILogger<Program> logger) =>
{
    logger.LogInformation($"Received request to add new item: {newItem.Name}");
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    logger.LogInformation($"Item with ID {newItem.Id} created successfully.");
    return Results.Created($"/items/{newItem.Id}", newItem);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem, ILogger<Program> logger) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        logger.LogWarning($"Item with ID {id} not found for update.");
        return Results.NotFound();
    }

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();
    logger.LogInformation($"Item with ID {id} updated successfully.");
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id, ILogger<Program> logger) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        logger.LogWarning($"Item with ID {id} not found for deletion.");
        return Results.NotFound();
    }

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    logger.LogInformation($"Item with ID {id} deleted successfully.");
    return Results.NoContent();
});

//app.MapGet("/", () => "AuthServer API is Running!");

app.Run();
