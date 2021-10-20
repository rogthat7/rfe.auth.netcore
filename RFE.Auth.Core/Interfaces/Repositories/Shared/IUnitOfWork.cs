using System;
using System.Data;
using System.Threading.Tasks;

namespace RFE.Auth.Core.Interfaces.Repositories.Shared
{
    public interface IUnitOfWork  : IDisposable
    {
        IDbConnection DbConnection {get;}
    }
}