using BlazorSozluk.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorSozluk.Infrastructure.Persistence.Extensions
{
    public static class Registration
    {
        public static IServiceCollection AddInfrastructureResgistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BlazerSozlukContext>(conf =>
            {
                string connStr = configuration["BlazerSozlukDbConnection"];
                conf.UseSqlServer(connStr, opt =>
                {
                    opt.EnableRetryOnFailure(); // DB'ye bağlanırken, bir hata alırken retrie mekanızması için yazıldı
                });
            });

            #region Seed Data
            /*
               var seedData = new SeedData();
               seedData.SeedAsync(configuration).GetAwaiter().GetResult();
           */
            #endregion

            return services;
        }
    }
}
