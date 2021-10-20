namespace RFE.Auth.Core.Models.Shared
{
    public class ResponseContainer<T> where T: class
    {
        #region Constants

        private const int ReturnCodeResourceNotFound = 404;
            
        #endregion
        
        #region Properties

        public int? ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public T Response { get; set; }
            
        #endregion               

        #region Public methods

        /// <summary>
        /// Validates Response container for errors
        /// </summary>
        /// <returns></returns>
        public virtual ResponseContainer<T> ValidateResponseContainer(bool exposeErrorMessageToConsumer = true) 
        {                       
            if (ReturnCode is null)
            {
                return this;
            }

            return this;
        }
            
        #endregion
    }
}