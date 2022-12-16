using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Infrastructure.Persistence.Context;
using BlazorSozluk.Infrastructure.Persistence.Repositories;
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
            //var seedData = new SeedData();
            //seedData.SeedAsync(configuration).GetAwaiter().GetResult();
            #endregion

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
