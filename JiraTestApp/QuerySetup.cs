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
    class QuerySetup
    {

        private string tableName = ConfigurationManager.AppSettings.Get("tableName");
        private bool existingTable = true;
        private SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["vmsfetdbConnectionString"].ConnectionString);
        // remote server connection

        //private SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["JiraTestApp.Properties.Settings.TestDBConnectionString"].ConnectionString);
        //local server test connection

        private SqlCommand cmd = new SqlCommand();
        
        public bool TableExists()
        {
            return existingTable;
        }

        public static string[] listNames = new string[] {
            "Components", //2
            "Client",     //5
            "Category",   //17
            "Labels",     //27
            "Fix_Versions",//25
            "Affects_Versions"//36
        }; // hook for future generalization to decrease code modifications when columns in the DB are changed

        public static string[] columnNames = new string[] {
            "IssueKey",
            "Assignee",
            "Components",
            "Created",
            "Creator",
            "Client",
            "Origin",
            "Product_and_Version",
            "Root_Cause",
            "Severity_Level",
            "QA_Assigned",
            "LP_Assigned",
            "AE_Assigned",
            "Priority_Level",
            "SE_Assigned",
            "Needs_Documentation",
            "Epic_Link",
            "Category",
            "Account_Name",
            "Copy_Number",
            "Planning_Rank",
            "Idea_ID",
            "Aha_Reference",
            "Regression",
            "Team",
            "Fix_Versions",
            "Issue_Type",
            "Labels",
            "Priority",
            "Project",
            "Reporter",
            "Resolution",
            "Resolved",
            "Status",
            "Summary",
            "Updated",
            "Affects_Versions"
        };

        public static string[] JsonFieldNames = new string[]
        {
            "key",
            "assignee",
            "components",
            "created",
            "creator",
            "customfield_10003",
            "customfield_10012",
            "customfield_10014",
            "customfield_10020",
            "customfield_10021",
            "customfield_10027",
            "customfield_10031",
            "customfield_10032",
            "customfield_10034",
            "customfield_10100",
            "customfield_10900",
            "customfield_12400",
            "customfield_12406",
            "customfield_13105",
            "customfield_13106",
            "customfield_13501",
            "customfield_13700",
            "customfield_13803",
            "customfield_13805",
            "customfield_14701",
            "fixVersions",
            "issuetype",
            "labels",
            "priority",
            "project",
            "reporter",
            "resolution",
            "resolutiondate",
            "status",
            "summary",
            "updated",
            "versions"
        };


        public string GetInsertSQLString()
        {
            string sql = "";
            sql = "INSERT INTO " + this.tableName + "(";

            foreach (var s in columnNames)
            {
                sql += s + ", ";
            }
            // Add all column names.
            sql = sql.Remove(sql.Length - 2);
            sql += ") VALUES (";
            // Remove the final ", " and prep for VALUES.

            foreach (var s in columnNames)
            {
                sql += "@" + s + ", ";
            }
            // Add all parameter flags for the command text. This does not add the actual parameters to the command object.
            sql = sql.Remove(sql.Length - 2);
            sql += ")";

            return sql;
        }
        //Returns query string for use by the command text.

        public string GetUpdateSqlString(Issue singleIssue)
        {
            string sql = "";

            sql = "UPDATE " + this.tableName + "\n"
                + "SET ";

            foreach (var s in columnNames)
            {
                sql += s + " = @" + s + ", ";
            }
            sql = sql.Remove(sql.Length - 2);
            sql += "\nWHERE IssueKey = '" + singleIssue.key + "'"; // exploring an alternative solution for this
            return sql;
        }

        public void InitializeCommand()
        {
            cnn.Open();
            cmd.Connection = cnn;
            //checking for an existing table with the same name

            cmd.CommandText = "SELECT * FROM " + tableName;
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                existingTable = true;
                reader.Close();
            }
                catch(SqlException)
            {
                existingTable = false;
                    
            }

            if (!existingTable)
            {
                cmd.CommandText = "CREATE TABLE " + tableName + "(\n";
                foreach (var column in columnNames)
                {
                    cmd.CommandText = cmd.CommandText + column + " varchar(255),\n";
                }
                cmd.CommandText.Remove(cmd.CommandText.Length - 2);
                cmd.CommandText += "\n);";
            
                cmd.ExecuteNonQuery();
                cmd.CommandText = "ALTER TABLE " + tableName + "\n"
                    + "ALTER COLUMN\nIssueKey\nvarchar(255) NOT NULL;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "ALTER TABLE " + tableName + "\n"
                    + "ADD PRIMARY KEY (IssueKey);";
                cmd.ExecuteNonQuery();
                // Set the special characteristics of IssueKey after creation to avoid a special check inside the
                // for-loop and to avoid modifying the string array
                try
                {
                    cmd.CommandText = "CREATE TABLE JiraLabels(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nLabel varchar(255)\n);\nCREATE TABLE JiraClients(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nClient varchar(255)\n);\nCREATE TABLE JiraFixVersions(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nFix_Version varchar(255)\n);\nCREATE TABLE JiraAffectsVersions(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nAffects_Version varchar(255)\n);\nCREATE TABLE JiraComponents(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nComponent varchar(255)\n);\nCREATE TABLE JiraCategories(\nIssueKey varchar(255) FOREIGN KEY REFERENCES JiraExport5(IssueKey),\nCategory varchar(255)\n);\n";
                    cmd.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Creating Relational Tables. This is usually caused if there is no existing JiraExport table, but the remnant relational tables were left behind.");
                }
                
            }
            cmd.CommandText = GetInsertSQLString();
            
            foreach(var s in columnNames)
            {
                cmd.Parameters.Add(("@" + s), SqlDbType.VarChar, 255);
                // e.g. cmd.Parameters.Add("@Assignee"), SqlDbType.VarChar, 255);
                // Adds parameters to match the corresponding parameter flags in the command text.
            }            
            
            // There's a catch block in main in case this can't open.
        }

        public void DefineCommandParameters(Fields currentFields, string key)
        {
            //cmd.Parameters["@Components"].Value = issuesJsonObject[i]["fields"]["components"][0]["name"].ToString();
            // deserializing again adds too much to the run time. changed the loop from a foreach to a for to make this work
            // too scared do delete the above line for now because of how log it took. changed the for back to a foreach
            // because I found a much better way to implement this but this method of reference might be useful later.

            cmd.Parameters["@IssueKey"].Value = key;
            cmd.Parameters["@Assignee"].Value = currentFields.assignee.name;
            cmd.Parameters["@Components"].Value = currentFields.GetComponentsString();
            cmd.Parameters["@Created"].Value = currentFields.created;
            cmd.Parameters["@Creator"].Value = currentFields.creator.name;
            cmd.Parameters["@Client"].Value = currentFields.GetClients();
            cmd.Parameters["@Origin"].Value = currentFields.origin.value;
            cmd.Parameters["@Product_and_Version"].Value = currentFields.productAndVersion.value;
            cmd.Parameters["@Root_Cause"].Value = currentFields.rootCause.value;
            cmd.Parameters["@Severity_Level"].Value = currentFields.severityLevel.value;
            cmd.Parameters["@QA_Assigned"].Value = currentFields.qaAssigned.name;
            cmd.Parameters["@LP_Assigned"].Value = currentFields.lpAssigned.name;
            cmd.Parameters["@AE_Assigned"].Value = currentFields.aeAssigned.name;
            cmd.Parameters["@Priority_Level"].Value = currentFields.priorityLevel.name;
            cmd.Parameters["@SE_Assigned"].Value = currentFields.seAssigned.name;
            cmd.Parameters["@Needs_Documentation"].Value = currentFields.needsDocumentation.value;
            cmd.Parameters["@Epic_Link"].Value = currentFields.epicLink;
            cmd.Parameters["@Category"].Value = currentFields.GetCategoriesString();
            cmd.Parameters["@Account_Name"].Value = currentFields.accountName;
            cmd.Parameters["@Copy_Number"].Value = currentFields.copyNumber;
            cmd.Parameters["@Planning_Rank"].Value = currentFields.planningRank;
            cmd.Parameters["@Idea_ID"].Value = currentFields.ideaID;
            cmd.Parameters["@Aha_Reference"].Value = currentFields.ahaReference.value;
            cmd.Parameters["@Regression"].Value = currentFields.regression.value;
            cmd.Parameters["@Team"].Value = currentFields.team;
            cmd.Parameters["@Fix_Versions"].Value = currentFields.GetFixVersionsString();
            cmd.Parameters["@Issue_Type"].Value = currentFields.issuetype.name;
            cmd.Parameters["@Labels"].Value = currentFields.GetLabelsString();
            cmd.Parameters["@Priority"].Value = currentFields.priority.id;
            cmd.Parameters["@Project"].Value = currentFields.project.name;
            cmd.Parameters["@Reporter"].Value = currentFields.reporter.name;
            cmd.Parameters["@Resolution"].Value = currentFields.resolution.name;
            cmd.Parameters["@Resolved"].Value = currentFields.resolutiondate;
            cmd.Parameters["@Status"].Value = currentFields.status.name;
            cmd.Parameters["@Summary"].Value = currentFields.summary;
            cmd.Parameters["@Updated"].Value = currentFields.updated;
            cmd.Parameters["@Affects_Versions"].Value = currentFields.GetVersionsString();
            // Define each parameter. Working on reworking the classes to make this automatable.

        }

        public void ExecuteInsert(Fields currentFields, string key)
        {
            cmd.ExecuteNonQuery();
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = cnn;

            foreach (var version in currentFields.versions)
            {
                cmd2.CommandText = "INSERT INTO JiraAffectsVersions(IssueKey, Affects_Version) VALUES ('" + key + "', '" + version + "')";
                cmd2.ExecuteNonQuery();
            }
            foreach (var label in currentFields.labels)
            {
                cmd2.CommandText = "INSERT INTO JiraLabels(IssueKey, Label) VALUES ('" + key + "', '" + label + "')";
                cmd2.ExecuteNonQuery();
            }
            foreach (var fixVersionItem in currentFields.fixVersions)
            {
                cmd2.CommandText = "INSERT INTO JiraFixVersions(IssueKey, Fix_Version) VALUES ('" + key + "', '" + fixVersionItem.name + "')";
                cmd2.ExecuteNonQuery();
            }
            foreach (var client in currentFields.clients)
            {
                cmd2.CommandText = "INSERT INTO JiraClients(IssueKey, Client) VALUES ('" + key + "', '" + client.value + "')";
                cmd2.ExecuteNonQuery();
            }
            foreach (var component in currentFields.components)
            {
                cmd2.CommandText = "INSERT INTO JiraComponents(IssueKey, Component) VALUES ('" + key + "', '" + component.value + "')";
                cmd2.ExecuteNonQuery();
            }
            foreach (var category in currentFields.categories)
            {
                cmd2.CommandText = "INSERT INTO JiraCategories(IssueKey, Category) VALUES ('" + key + "', '" + category.value + "')";
                cmd2.ExecuteNonQuery();
            }            
        }
        // Executes the current command. Naming for clarity of request type used in this program.

        public void CloseConnection()
        {
            cnn.Close();
        }


        public bool ExistingIssue(Issue singleIssue)
        {
            
            string temp = cmd.CommandText;
            int issueCount = 0;
            cmd.CommandText = "SELECT COUNT(*) from " + tableName + " where IssueKey = '" + singleIssue.key + "'";
            issueCount = (int)cmd.ExecuteScalar();
            cmd.CommandText = temp;

            return (issueCount > 0);
        }

        public void ExecuteUpdate(Issue singleIssue)
        {
            string temp = cmd.CommandText;
            cmd.CommandText = GetUpdateSqlString(singleIssue);
            cmd.ExecuteNonQuery();
            cmd.CommandText = temp;

            SqlCommand cmd2 = new SqlCommand();
            cmd2.CommandText = "DELETE FROM JiraCategories Where IssueKey = '" + singleIssue.key + "'";
            cmd2.ExecuteNonQuery();
            cmd2.CommandText = "DELETE FROM JiraLabels Where IssueKey = '" + singleIssue.key + "'";
        }

    }
}
