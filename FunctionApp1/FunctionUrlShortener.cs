using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Net;

namespace FunctionApp1
{
    public static class FunctionUrlShortener
    {

        // "https://www.example.com"
        [FunctionName("UrlShortener")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string connectionString = "Data Source=localhost;Initial Catalog=AzureFunc;Integrated Security=True;TrustServerCertificate=True;";
            int rowCount = 1;

            string url = req.Query["url"];
            string messsage = "";
            string shortUrl = $"www.getit.com/";

            #region get rowCount 
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM url_mapping";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        rowCount = (int)command.ExecuteScalar();
                    }
                }
            }

            catch (Exception ex)
            {

                //throw new Exception("Unable to get row count.  " + ex);
            }
            #endregion

            shortUrl += $"{rowCount + 1}";

            #region URL validation
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                messsage = "Valid URL";
            }
            else
            {
                messsage = "Invalid URL";
            }
            #endregion

            #region sql
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO url_mapping (long_url, short_url) VALUES (@LongUrl, @ShortUrl)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@LongUrl", url);
                        command.Parameters.AddWithValue("@ShortUrl", shortUrl);
                        command.ExecuteNonQuery();
                    }
                }

                log.LogInformation("Data inserted into the table successfully.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while inserting data into the table.");
            }
            #endregion

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string responseMessage = string.IsNullOrEmpty(url)
             ? $"{messsage}"
             : $"Your shortened version of <a href=\"{url}\">{url}</a> is: <a href=\"{shortUrl}\">{shortUrl}</a>";

            return new ContentResult
            {
                Content = responseMessage,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}