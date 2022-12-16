using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Api.Domain.Models;
using BlazorSozluk.Infrastructure.Persistence.Context;

namespace BlazorSozluk.Infrastructure.Persistence.Repositories
{
    internal class EntryCommentRepository : GenericRepository<EntryComment>, IEntryCommentRepository
    {
        public EntryCommentRepository(BlazerSozlukContext dbContext) : base(dbContext)
        {
        }
    }
}
