using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories.Shared;

namespace RFE.Auth.Core.Models.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Properties

        private readonly string _dbConnectionString;
        
        private IDbConnection _dbConnection;
        
        public IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    _dbConnection = new SqlConnection(_dbConnectionString);
                }
                return _dbConnection;
            }            
        }       

        #endregion

        #region Constructor

        public UnitOfWork(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }        

        #endregion

        #region Public methods

        /// <summary>
        /// dispose underlying DB connection
        /// </summary>
        public void Dispose()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }

        #endregion

    }
}