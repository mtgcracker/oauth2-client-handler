using OAuth2ClientHandler.Authorizer;
using System.Net.Http;

namespace OAuth2ClientHandler
{
	public class ApiKeyHttpHandlerOptions
	{
		public ApiKeyAuthorizerOptions AuthorizerOptions { get; set; }

		public HttpMessageHandler InnerHandler { get; set; }

		public ApiKeyHttpHandlerOptions()
		{
			AuthorizerOptions = new ApiKeyAuthorizerOptions();
		}
	}
}