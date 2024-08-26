using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Repositories;
using P2PWallet.Services.Validators;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
var corsPolicy = "MyCorsPolicy";

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In=ParameterLocation.Header,
        Name="Authorization",
        Type=SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
    
}
);
//db connection
builder.Services.AddDbContext<P2PWalletDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(option =>
    option.AddPolicy(corsPolicy, policy =>
        policy.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader()
        )
    );


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISecurityQuestionRepository, SecurityQuestionRepository>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IPaystackFundService, PaystackFundService>();
builder.Services.AddScoped<IForeignWalletRepository, ForeignWalletRepository>();
builder.Services.AddScoped<IGLSevice, GLService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<IStatementService, StatementService>();
builder.Services.AddScoped<UserDTOValidator>();
builder.Services.AddScoped<PinDTOValidator>();




builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Secret").Value))
    }
    );
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    try
    {

    var dbContext = scope.ServiceProvider.GetRequiredService<P2PWalletDbContext>();
    dbContext.Database.Migrate();
    SeedQuestionInitialiser.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        throw;
    }
 
}
app.UseHttpsRedirection(); 

app.UseCors(corsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
