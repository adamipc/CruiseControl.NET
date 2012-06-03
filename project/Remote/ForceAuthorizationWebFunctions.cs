using System;
using System.Net;
using System.Text;

namespace ThoughtWorks.CruiseControl.Remote
{
    /// <summary>
    /// Implementation of web functions that forces authorization for SetCredentials
    /// </summary>
    public class ForceAuthorizationWebFunctions : DefaultWebFunctions
    {
        /// <summary>
        /// Sets credentials on client if address contains user info.
        /// </summary>
        /// <param name="webClient">The <see cref="WebClient"/> to set credentials on.</param>
        /// <param name="address">The address to check for user info.</param>
        public override void SetCredentials(WebClient webClient, Uri address)
        {
            if (address.UserInfo.Length <= 0) return;

            var userInfoValues = address.UserInfo.Split(':');

            webClient.Headers.Add("Authorization",
                                  GenerateAuthorizationFromCredentials(userInfoValues[0],
                                                                       userInfoValues.Length > 1
                                                                           ? userInfoValues[1]
                                                                           : ""));
        }

        /// <summary>
        /// Generates the body of a Basic HTTP Authorization header.
        /// </summary>
        /// <param name="userName"> </param>
        /// <param name="password"> </param>
        /// <returns>The body of a basic HTTP Authorization header.</returns>
        private static string GenerateAuthorizationFromCredentials(string userName, string password)
        {
            string credentialsText = String.Format("{0}:{1}", userName, password);
            byte[] bytes = Encoding.ASCII.GetBytes(credentialsText);
            string base64 = Convert.ToBase64String(bytes);
            return String.Concat("Basic ", base64);
        }
    }
}