using Microsoft.EntityFrameworkCore;
using NotesApp.Domain.Interfaces;
using NotesApp.DataAccess;
using NotesApp.Services.Services;
using NotesApp.Services.Interfaces;
using NotesApp.DataAccess.Repositories;
using NotesApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using NotesApp.Services.Dto;
using NotesApp.Services.Dto.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using NotesApp.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddFluentValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Dbcontext
builder.Services.AddDbContext<NotesDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(NotesDbContext).Assembly.FullName)),
    ServiceLifetime.Transient,
    ServiceLifetime.Transient);

builder.Services.AddTransient<NotesSeeder>();
builder.Services.AddTransient<INoteService, NoteService>();
builder.Services.AddTransient<IUserService, UserService>();

builder.Services.AddTransient<INoteRepository, NoteRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();

//Validators
builder.Services.AddTransient<IValidator<CreateUserDto>, CreateUserValidator>();
builder.Services.AddTransient<IValidator<LoginDto>, LoginUserValidator>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

//Add automapper
builder.Services.AddAutoMapper(
    typeof(NoteService).Assembly
);

//Authentication
var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = authenticationSettings.JwtIssuer,
        ValidAudience = authenticationSettings.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
    };
});

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