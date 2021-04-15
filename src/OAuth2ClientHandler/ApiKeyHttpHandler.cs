using OAuth2ClientHandler.Authorizer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OAuth2ClientHandler
{
	public class ApiKeyHttpHandler : DelegatingHandler
	{
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ApiKeyHttpHandlerOptions _options;
        private readonly bool _ownsHandler = false;
        private readonly IAuthorizer _authorizer;
        private TokenResponse _tokenResponse;

		public ApiKeyHttpHandler(ApiKeyHttpHandlerOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
            InnerHandler = options.InnerHandler ?? new HttpClientHandler();
            _ownsHandler = options.InnerHandler == null;
            _authorizer = new Authorizer.ApiKeyAuthorizer(options.AuthorizerOptions);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _ownsHandler)
                InnerHandler.Dispose();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                var tokenResponse = await GetTokenResponse(cancellationToken);
                if (tokenResponse != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
            {
                var tokenResponse = await RefreshTokenResponse(cancellationToken);
                if (tokenResponse != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<TokenResponse> GetTokenResponse(CancellationToken cancellationToken)
        {
            try
            {
                _semaphore.Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return null;
                _tokenResponse = _tokenResponse ?? await _authorizer.GetToken(cancellationToken);
                return _tokenResponse;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<TokenResponse> RefreshTokenResponse(CancellationToken cancellationToken)
        {
            try
            {
                _semaphore.Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return null;
                _tokenResponse = await _authorizer.GetToken(cancellationToken);
                return _tokenResponse;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
