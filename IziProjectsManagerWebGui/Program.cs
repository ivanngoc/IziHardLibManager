using System;
using System.IO;
using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using IziProjectsManagerWebGui.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization();
//Microsoft.AspNetCore.Authentication.JwtBearer.dll
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
/*
New JWT saved with ID '5e175e26'.
Name: ngoc
Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im5nb2MiLCJzdWIiOiJuZ29jIiwianRpIjoiNWUxNzVlMjYiLCJhdWQiOlsiaHR0cDovL2xvY2FsaG9zdDoyMzI1MyIsImh0dHBzOi8vbG9jYWxob3N0OjAiLCJodHRwOi8vbG9jYWxob3N0OjUxNjAiXSwibmJmIjoxNzExNDMwMjk3LCJleHAiOjE3MTkzNzkwOTcsImlhdCI6MTcxMTQzMDI5NywiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.kmEIX3TSo9kSyB9f89o6LgP8svR8yLiXi8e3lqPgK_s
*/

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors(option =>
{
    option.AddPolicy("AngualarApp", policyBuilder =>
    {
        policyBuilder.WithOrigins("localhost:8080").AllowAnyMethod().AllowAnyHeader();
    });
});



var app = builder.Build();

app.UseCors("AngualarApp");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}


app.UseStaticFiles();

//app.UseEndpoints(endpoint =>
//{
//    endpoint.MapSwagger();
//});

//app.MapControllerRoute();

app.MapControllers();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();
//app.UseEndpoints();

//var v = nameof(ProjectsController);
//app.MapControllerRoute("Projects", "{controller=Projects}/{action=Index}/{id?}");

app.Run();

