using Infor.DocumentManagement.ICP;
using System;
using System.IO;

namespace Example
{
	class Authentication
	{
		static void Main(string[] args)
		{
			try
			{
				///////////////////////////////////////////////////////////////////////////////////////
				// This example class shows some of the more common ways of authenticating with IDM. //
				// In all the examples the "general purpose" Connection constructor is used but it   //
				// is also possible to use the default constructor and set the different parameters  //
				// separately for most authentication modes (but not all).
				///////////////////////////////////////////////////////////////////////////////////////

				// Using Basic authentication, this is the simplest approach using a username and password. But it will also not
				// work in all scenarios. The requires a Basic enabled router in Infor Grid which in most cases (but not all) can 
				// be created/used when IDM  has been installed using the on-premise Xi installer.
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();
				conn.Disconnect();

				// Using OAuth 1.0a authentication, a consumer key and secret key is needed for this, these keys can be generated through
				// the Infor Grid Admin UI.
				Connection conn2 = new Connection("https://<server>:<port>/ca/", "<consumer key>", "<secret key>", AuthenticationMode.OAuth1);
                // This connection is usually put in a connection pool for optimal performance
                conn2.Connect();
				// Before doing an actual call we must set the tenant (not needed on-premise) and the IFS username (the Identity2 claim)
				conn2.Tenant = "<tenant>";
				conn2.Username = "<IFS username (UPN or Guid)>";
				// Then we can do the API call, the connection doesn't need to be disconnected when impersonation has been used

				// Using OAuth 2.0 authentication ("Resource Owner Grant"), this should not be confused with "Bearer token" authentication.
				// The .ionapi file can be generated through the ION Api application running in Ming.le.
				// With the "general purpose" constructor it is possible to either specify the filepath to the .ionapi file or to read the
				// .ionapi Json file into a String, the argument not used should be set to null.
				// There is no need to specify the url since that is automatically calculated from the .ionapi file.
				// All calls here will be executed against the ION Api server, so not the IDM Server.
				Connection conn3 = new Connection(null, "<.ionapi filepath>", "<.ionapi Json file>", AuthenticationMode.OAuth2);
				conn3.Connect();
				conn3.Disconnect();

				// Hybrid ION (1): Using OAuth2.0 authentication("Resource Owner Grant") with
				// impersonation/Id transalation, same as that of Oauth2.0 authentication
				// A privateKey should is needed to use impersonation/Id translation
				// To impersonate the Identity2 of the user should be set as
				// ImpersonationIdValue.
				// To Use ID translation CrossRefClaim should be set. CrossRefClaim is
				// configured while downloading .ionapi Json from ION API UI.
				// We can also set both CrossRefClaim and ImpersonationIDValue but the
				// IdTranslation takes precedence in such cases
				Connection conn4 = new Connection(null, "<.ionapi filepath>", "<.ionapi Json file>", AuthenticationMode.OAuth2);
				conn4.SetPrivateKey("<privateKey FilePath>", "<privateKey Json/Pem File>");
				conn4.CrossRefClaim = "<CrossRefClaim value>";
				conn4.ImpersonationIDValue = "<Identity2>";
				conn4.Connect();
				conn4.Disconnect();

				//Hybrid ION (2): We can also make use of constructor to set private Key. Rest is same
				Connection conn5 = new Connection(null, "<.ionapi filepath>", "<.ionapi Json file>", AuthenticationMode.OAuth2, "<privateKey FilePath>", "<privateKey Json/Pem File>");
				conn5.CrossRefClaim = "<CrossRefClaim value>";
				conn5.ImpersonationIDValue = "<Identity2>";
				conn5.Connect();
				conn5.Disconnect();

				// Using a Http Authorization header for authentication, this us usually used together with an OAuth2 Bearer token.
				// This token must be generated on the server side, for Infor Grid applications there is support already in Infor Grid to 
				// generate this but for other application they will need to do it themselves. This SDK will not help with the generation of the token.
				// The token looks like this: "Bearer 16acd3a25fc9bc810539f5de656df53c"
				Connection conn6 = new Connection("https://<ION Api server>/<tenant>/IDM", "<the Bearer token>", null, AuthenticationMode.Authorization);
				conn6.Connect();
				conn6.Disconnect();

                // Using OAuth 1.0a authentication, a consumer key and secret key is needed for this, these keys can be generated through
                // the Infor Grid Admin UI. For server-to-server communication this is the recommended approach, especially since it supports
                // impersonating a user and tenant.
                Connection conn7 = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>", AuthenticationMode.Token);
				// This connection is usually put in a connection pool for optimal performance
				conn7.Connect();
				// Before doing an actual call we must set the tenant (not needed on-premise) and the IFS username (the Identity2 claim)
				conn7.Tenant = "<tenant>";
				conn7.Username = "<IFS username (UPN or Guid)>";
				// Then we can do the API call, the connection doesn't need to be disconnected when impersonation has been used	
				//Private Space : Set PrivateSpace value true in Connection and only token authentication mode is allowed
				Connection conn8 = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>", AuthenticationMode.Token);
				conn8.Connect();
				conn8.Tenant = "<tenant>";
				conn8.Username = "<IFS username (UPN or Guid)>";
				conn8.isPrivateSpace = true;

                // MDC traceability headers can be set on any connection regardless of authentication mode.
                // CorrelationId: auto-generated with "req-" prefix if not set
                // conn.CorrelationId = "req-abc12345-def6-7890";
                // CallerId: auto-generated based on auth mode:
                //   OAuth1:        DOTNETSDK-OAuth1-{first 4 chars of ConsumerKey}
                //   OAuth2:        DOTNETSDK-OAuth2-{first 4 chars of ClientId}
                //   Token:         DOTNETSDK-Token-{clienttype from JWT}
                //   Authorization: DOTNETSDK-Auth
                // conn.CallerId = "MyCustomApp"; 
            }
            catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
