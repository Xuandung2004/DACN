using Microsoft.Extensions.DependencyInjection;

namespace DACN_Web_API.Services;

// Lớp static chứa phương thức mở rộng
public static class ThongKeServiceExtensions
{
    // Phương thức mở rộng AddThongKeServices cho IServiceCollection
    public static IServiceCollection AddThongKeServices(this IServiceCollection services)
    {
        // Đăng ký các Service thống kê tại đây
        services.AddScoped<IThongKeService, ThongKeService>();

        // Bạn có thể đăng ký thêm các Service liên quan khác nếu có
        // services.AddTransient<IAnotherService, AnotherService>();

        return services;
    }
}