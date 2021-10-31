using System;
using System.Data;
using System.Threading.Tasks;

namespace RFE.Auth.Core.Interfaces.Shared
{
    public interface IUnitOfWork  : IDisposable
    {
        IDbConnection DbConnection {get;}
    }
}