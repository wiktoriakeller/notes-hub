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
using NotesApp.Services.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApp.Services.Middleware;
using Microsoft.AspNetCore.Authorization;
using HashidsNet;
using NotesApp.Services.Email;

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

builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddTransient<INoteRepository, NoteRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

//Validators
builder.Services.AddFluentValidation();
builder.Services.AddTransient<IValidator<CreateUserDto>, CreateUserValidator>();
builder.Services.AddTransient<IValidator<RegisterUserDto>, RegisterUserValidator>();
builder.Services.AddTransient<IValidator<LoginDto>, LoginUserValidator>();
builder.Services.AddTransient<IValidator<CreateNoteDto>, CreateNoteValidator>();
builder.Services.AddTransient<IValidator<UpdateNoteDto>, UpdateNoteValidator>();
builder.Services.AddTransient<IValidator<ForgotPasswordRequestDto>, ForgotPasswordRequestValidator>();
builder.Services.AddTransient<IValidator<ResetPasswordDto>, ResetPasswordValidator>();
builder.Services.AddTransient<IValidator<NoteQuery>, NoteQueryValidator>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

//Add automapper
builder.Services.AddAutoMapper(
    typeof(NoteService).Assembly
);

//Authentication and authorization
builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();

var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = authenticationSettings.Issuer,
        ValidAudience = authenticationSettings.Audience,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.Secret))
    };
});

builder.Services.AddScoped<IAuthorizationHandler, NotesAuthorizationHandler>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddHttpContextAccessor();

//HashId
builder.Services.AddScoped<IHashids>(_ => 
    new Hashids(builder.Configuration.GetSection("HashIdSalt").ToString(), 11));

//Add middleware
builder.Services.AddScoped<ErrorHandlingMiddleware>();

//Add email service and configuration
var emailConfiguration = new EmailSettings();
builder.Configuration.GetSection("EmailConfiguration").Bind(emailConfiguration);
builder.Services.AddSingleton(emailConfiguration);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokensHandler,TokensHandler>();

//Add CORS
var origins = builder.Configuration["AllowedOrigins"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("notes-client", builder =>
        builder.AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins(origins)
    );
});

var app = builder.Build();
app.UseCors("notes-client");
await SeedDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();

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