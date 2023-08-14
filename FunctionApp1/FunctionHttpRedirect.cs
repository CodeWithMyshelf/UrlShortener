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
using FunctionApp1.Models;
using Microsoft.Azure.Functions.Worker;
using System.Collections.Generic;

namespace FunctionApp1
{
    public static class FunctionHttpRedirect
    {
        [FunctionName("FunctionHttpRedirect")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string connectionString = "Data Source=localhost;Initial Catalog=AzureFunc;Integrated Security=True;TrustServerCertificate=True;";
            string shortUrl = req.Query["shortUrl"];

            UrlMapping mapping = new UrlMapping();

            #region get rowCount 

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM url_mapping WHERE short_url = @ShortUrl";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ShortUrl", shortUrl);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mapping = new UrlMapping
                                {
                                    Id = (int)reader["id"],
                                    LongUrl = (string)reader["long_url"],
                                    ShortUrl = (string)reader["short_url"]
                                };
                                return new RedirectResult(mapping.LongUrl, permanent: false); // Perform a temporary redirect (HTTP 302)

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }

            #endregion

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            return new OkObjectResult(mapping);
        }
    }
}
