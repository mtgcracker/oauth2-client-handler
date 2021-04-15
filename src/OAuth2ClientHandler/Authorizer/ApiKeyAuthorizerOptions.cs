namespace OAuth2ClientHandler.Authorizer
{
	public class ApiKeyAuthorizerOptions
	{
		public string ApiKey { get; set; }
		public int? ExpiresInSeconds { get; set; }
	}
}