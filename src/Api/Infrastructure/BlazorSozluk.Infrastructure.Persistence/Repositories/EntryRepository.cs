using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Api.Domain.Models;
using BlazorSozluk.Infrastructure.Persistence.Context;

namespace BlazorSozluk.Infrastructure.Persistence.Repositories
{
    internal class EntryRepository : GenericRepository<Entry>, IEntryRepository
    {
        public EntryRepository(BlazerSozlukContext dbContext) : base(dbContext)
        {
        }
    }
}
