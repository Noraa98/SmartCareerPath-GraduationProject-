
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Infrastructure.Persistence;
using SmartCareerPath.Infrastructure.Persistence.Data;

namespace SmartCareerPath.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add Infrastructure services
            builder.Services.AddInfrastructure(builder.Configuration);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();
           

            var app = builder.Build();

            // Auto-migrate database on startup (optional)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
