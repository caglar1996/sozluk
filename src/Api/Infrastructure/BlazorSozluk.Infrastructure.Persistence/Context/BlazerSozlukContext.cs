using BlazorSozluk.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BlazorSozluk.Infrastructure.Persistence.Context;
public class BlazerSozlukContext : DbContext
{
    public const string DEFAULT_SCHEMA = "dbo";

    /// <summary>
    /// Migration yaparken bu constructr calısacağı için, bir optionbuilder config bulunmayacak
    /// </summary>
    public BlazerSozlukContext()
    {

    }

    public BlazerSozlukContext(DbContextOptions options) : base(options)
    {

    }
    public DbSet<User> Users { get; set; }

    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryVote> EntryVotes { get; set; }
    public DbSet<EntryFavorite> EntryFavorites { get; set; }

    public DbSet<EntryComment> EntryComments { get; set; }
    public DbSet<EntryCommentVote> EntryCommentVotes { get; set; }
    public DbSet<EntryCommentFavorite> EntryCommentFavorites { get; set; }
    public DbSet<EmailConfirmation> EmailConfirmations { get; set; }

    /// <summary>
    /// Migration için ekledik, optionbuilder config bulunmadığın için burası çalışacak
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connStr = "Data Source=DESKTOP-1O3E3HA;TrustServerCertificate=True;Initial Catalog=blazorsozluk;User ID=sa;Password=sa";
            optionsBuilder.UseSqlServer(connStr, opt =>
            {
                opt.EnableRetryOnFailure();
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    #region SaveChanges Overrides
    public override int SaveChanges()
    {
        OnBeforeSave();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSave();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSave();
        return base.SaveChangesAsync(cancellationToken);
    }
    #endregion

    private void OnBeforeSave()
    {
        // Veri tabanına kayıt eklenirken araya girilecek ve base entity değer atanacak
        var addedEntites = ChangeTracker.Entries()
                           .Where(i => i.State == EntityState.Added)
                           .Select(i => (BaseEntity)i.Entity);

        PrepareAddedEntities(addedEntites);
    }

    private void PrepareAddedEntities(IEnumerable<BaseEntity> entites)
    {
        foreach (var entity in entites)
        {
            if (entity.CreateDate == DateTime.MinValue) // DateTime null olamadığı için en küçük min val. verilebilir
                entity.CreateDate = DateTime.Now;
        }
    }
}
