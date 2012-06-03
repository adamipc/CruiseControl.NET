using System;
using System.Net;

namespace ThoughtWorks.CruiseControl.Remote
{
    /// <summary>
    /// Common web functions
    /// </summary>
    public interface IWebFunctions
    {
        /// <summary>
        /// Sets credentials on client if address contains user info.
        /// </summary>
        /// <param name="webClient">The <see cref="WebClient"/> to set credentials on.</param>
        /// <param name="address">The address to check for user info.</param>
        void SetCredentials(WebClient webClient, Uri address);
    }
}