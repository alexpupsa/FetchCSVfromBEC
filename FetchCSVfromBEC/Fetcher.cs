using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FetchCSVfromBEC
{
    public class Fetcher
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient = new HttpClient();

        public Fetcher(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Fetcher>();
        }

        [Function("fetch-csv")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            var url = req.Query["url"];

            if (string.IsNullOrEmpty(url))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Please provide a valid URL.");
                return response;
            }

            try
            {
                var fileBytes = await _httpClient.GetByteArrayAsync(url);
                var fileName = Path.GetFileName(url);
                response.StatusCode = HttpStatusCode.OK;
                response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
                await response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from URL: {url}", url);
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("An error occurred while downloading the file.");
                return response;
            }
        }
    }
}
