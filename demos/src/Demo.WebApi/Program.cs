using Demo.WebApi.Endpoints;
using Demo.WebApi.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var useCookieAuth = builder.Configuration.GetValue("Infrastructure:UseCookieAuth", false);
builder.Services.AddInfrastructure(useCookieAuth);

builder.Services.ConfigureFluentAnnotations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGroup("/auth")
    .WithDescription("Authentication-related endpoints for user registration and login.")
    .AddAuthEndpoints();

app.Run();
