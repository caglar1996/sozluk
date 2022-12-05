using BlazorSozluk.Common.ViewModels;

namespace BlazorSozluk.Api.Domain.Models;

/// <summary>
/// Entry Verilan + yada - oylar olacak
/// </summary>
public class EntryCommentVote : BaseEntity
{
    public Guid EntyCommentId { get; set; }
    public VoteType VoteType { get; set; }
    public Guid CreatedById { get; set; }
    public virtual EntryComment EntryComment { get; set; }
}
