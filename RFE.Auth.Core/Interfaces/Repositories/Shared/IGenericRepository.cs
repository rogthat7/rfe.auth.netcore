using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Repositories.Shared
{
    public interface IGenericRepository<T> where T: class
    {
        Task<IEnumerable<T>> All();
        Task<T> GetById(System.Guid id);
        Task<bool> Add(T entity);
        Task<bool> Delete(System.Guid id);
        Task<bool> Upsert(T entity);
    }
}