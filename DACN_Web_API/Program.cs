using DACN_Web_API.Models;
using DACN_Web_API.Services;
namespace DACN_Web_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // --- THÊM DÒNG DUY NHẤT NÀY VÀO ĐÂY ---
            builder.Services.AddThongKeServices();
            // ------------------------------------

            // Kết nối Vnpay API Service
            builder.Services.AddScoped<Services.Vnpay.IVnPayService, Services.Vnpay.VnPayService>();
            builder.Services.AddScoped<CsdlFinal1Context>(_ => new CsdlFinal1Context());
            builder.Services.AddControllers();
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

            // Use CORS (development only). If you want to restrict origins, change this policy.
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}