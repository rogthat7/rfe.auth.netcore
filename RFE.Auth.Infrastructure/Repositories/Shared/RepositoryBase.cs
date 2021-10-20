using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.Shared;

namespace RFE.Auth.Infrastructure.Repositories.Shared
{
    public class RepositoryBase
    {
        private IUnitOfWork _unitOfWork;

        public RepositoryBase(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        protected virtual async Task<ResponseContainer<IEnumerable<T>>> ExecuteStoredProcedureListResult<T>(string storedProcedureName, DynamicParameters parameters) where T: class
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentNullException(nameof(storedProcedureName));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var commandDefinition = new CommandDefinition(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            
            return await ExecuteStoredProcedureListResult<T>(commandDefinition);
        }
        protected virtual async Task<ResponseContainer<IEnumerable<T>>> ExecuteStoredProcedureListResult<T>(CommandDefinition commandDefinition) where T: class
        {
            if (commandDefinition.Equals(default(CommandDefinition)))            
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }
            
            var response = await _unitOfWork.DbConnection.QueryAsync<T>(commandDefinition);
            
            return CreateResponseContainer<IEnumerable<T>>(response, commandDefinition.Parameters as DynamicParameters);            
        }
        /// <summary>
        /// Create a response container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected virtual ResponseContainer<T> CreateResponseContainer<T>(T response, DynamicParameters parameters) where T: class
        {
            return new ResponseContainer<T>
            {
                Response = response
            };
        }
    }
}