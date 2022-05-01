using Microsoft.EntityFrameworkCore;
using NotesApp.Domain.Interfaces;
using NotesApp.DataAccess;
using NotesApp.Services.Services;
using NotesApp.Services.Interfaces;
using NotesApp.DataAccess.Repositories;
using NotesApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Dbcontext
builder.Services.AddDbContext<NotesDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(NotesDbContext).Assembly.FullName)));

builder.Services.AddScoped<NotesSeeder>();
builder.Services.AddScoped<INotesService, NotesService>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddScoped<INotesRepository, NotesRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//Add automapper
builder.Services.AddAutoMapper(
    typeof(NotesService).Assembly
);

var app = builder.Build();
await SeedDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<NotesSeeder>();
        await dbInitializer.Seed();
    }
}