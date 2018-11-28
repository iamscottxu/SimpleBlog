using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scottxu.Blog.Captcha
{
    public static class CaptchaService
    {
        public static IServiceCollection AddCaptcha<T>(this IServiceCollection collection) where T : class, ICaptcha
        {

            return collection.AddSingleton<ICaptcha, T>();
        }

        public static IServiceCollection AddCaptcha<T, U>(this IServiceCollection collection, IConfiguration configuration) 
            where T : class, ICaptcha 
            where U : class, ICaptchaOptions
        {
            return AddCaptcha<T>(collection.Configure<U>(configuration.GetSection("CaptchaOptions")));
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
