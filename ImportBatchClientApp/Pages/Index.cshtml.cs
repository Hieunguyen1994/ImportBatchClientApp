using ImportBatchClientApp.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ImportBatchClientApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public string XmlInput { get; set; }

        [BindProperty]
        public string Filename { get; set; }

        [BindProperty]
        public string ExternalReference { get; set; }
        [BindProperty]
        public string Format { get; set; }

        public string Token { get; set; }
        public string ApiResponse { get; set; }
        public string TokenResponse { get; set; }

        /// <summary>
        /// Handles the POST request to the page.
        /// </summary>
        public async Task OnPostAsync()
        {
            // Step 1: Retrieve the token
            Token = await GetApiTokenAsync();
            if (Token == null)
            {
                return;
            }

            // Step 2: Prepare request data with XML content
            var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(XmlInput ?? ""));
            var requestData = new RequestData<string>
            {
                Data = base64Data,
                Filename = Filename,
                ExternalReference = ExternalReference,
                Format = Format
            };

            // Step 3: Call the ImportBatchInformation API
            ApiResponse = await CallImportBatchInformationApiAsync(requestData);
        }

        /// <summary>
        /// Retrieves an API token from the authentication server.
        /// </summary>
        /// <returns>The API token as a string, or null if the request fails.</returns>
        private async Task<string> GetApiTokenAsync()
        {
            var url = "http://localhost/identity/connect/token";
            var requestData = new Dictionary<string, string>
            {
                { "client_id", "gf.his.integration" },
                { "client_secret", "default@123" },
                { "grant_type", "client_credentials" },
                { "Scopes", "openid profile offline_access email roles bridge.api.integration" }
            };

            var requestContent = new FormUrlEncodedContent(requestData);

            try
            {
                var response = await _httpClient.PostAsync(url, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    // Extract detailed error information from the response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TokenResponse = $"Failed to retrieve token. Status code: {response.StatusCode}. Error: {errorContent}";
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                return json["access_token"]?.ToString();
            }
            catch (HttpRequestException httpEx)
            {
                // Log or return a detailed message for HTTP request failures
                TokenResponse = $"HTTP request error: {httpEx.Message}";
                return null;
            }
            catch (Exception ex)
            {
                // Log or return a detailed message for any other exceptions
                TokenResponse = $"Unexpected error: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Calls the ImportBatchInformation API with the provided request data.
        /// </summary>
        /// <param name="requestData">The data to be sent to the API.</param>
        /// <returns>The API response as a string, or an error message if the request fails.</returns>
        private async Task<string> CallImportBatchInformationApiAsync(RequestData<string> requestData)
        {
            var url = "http://localhost/bridge-api/api/batch";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var jsonContent = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return $"API Response: {responseContent}";
            }
            catch (Exception ex)
            {
                return $"Error calling API: {ex.Message}";
            }
        }
    }
}
