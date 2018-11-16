using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;

namespace JiraTestApp
{
    class RequestSetup
    {
        public int startAt = Int32.Parse(ConfigurationManager.AppSettings.Get("startAt"));
        public int maxResults = Int32.Parse(ConfigurationManager.AppSettings.Get("maxResults"));
        public int totalIssues = 0;

        private string prefixJql = ConfigurationManager.AppSettings.Get("prefixJql");
        public string jql = "";

        private RestClient client;
        private RestRequest request;
        public void InitializeRequest(bool existingTable)
        {
            client = new RestClient();
            request = new RestRequest(Method.GET);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Authorization", "Basic RVQ6ZXQxMjM=");
            // "Basic" keyword for Authorization Type and then a hashed login credential (Username and Password).
            if (!existingTable)
            {
                jql = ConfigurationManager.AppSettings["prefixJql"]
                    + startAt.ToString() + "&maxResults="
                    + maxResults.ToString();
            }
            else
            {
                jql = ConfigurationManager.AppSettings["updateJql"];
            }
        }
        // Sets instances of the client and GET request. Defines jql. Could do an overloaded default constructor but
        // this way initialization step is more visible.

        public void SetNextRequest()
        {
            startAt += maxResults;
            jql = prefixJql + startAt.ToString() + "&maxResults=" + maxResults.ToString();
        }
        // Increments startAt for paginated requests. Updates jql to reflect the change.

       
        public IRestResponse GetResponse(string jql)
        {
            client.BaseUrl = new System.Uri(jql);
            IRestResponse response = client.Execute(request);
            // RestSharp request execution.

            // Begin checking for and retrying failed requests below.
            if(!response.IsSuccessful)
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.Error.WriteLine("Response Failed");
                    Console.Error.WriteLine(client.BaseUrl + "\n"
                        + response.Request + "\n" + response.ResponseUri);
                    Console.WriteLine("Retry " + (i + 1) + "...\n");
                    response = client.Execute(request);
                    // Try it again, up to 3 times.
                    if(response.IsSuccessful)
                    {
                        break;
                        // Once we have a successful request to the same url, break from
                        // the retry loop.
                    }
                }
                // End of retry loop
            }
            return response;
        }
        // Sends a GET request with the passed jql string.
        // Instantiation and headers of the request are set through InitializeRequest()
    }
}
