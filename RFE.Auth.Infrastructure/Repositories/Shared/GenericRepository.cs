using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Interfaces.Repositories.Shared;

namespace RFE.Auth.Infrastructure.Repositories.Shared
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected DbContext _context;
        protected DbSet<T> _dbSet;
        protected ILogger _logger;
        public GenericRepository(IUnitOfWork unitOfWork, DbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async virtual Task<bool> Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            return true;
        }

        public async virtual Task<IEnumerable<T>> All()
        {
            return await _dbSet.ToListAsync<T>();
        }

        public virtual Task<bool> Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public async virtual Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public Task<bool> Upsert(T entity)
        {
            throw new System.NotImplementedException();
        }
    }
}