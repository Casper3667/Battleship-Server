using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net;

namespace GameServer
{
    public class Program
    {
        public static GameServer Server { get; private set; }
        public static void Main(string[] args)
        {
            Console.WriteLine("Booting up");
            Server = new GameServer(IPAddress.Any, 13000, 2, false);
            Server.Start();
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks()
                .AddCheck<ReadinessHealthCheck>("readiness")
                .AddCheck<LivenessHealthCheck>("liveness");

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Health check endpoints
            app.UseHealthChecks("/health/readiness", new HealthCheckOptions
            {
                Predicate = check => check.Name == "readiness",
            });

            app.UseHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = check => check.Name == "liveness",
            });

            //app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            Console.WriteLine("Bootup complete.");
            app.Run();
        }
    }
}
