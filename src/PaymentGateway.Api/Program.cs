using FluentValidation;

using Microsoft.OpenApi.Models;

using PaymentApi.Validators;

using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Middleware;
using PaymentGateway.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PaymentMappingProfile>());

builder.Services.AddOptions(builder.Configuration);

builder.Services.AddTelemetry();
builder.Services.AddHttpClient(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Policy Documents API",
        Version = "v1",
        Description = "API for Policy Documents"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentsTest API v1");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<AuthorizationMiddleware>();

app.UseAuthorization();
app.UseMiddleware<ValidationMiddleware>();
app.MapControllers();
app.Run();
