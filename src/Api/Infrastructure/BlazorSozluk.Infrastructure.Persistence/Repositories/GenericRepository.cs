using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlazorSozluk.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntiy> : IGenericRepository<TEntiy> where TEntiy : BaseEntity
    {
        private readonly DbContext dbContext;
        protected DbSet<TEntiy> Entity => dbContext.Set<TEntiy>();

        public GenericRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Insert Methods
        public virtual async Task<int> AddAsync(TEntiy entity)
        {
            await Entity.AddAsync(entity);
            return await dbContext.SaveChangesAsync();
        }

        public virtual int Add(TEntiy entity)
        {
            Entity.Add(entity);
            return dbContext.SaveChanges();
        }

        public virtual int Add(IEnumerable<TEntiy> entities)
        {
            Entity.AddRange(entities);
            return dbContext.SaveChanges();
        }

        public virtual async Task<int> AddAsync(IEnumerable<TEntiy> entities)
        {
            await Entity.AddRangeAsync(entities);
            return await dbContext.SaveChangesAsync();
        }
        #endregion

        #region Update Methods
        public virtual async Task<int> UpdateAsync(TEntiy entity)
        {
            Entity.Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;
            return await dbContext.SaveChangesAsync();
        }

        public virtual int Update(TEntiy entity)
        {
            Entity.Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;
            return dbContext.SaveChanges();
        }
        #endregion

        #region Delete Methods
        public virtual async Task<int> DeleteAsync(TEntiy entity)
        {
            //Get Edilen bir entity silinmişsse daha önceden ve tekrar silinmek isteniyorsa
            // Tekrar Attact edilerek remove methoduna gönderilir
            if (dbContext.Entry(entity).State == EntityState.Deleted)
                Entity.Attach(entity);

            Entity.Remove(entity);
            return await dbContext.SaveChangesAsync();
        }

        public virtual int Delete(TEntiy entity)
        {
            if (dbContext.Entry(entity).State == EntityState.Detached)
                Entity.Attach(entity);

            Entity.Remove(entity);
            return dbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(Guid id)
        {
            var entity = Entity.Find(id);
            return DeleteAsync(entity);
        }

        public virtual int Delete(Guid id)
        {
            var entity = Entity.Find(id);
            return Delete(entity);
        }

        public virtual bool DeleteRange(Expression<Func<TEntiy, bool>> predicate)
        {
            // Dışardan gelen lamda Expression ile gelen kayıtların toplu şekilde silinmesi
            dbContext.RemoveRange(Entity.Where(predicate));
            return dbContext.SaveChanges() > 0;
        }

        public virtual async Task<bool> DeleteRangeAsync(Expression<Func<TEntiy, bool>> predicate)
        {
            dbContext.RemoveRange(Entity.Where(predicate));
            return await dbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region AddOrUpdate Methods
        public virtual async Task<int> AddOrUpdateAsync(TEntiy entity)
        {
            if (!Entity.Local.Any(i => EqualityComparer<Guid>.Default.Equals(i.Id, entity.Id)))
                dbContext.Update(entity);

            return await dbContext.SaveChangesAsync();
        }

        public virtual int AddOrUpdate(TEntiy entity)
        {
            // Gönderilen entity'nin db'den çekilip local'e alınıp alınmadığı
            // kontrol ediliyor, eğer entity localde varsa çekmeye gerek yok.
            // performans arttırma
            if (!Entity.Local.Any(i => EqualityComparer<Guid>.Default.Equals(i.Id, entity.Id)))
                dbContext.Update(entity);

            return dbContext.SaveChanges();
        }
        #endregion

        /* NOTE
         * Methodların 'virtual' olmasının sebebi, miras alacağımız
         * repositoryler içerisinde bunları özelleştirebiliriz.
         */

        #region Get Methods
        public virtual IQueryable<TEntiy> AsQueryable() => Entity.AsQueryable();

        public virtual async Task<List<TEntiy>> GetAll(bool noTracking = true)
        {
            if (noTracking)
                return await Entity.AsNoTracking().ToListAsync();

            return await Entity.ToListAsync();
        }

        public virtual async Task<List<TEntiy>> GetList(Expression<Func<TEntiy, bool>> predicate, bool noTracking = true, Func<IQueryable<TEntiy>, IOrderedQueryable<TEntiy>> orderBy = null, params Expression<Func<TEntiy, object>>[] includes)
        {
            IQueryable<TEntiy> query = Entity;

            if (predicate != null)
                query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            if (orderBy != null)
                query = orderBy(query);

            if (noTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public virtual async Task<TEntiy> GetByIdAsync(Guid id, bool noTracking = true, params Expression<Func<TEntiy, object>>[] includes)
        {
            TEntiy found = await Entity.FindAsync(id);

            if (found == null)
                return null;

            if (noTracking)
                dbContext.Entry(found).State = EntityState.Detached;

            foreach (Expression<Func<TEntiy, object>> include in includes)
            {
                dbContext.Entry(found).Reference(include).Load();
            }

            return found;
        }

        public virtual async Task<TEntiy> GetSingleAsync(Expression<Func<TEntiy, bool>> predicate, bool noTracking = true, params Expression<Func<TEntiy, object>>[] includes)
        {
            IQueryable<TEntiy> query = Entity;

            if (predicate != null)
                query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            if (noTracking)
                query = query.AsNoTracking();

            return await query.SingleOrDefaultAsync();
        }

        public virtual Task<TEntiy> FirstOrDefaultAsync(Expression<Func<TEntiy, bool>> predicate, bool noTracking = true, params Expression<Func<TEntiy, object>>[] includes)
        {
            return Get(predicate, noTracking, includes).FirstOrDefaultAsync();
        }

        public virtual IQueryable<TEntiy> Get(Expression<Func<TEntiy, bool>> predicate, bool noTracking = true, params Expression<Func<TEntiy, object>>[] includes)
        {
            var query = Entity.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            if (noTracking)
                query = query.AsNoTracking();

            return query;
        }
        #endregion

        #region Bulk Methods
        public virtual async Task BulkDeleteById(IEnumerable<Guid> ids)
        {
            if (ids != null && !ids.Any())
                await Task.CompletedTask;

            dbContext.RemoveRange(Entity.Where(i => ids.Contains(i.Id)));
            await dbContext.SaveChangesAsync();
        }

        public virtual async Task BulkDelte(Expression<Func<TEntiy, bool>> predicate)
        {
            dbContext.RemoveRange(Entity.Where(predicate));
            await dbContext.SaveChangesAsync();
        }

        public virtual async Task BulkDelete(IEnumerable<TEntiy> entities)
        {
            if (entities != null && !entities.Any())
                await Task.CompletedTask;

            Entity.RemoveRange(entities);
            await dbContext.SaveChangesAsync();
        }

        public virtual async Task BulkUpdate(IEnumerable<TEntiy> entities)
        {
            if (entities != null && !entities.Any())
                await Task.CompletedTask;

            Entity.UpdateRange(entities);
            await dbContext.SaveChangesAsync();
        }

        public virtual async Task BulkAdd(IEnumerable<TEntiy> entities)
        {
            if (entities != null && !entities.Any())
                await Task.CompletedTask;

            Entity.AddRange(entities);
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region SaveChanges Methos
        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }
        public int SaveChanges()
        {
            return dbContext.SaveChanges();
        }
        #endregion

        private static IQueryable<TEntiy> ApplyIncludes(IQueryable<TEntiy> query, params Expression<Func<TEntiy, object>>[] includes)
        {
            if (includes != null)
                foreach (var includesItem in includes)
                {
                    query = query.Include(includesItem);
                }

            return query;
        }
    }
}
