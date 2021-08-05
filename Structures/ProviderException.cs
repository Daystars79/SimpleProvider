using System;


namespace SimpleProvider
{/// <summary>
/// Custom Exceptions from the Provider
/// </summary>
    public class ProviderException : Exception
    {
        /// <summary>
        /// Create an new exception
        /// </summary>
        /// <param name="message"></param>
        public ProviderException(string message, Exception ex) : base(message)
        {
        }
    }
}
