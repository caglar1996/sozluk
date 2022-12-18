using BlazorSozluk.Api.Domain.Models;
using BlazorSozluk.Common.Infastructure;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazorSozluk.Infrastructure.Persistence.Context
{
    internal class SeedData
    {
        private static List<User> GetUsers()
        {
            var result = new Faker<User>("tr")
                .RuleFor(i => i.Id, i => Guid.NewGuid())
                .RuleFor(i => i.CreateDate, i => i.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now))
                .RuleFor(i => i.FirstName, i => i.Person.FirstName)
                .RuleFor(i => i.LastName, i => i.Person.LastName)
                .RuleFor(i => i.EmailAddress, i => i.Internet.Email())
                .RuleFor(i => i.UserName, i => i.Internet.UserName())
                .RuleFor(i => i.Password, i => PasswordEncryptor.Encrpt(i.Internet.Password()))
                .RuleFor(i => i.EmailConfig, i => i.PickRandom(true, false))
                .Generate(500);

            return result;
        }


        public async Task SeedAsync(IConfiguration configuration)
        {
            var dbContextBuilder = new DbContextOptionsBuilder();
            dbContextBuilder.UseSqlServer(configuration["BlazerSozlukDbConnection"]);

            var context = new BlazerSozlukContext(dbContextBuilder.Options);

            if (context.Users.Any())
            {
                //User tablosunda herhangi bir kayıt varsa seed data çalışması
                await Task.CompletedTask;
                return;
            }

            var users = GetUsers();
            var userIds = users.Select(i => i.Id);

            await context.Users.AddRangeAsync(users);

            int counter = 0, entryCount = 150; ;
            // entry idlerini tekrar'dan almak zor olacağı için önceden oluşturdum
            var guids = Enumerable.Range(0, entryCount).Select(i => Guid.NewGuid()).ToList();


            var entries = new Faker<Entry>("tr")
                .RuleFor(i => i.Id, i => guids[counter++])
                .RuleFor(i => i.CreateDate, i => i.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now))
                .RuleFor(i => i.Subject, i => i.Lorem.Sentence(5, 5))
                .RuleFor(i => i.Content, i => i.Lorem.Paragraph(2))
                .RuleFor(i => i.CreatedById, i => i.PickRandom(userIds))
                .Generate(entryCount);

            await context.Entries.AddRangeAsync(entries);

            var comments = new Faker<EntryComment>("tr")
                .RuleFor(i => i.Id, i => Guid.NewGuid())
                .RuleFor(i => i.CreateDate, i => i.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now))
                .RuleFor(i => i.Content, i => i.Lorem.Paragraph(2))
                .RuleFor(i => i.CreatedById, i => i.PickRandom(userIds))
                .RuleFor(i => i.EntryId, i => i.PickRandom(guids))
                .Generate(1000);
            await context.EntryComments.AddRangeAsync(comments);

            await context.SaveChangesAsync();
        }
    }
}
