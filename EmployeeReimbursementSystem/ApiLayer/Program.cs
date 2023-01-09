using BusinessLayer;
using RepositoryLayer;

namespace ApiLayer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add CORS
        var angularCORSPolicy = "AllowAngularFE";
        builder.Services.AddCors(options => { // Why does this CORS policy let the cookie go to the frontend?
            options.AddPolicy(name: angularCORSPolicy, 
                policy => { // Add CORS policy so we can consume API in angular frontend
                    policy.WithOrigins("https://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.MaxValue);
                }
            );
        });

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Adding our services
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<ITicketRepository, TicketRepository>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IEmployeeValidationService, EmployeeValidationService>();
        builder.Services.AddScoped<ITicketValidationService, TicketValidationService>();
        builder.Services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
        builder.Services.AddSingleton<IDataLogger, DataLogger>();
        // Adding this for allowing access to cookies in controller method
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors(angularCORSPolicy);

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
