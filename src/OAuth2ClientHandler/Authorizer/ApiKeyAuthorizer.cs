using OAuth2ClientHandler.Authorizer;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OAuth2ClientHandler.Authorizer
{
	internal sealed class ApiKeyAuthorizer : IAuthorizer
	{
		private readonly ApiKeyAuthorizerOptions _options;

		internal ApiKeyAuthorizer(ApiKeyAuthorizerOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}


		public async Task<TokenResponse> GetToken(CancellationToken? cancellationToken = null)
		{
			if (_options.ApiKey == null) throw new ArgumentNullException(nameof(_options.ApiKey));

			var tokenResponse = new TokenResponse
			{
				AccessToken = _options.ApiKey,
				ExpiresInSeconds = (_options.ExpiresInSeconds.HasValue?_options.ExpiresInSeconds.Value:3600),
				RefreshToken = null,
				TokenType = "ApiKey"
			};

			return tokenResponse;
		}
	}
}
