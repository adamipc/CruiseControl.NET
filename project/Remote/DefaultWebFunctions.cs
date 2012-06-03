namespace ThoughtWorks.CruiseControl.Remote
{
    using System;
    using System.Net;

    /// <summary>
    /// Default implementation of web functions
    /// </summary>
    public class DefaultWebFunctions : IWebFunctions
    {
        /// <summary>
        /// Sets credentials on client if address contains user info.
        /// </summary>
        /// <param name="webClient">The <see cref="WebClient"/> to set credentials on.</param>
        /// <param name="address">The address to check for user info.</param>
        public virtual void SetCredentials(WebClient webClient, Uri address)
        {
            if (address.UserInfo.Length <= 0) return;

            var userInfoValues = address.UserInfo.Split(':');

            webClient.Credentials = new NetworkCredential
                                        {
                                            UserName = userInfoValues[0],
                                            Password = userInfoValues.Length > 1 ? userInfoValues[1] : ""
                                        };
        }
    }
}