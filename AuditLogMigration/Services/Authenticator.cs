using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel.Description;
using AuditLogMigration.DataModel;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace AuditLogMigration
{
    public class Authenticator
    {
        #region Class Level Members
        // To get discovery service address and organization unique name, 
        // Sign in to your CRM org and click Settings, Customization, Developer Resources.
        // On Developer Resource page, find the discovery service address under Service Endpoints and organization unique name under Your Organization Information.
        private string _discoveryServiceAddress = string.Empty;
        private string _organizationUniqueName = string.Empty;
        private string clientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
        private string redirectUrl = "app://58145B91-0C36-4500-8554-080854F2AC97";

        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _domain = string.Empty;
        private static Version _ADALAsmVersion;

        #endregion Class Level Members

        public Authenticator(string userName, string password, string orgName)
        {
            this._userName = userName;
            this._password = password;
            this._organizationUniqueName = orgName;

            this._discoveryServiceAddress = ConfigurationManager.AppSettings.Get("DiscoveryServiceAddress");
            this._domain = ConfigurationManager.AppSettings.Get("Domain");
        }

        public IOrganizationService Run()
        {
            string organizationUri = string.Empty;

            organizationUri = FindOrganization(_organizationUniqueName,
                        GetInstances()).ApiUrl;

            OrganizationServiceProxy organizationProxy = null;

            if (!string.IsNullOrWhiteSpace(organizationUri))
            {
                IServiceManagement<IOrganizationService> orgServiceManagement =
                    ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
                    new Uri($"{organizationUri}/XRMServices/2011/Organization.svc"));

                // Set the credentials.
                AuthenticationCredentials credentials = GetCredentials(orgServiceManagement, AuthenticationProviderType.None);

                // Get the organization service proxy.
                organizationProxy = GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, credentials);
                // This statement is required to enable early-bound type support.
                organizationProxy.EnableProxyTypes();
            }

            return organizationProxy;
        }

        /// <summary>
        /// Uses the global Discovery service Web API to return environment instances that the specified user
        /// is a member of.
        /// </summary>        
        /// <returns>A List of Instances</returns>
        private IEnumerable<Instance> GetInstances()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", 
                GetAccessToken(_userName, _password, 
                    new Uri("https://disco.crm.dynamics.com/api/discovery/"))); // Getting Authority from Commercial 

            client.Timeout = new TimeSpan(0, 2, 0);
            client.BaseAddress = new Uri(this._discoveryServiceAddress);

            HttpResponseMessage response = client.GetAsync("api/discovery/v2.0/Instances", HttpCompletionOption.ResponseHeadersRead).Result;

            if (response.IsSuccessStatusCode)
            {
                //Get the response content and parse it.
                string result = response.Content.ReadAsStringAsync().Result;
                JObject body = JObject.Parse(result);
                JArray values = (JArray)body.GetValue("value");
                if (!values.HasValues)
                {
                    return new List<Instance>();
                }

                return JsonConvert.DeserializeObject<List<Instance>>(values.ToString());
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Gets the authentication access token.
        /// </summary>
        /// <param name="userName">The user's username,</param>
        /// <param name="password">The user's password.</param>
        /// <param name="serviceRoot">The service root URI.</param>
        /// <returns>The access token.</returns>
        /// <remarks>The access token is only good for about one hour. In real world applications
        /// you should refresh the access token periodically so it does not expire.</remarks>
        private string GetAccessToken(string userName, string password, Uri serviceRoot)
        {
            var targetServiceUrl = GetUriBuilderWithVersion(serviceRoot);
            
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            AuthenticationParameters ap = GetAuthorityFromTargetService(targetServiceUrl.Uri);
            Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(ap.Authority, false);
            
            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            AuthenticationResult authResult;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                UserPasswordCredential cred = new UserPasswordCredential(userName, password);
                authResult = authContext.AcquireTokenAsync(ap.Resource, clientId, cred).Result;
            }
            else
            {
                // Note that PromptBehavior.Always is why the user is aways prompted when this path is executed.
                // Look up PromptBehavior to understand what other options exist. 
                PlatformParameters platformParameters = new PlatformParameters(PromptBehavior.Always);
                authResult = authContext.AcquireTokenAsync(ap.Resource, clientId, new Uri(redirectUrl), platformParameters).Result;
            }

            return authResult.AccessToken;
        }

        /// <summary>
        /// Appends '/web' to the service URI.
        /// </summary>
        /// <param name="discoveryServiceUri">The Discovery web service URI.</param>
        /// <returns>A properly formatted web service URI.</returns>
        private UriBuilder GetUriBuilderWithVersion(Uri discoveryServiceUri)
        {
            UriBuilder webUrlBuilder = new UriBuilder(discoveryServiceUri);
            string webPath = "web";

            if (!discoveryServiceUri.AbsolutePath.EndsWith(webPath))
            {
                if (discoveryServiceUri.AbsolutePath.EndsWith("/"))
                    webUrlBuilder.Path = string.Concat(webUrlBuilder.Path, webPath);
                else
                    webUrlBuilder.Path = string.Concat(webUrlBuilder.Path, "/", webPath);
            }

            UriBuilder versionTaggedUriBuilder = new UriBuilder(webUrlBuilder.Uri);
            return versionTaggedUriBuilder;
        }

        /// <summary>
        /// Get the authentication Authority and support data from the requesting system. 
        /// </summary>
        /// <param name="targetServiceUrl">Resource URL</param>
        /// <param name="logSink">Log tracer</param>
        /// <returns>Populated AuthenticationParameters or null</returns>
        /// <remarks>Handles breaking API changes in the various versions of ADAL.</remarks>
        private AuthenticationParameters GetAuthorityFromTargetService(Uri targetServiceUrl)
        {
            try
            {
                // if using ADAL > 4.x  return.. // else remove oauth2/authorize from the authority
                if (_ADALAsmVersion == null)
                {
                    // initial setup to get the ADAL version 
                    var adalAsm = System.Reflection.Assembly.GetAssembly(typeof(IPlatformParameters));
                    if (adalAsm != null)
                        _ADALAsmVersion = adalAsm.GetName().Version;
                }

                AuthenticationParameters foundAuthority;
                if (_ADALAsmVersion != null && _ADALAsmVersion >= Version.Parse("5.0.0.0"))
                {
                    foundAuthority = CreateFromUrlAsync(targetServiceUrl);
                }
                else
                {
                    foundAuthority = CreateFromResourceUrlAsync(targetServiceUrl);
                }

                if (_ADALAsmVersion != null && _ADALAsmVersion > Version.Parse("4.0.0.0"))
                {
                    foundAuthority.Authority = foundAuthority.Authority.Replace("oauth2/authorize", "");
                }

                return foundAuthority;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Creates authentication parameters from the address of the resource.
        /// </summary>
        /// <param name="targetServiceUrl">Resource URL</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        private AuthenticationParameters CreateFromResourceUrlAsync(Uri targetServiceUrl)
        {
            var result = (Task<AuthenticationParameters>)typeof(AuthenticationParameters)
                .GetMethod("CreateFromResourceUrlAsync").Invoke(null, new[] { targetServiceUrl });
            return result.Result;
        }

        /// <summary>
        /// Creates authentication parameters from the address of the resource.
        /// Invoked for ADAL v5+ which changed the method used to retrieve authentication parameters.
        /// </summary>
        /// <param name="targetServiceUrl">Resource URL</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        private AuthenticationParameters CreateFromUrlAsync(Uri targetServiceUrl)
        {
            var result = (Task<AuthenticationParameters>)typeof(AuthenticationParameters)
                .GetMethod("CreateFromUrlAsync").Invoke(null, new[] { targetServiceUrl });

            return result.Result;
        }

        /// <summary>
        /// Obtain the AuthenticationCredentials based on AuthenticationProviderType.
        /// </summary>
        /// <param name="service">A service management object.</param>
        /// <param name="endpointType">An AuthenticationProviderType of the CRM environment.</param>
        /// <returns>Get filled credentials.</returns>
        private AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        {
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(_userName,
                            _password,
                            _domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    throw new Exception("LiveId is no longer used.");
                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  // Windows Kerberos

                    break;
            }

            return authCredentials;
        }

        /// <summary>
        /// Finds a specific organization detail in the array of organization details
        /// returned from the Discovery service.
        /// </summary>
        /// <param name="orgUniqueName">The unique name of the organization to find.</param>
        /// <param name="orgDetails">Array of organization detail object returned from the discovery service.</param>
        /// <returns>Organization details or null if the organization was not found.</returns>
        /// <seealso cref="DiscoveryOrganizations"/>
        private Instance FindOrganization(string orgUniqueName,
            IEnumerable<Instance> orgDetails)
        {
            if (string.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");

            Instance orgDetail = null;

            foreach (Instance detail in orgDetails)
            {
                if (string.Compare(detail.UrlName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            return orgDetail;
        }

        /// <summary>
        /// Generic method to obtain discovery/organization service proxy instance.
        /// </summary>
        /// <typeparam name="TService">
        /// Set IDiscoveryService or IOrganizationService type to request respective service proxy instance.
        /// </typeparam>
        /// <typeparam name="TProxy">
        /// Set the return type to either DiscoveryServiceProxy or OrganizationServiceProxy type based on TService type.
        /// </typeparam>
        /// <param name="serviceManagement">An instance of IServiceManagement</param>
        /// <param name="authCredentials">The user's Microsoft Dynamics CRM logon credentials.</param>
        /// <returns></returns>
        /// <snippetAuthenticateWithNoHelp4>
        private TProxy GetProxy<TService, TProxy>(
            IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }
    }
}
