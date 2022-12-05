using BlazorSozluk.Common.ViewModels;

namespace BlazorSozluk.Api.Domain.Models;

/// <summary>
/// Entry Verilan + yada - oylar olacak
/// </summary>
public class EntryVote : BaseEntity
{
    public Guid EntyId { get; set; }
    public VoteType VoteType { get; set; }
    public Guid CreatedById { get; set; }
    public virtual Entry Entry { get; set; }
}
