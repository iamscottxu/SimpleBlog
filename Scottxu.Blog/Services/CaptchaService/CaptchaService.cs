using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scottxu.Blog.Services.CaptchaService;

namespace Scottxu.Blog.Services.CaptchaService
{
    public static class CaptchaService
    {
        public static IServiceCollection AddCaptcha<T>(this IServiceCollection collection) where T : class, ICaptcha
        {
            return collection.AddSingleton<ICaptcha, T>();
        }

        public static IServiceCollection AddCaptcha<T, TU>(this IServiceCollection collection,
            IConfiguration configuration)
            where T : class, ICaptcha
            where TU : class, ICaptchaOptions
        {
            return AddCaptcha<T>(collection.Configure<TU>(configuration.GetSection("CaptchaOptions")));
        }

        public static IServiceCollection AddCaptcha(this IServiceCollection collection, IConfiguration configuration)
        {
            switch (configuration["CaptchaType"])
            {
                case "reCAPTCHAv2":
                    return collection.AddCaptcha<GoogleReCaptchaV2, GoogleReCaptchaV2.Options>(configuration);
                case "reCAPTCHAv3":
                    return collection.AddCaptcha<GoogleReCaptchaV3, GoogleReCaptchaV3.Options>(configuration);
                default:
                    return collection.AddCaptcha<DisabledCaptcha>();
            }
        }
    }
}