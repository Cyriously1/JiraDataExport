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
    class Program
    {

        static void Main(string[] args)
        {
            QuerySetup qSetup;
            string errorMessages = "";

            try
            {
                qSetup = new QuerySetup();
                qSetup.InitializeCommand();
            }
            catch (SqlException ex)
            {               
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages = "Error " + (i + 1) + ":\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n";
                }
                Console.WriteLine(errorMessages + "Exiting");
                return;
                // No point making someone wait for parsing to fail when the connection didn't work out. 
            }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            // Allow for null/empty values in Jira, and also for parameters to be skipped over. I'm not taking
            // advantage of that 2nd part but it's safer for future changes.

            RequestSetup rSetup = new RequestSetup();
            rSetup.InitializeRequest(qSetup.TableExists());

            do{
                IRestResponse currentResponse = rSetup.GetResponse(rSetup.jql);
                if(!currentResponse.IsSuccessful)
                {
                    Console.Error.WriteLine("Unsuccessful Retries\n"
                        + "Request for Jira data failed\n"
                        + "Exiting");
                    return;
                }
                JObject jsonParsed = JObject.Parse(currentResponse.Content);
                // Converts the response data into a proper json object.
                rSetup.totalIssues = (int)jsonParsed["total"];
                var issuesJsonObject = jsonParsed["issues"];
                // Reference the "issues" key whose value is an array of JSON objects.
                var issuesList = JsonConvert.DeserializeObject<List<Issue>>(issuesJsonObject.ToString(), settings);
                // Deserialize into the objects with matching property flags.       
                
                foreach (var singleIssue in issuesList)
                {
                    Fields currentFields = singleIssue.fields;
                    qSetup.DefineCommandParameters(currentFields, singleIssue.key);
                    if (qSetup.TableExists() && qSetup.ExistingIssue(singleIssue))
                    {         
                        qSetup.ExecuteUpdate(singleIssue);
                    }
                    else
                    {
                        qSetup.ExecuteInsert(currentFields, singleIssue.key);
                    }
                    // SQL statement execution for 1 row.
                }
                // This approach avoids the messy sql string appending to craft a query.
                // If one row were to fail, the whole execution wouldn't fail since it's going one row at a time.
                // The downside to "issue-by-issue" is the number of executions. Since the cap on what we 
                // can call from jira is 1000, it's not like we could do a 300k line INSERT statement anyway.
                // Looking into the SqlBulkCopy method of writing to the server. Recommended for single queries over 10,000 rows

                rSetup.SetNextRequest();
                // Jql can only query 1000 at a time so we move the start point until we've passed the total.
                Console.WriteLine(rSetup.startAt);
                // If failure occurs, this gives a decent approximation of where.
                // Null reference errors will usually be solved by giving default definitions.
                // Inconsistency in failure points is request related but retries should ensure safety.

            } while (rSetup.startAt < rSetup.totalIssues);

            qSetup.CloseConnection();

        }
       

    }
}
