
using DACN_Web_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN_Web_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<CsdlFinal1Context>(options => options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                )
            );
            // Enable CORS for development/testing of the static admin pages
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();

            // Use CORS (development only). If you want to restrict origins, change this policy.
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
