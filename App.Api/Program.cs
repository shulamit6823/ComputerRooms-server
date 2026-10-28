using System.Text;
using System.Text.Json.Serialization;
using App.Api.Binding;
using App.Api.Json;
using App.Api.Middleware;
using App.Core.Interfaces.Repositories;
using App.Core.Interfaces.Services;
using App.Data;
using App.Data.Repositories;
using App.Services;
using App.Services.Security;
using App.Services.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- הגדרות ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("חסר ConnectionStrings:DefaultConnection בהגדרות (appsettings.Development.json).");

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (string.IsNullOrWhiteSpace(jwtSettings?.Key) || jwtSettings.Key.Length < 32)
    throw new InvalidOperationException("חסר Jwt:Key בהגדרות, או שהוא קצר מ־32 תווים.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<InitialManagerSettings>(builder.Configuration.GetSection(InitialManagerSettings.SectionName));

// ---------- DbContext ----------
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// ---------- DI: Repositories ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();

// ---------- DI: Services ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// ---------- Authentication + Authorization ----------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// ---------- CORS ל־React ----------
const string ReactCorsPolicy = "React";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy(ReactCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// ---------- Controllers ----------
builder.Services
    .AddControllers(options =>
        // סוג פנייה ב־query string מתקבל בעברית (?requestType=חומרה). חייב להופיע לפני ה־Binders המובנים.
        options.ModelBinderProviders.Insert(0, new RequestTypeModelBinderProvider()))
    .AddJsonOptions(options =>
    {
        // סוג פנייה נשלח ומתקבל בעברית ("חומרה"). חייב להופיע לפני JsonStringEnumConverter.
        options.JsonSerializerOptions.Converters.Add(new RequestTypeJsonConverter());
        // שאר ה־Enums נשלחים ומתקבלים כטקסט: "Pending" וכו'
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ---------- Swagger עם כפתור Authorize ל־JWT ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Support System API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "הדביקי כאן את ה־Token שהתקבל מ־login"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Seed: מנהל ראשוני ----------
using (var scope = app.Services.CreateScope())
{
    var managerService = scope.ServiceProvider.GetRequiredService<IManagerService>();
    await managerService.EnsureInitialManagerAsync();
}

// ---------- Pipeline ----------
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(ReactCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
