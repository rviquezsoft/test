using WebSocketsServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSingleton<WebSocketConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var manager = context.RequestServices.GetService<WebSocketConnectionManager>();

        // Genera un identificador único para el cliente
        var clientId = Guid.NewGuid().ToString();
        manager.AddSocket(clientId, webSocket);

        try
        {
            await manager.Echo(context, webSocket, manager);
        }
        finally
        {
            // Elimina la conexión del cliente cuando se desconecta
            manager.RemoveSocket(clientId);
        }
    }
    else
    {
        await next();
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
