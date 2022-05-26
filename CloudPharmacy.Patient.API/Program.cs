using CloudPharmacy.Common.ExceptionMiddleware;
using CloudPharmacy.Logging;
using CloudPharmacy.Patient.API.Core.DependencyInjection;
using CloudPharmacy.Patient.API.Infrastructure.Services.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2CConfiguration"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Patient", configurePolicy =>
    {
        configurePolicy.RequireClaim(ClaimConstants.Scope, "patient.access");
    });
});

builder.Services.AddTransient<IIdentityService, IdentityService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAppConfiguration(builder.Configuration);
builder.Services.AddLoggingServices();
builder.Services.AddDataServices();
builder.Services.AddVerifiableCredentialsServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Cloud Pharmacy Patient API",
        Description = "API to manage Cloud Pharmacy Patient's data",
        Contact = new OpenApiContact
        {
            Name = "Daniel Krzyczkowski",
            Email = string.Empty,
            Url = new Uri("https://twitter.com/dkrzyczkowski"),
        },
        License = new OpenApiLicense
        {
            Name = "Use under MIT"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                    }
                });
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
