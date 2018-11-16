using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JiraTestApp
{


    public class Issue
    {
        public string id = "";
        public string self = "";

        [JsonProperty("key")]
        public string key = "";

        [JsonProperty("fields")]
        public Fields fields { get; set; }
        // left this as default get/set because of null reference errors with keys during 
        // deserialization. Applied a fix for those, but if there's a null in place of a fields object
        // the program should break and I want it to be loud.
    }
    public class Fields
    {

        [JsonProperty("assignee")]
        public Assignee assignee = new Assignee();

        [JsonProperty("components")]
        public List<Component> components = new List<Component>();

        public string GetComponentsString()
        {
            string componentsString = "";
            if (components.Count > 0)
            {
                foreach (var c in components)
                {
                    componentsString += c.value;
                    componentsString += ", ";
                }
                componentsString = componentsString.Remove(componentsString.Length - 2);
            }
            return componentsString;
        }
        // returns the components list as a single string for the SQL query

        [JsonProperty("created")]
        public string created = "";

        [JsonProperty("creator")]
        public Creator creator = new Creator();

        [JsonProperty("customfield_10003")]
        public List<Client> clients = new List<Client>();

        public string GetClients()
        {
            string s = "";
            if (clients.Count > 0)
            {
                foreach (var c in clients)
                {
                    s += c.value;
                    s += ", ";
                }
                s = s.Remove(s.Length - 2);
            }
            return s;
        }
        

        [JsonProperty("customfield_10012")]
        public Origin origin = new Origin();

        [JsonProperty("customfield_10014")]
        public ProductAndVersion productAndVersion = new ProductAndVersion();

        [JsonProperty("customfield_10020")]
        public RootCause rootCause = new RootCause();

        [JsonProperty("customfield_10021")]
        public SeverityLevel severityLevel = new SeverityLevel();

        [JsonProperty("customfield_10027")]
        public QAAssigned qaAssigned = new QAAssigned();

        [JsonProperty("customfield_10031")]
        public LPAssigned lpAssigned = new LPAssigned();

        [JsonProperty("customfield_10032")]
        public AEAssigned aeAssigned = new AEAssigned();

        [JsonProperty("customfield_10034")]
        public PriorityLevel priorityLevel = new PriorityLevel();

        [JsonProperty("customfield_10100")]
        public SEAssigned seAssigned = new SEAssigned();

        [JsonProperty("customfield_10900")]
        public NeedsDocumentation needsDocumentation = new NeedsDocumentation();
 
        [JsonProperty("customfield_12400")]
        public string epicLink = "";


        [JsonProperty("customfield_12406")]
        public List<CategoryItem> categories = new List<CategoryItem>();

        public string GetCategoriesString()
        {
            string s = "";
            if (categories.Count > 0)
            {
                foreach (var c in categories)
                {
                    s += c.value;
                    s += ", ";
                }
                // TODO figure out how s.Length-2 could have possibly given OutOfBounds. Added an extra IF to fix for now
                // even if it had components with blank values, the append of ", " should have always ensured safety 
                if(s.Length > 1)
                s = s.Remove(s.Length - 2);
            }
            return s;
        }

        [JsonProperty("customfield_13105")]
        public string accountName = "";

        [JsonProperty("customfield_13106")]
        public string copyNumber = "";

        [JsonProperty("customfield_13501")]
        public string planningRank = "";

        [JsonProperty("customfield_13700")]
        public string ideaID = "";

        [JsonProperty("customfield_13803")]
        public AhaReference ahaReference = new AhaReference();

        [JsonProperty("customfield_13805")]
        public Regression regression = new Regression();

        [JsonProperty("customfield_14701")]
        public string team = "";

        [JsonProperty("fixVersions")]
        public List<FixVersionsItem> fixVersions = new List<FixVersionsItem>();

        public string GetFixVersionsString()
        {
            string s = "";
            if(fixVersions.Count > 0)
            {
                foreach(var f in fixVersions)
                {
                    s += f.name;
                    s += ", ";
                }
                if(s.Length > 1)
                s = s.Remove(s.Length - 2);
            }
            return s;
        }

        [JsonProperty("issuetype")]
        public Issuetype issuetype = new Issuetype();

        [JsonProperty("labels")]
        public List<string> labels = new List<string>();

        public string GetLabelsString()
        {
            string s = "";
            if (labels.Count > 0)
            {
                foreach (var st in labels)
                {
                    s += st;
                    s += ", ";
                }
                if (s.Length > 1)
                    s = s.Remove(s.Length - 2);
            }
            return s;
        }

        [JsonProperty("priority")]
        public Priority priority = new Priority();

        [JsonProperty("project")]
        public Project project = new Project();

        [JsonProperty("reporter")]
        public Reporter reporter = new Reporter();

        [JsonProperty("resolution")]
        public Resolution resolution = new Resolution();


        public string resolutiondate = "";
        // Good time to point out, these names are not customizable. They must match the JSON

        [JsonProperty("status")]
        public Status status = new Status();

        public string summary = "";

        public string updated = "";

        [JsonProperty("versions")]
        public List<VersionsItem> versions = new List<VersionsItem>();

        public string GetVersionsString()
        {
            string s = "";
            if (versions.Count > 0)
            {
                foreach (var v in versions)
                {
                    s += v.name;
                    s += ", ";
                }
                // TODO figure out how s.Length-2 could have possibly given OutOfBounds. Added an extra IF to fix for now
                // even if it had components with blank values, the append of ", " should have always ensured safety 
                if (s.Length > 1)
                    s = s.Remove(s.Length - 2);
            }
            return s;
        }

    }
    
    public class Assignee
    {
        [JsonProperty("name")]
        public string name = "";
        //assignee
        // could be key or displayName instead
        [JsonProperty("displayName")]
        public string displayName = "";
    }

    public class Component
    {
        [JsonProperty("name")]
        public string value = "";
        //Component
        // Array Item
    }

    public class Creator
    {
        [JsonProperty("name")]
        public string name = "";
        // Creator
        // user identifiers
    }
    
    public class Client
    {
        [JsonProperty("value")]
        public string value = "";
        // Client
    }
    public class Origin
    {
        public string value = "";
        //Origin
    }
    public class ProductAndVersion
    {
        public string value = "";
        // Product And Version
    }
    public class RootCause
    {
        public string value = "";
        // Root Cause
    }

    public class SeverityLevel
    {
        public string value = "";
        // Severity Level
        // queried a few thousand issues and I only ever found this field object to be null so I
        // don't know what's in the JSON object but 'value' feels safe for now.
    }
    public class QAAssigned
    {
        public string name = "";
        // QA Assigned
        // user identifiers
    }
    public class LPAssigned
    {
        public string name = "";
        // LP Assigned
        // user identifiers
    }
    public class AEAssigned
    {
        public string name = "";
        // AE Assigned
        // user identifiers
    }
    public class PriorityLevel
    {
        public string name = "";
        // Priority Level
        // queried a few thousand issues and I only ever found this field object to be null so I
        // don't know what's in the JSON object but 'name' feels safe for now because that's what
        // it's called in the Priority objects
        
        // SUGGEST add another variable in Priority class called "name"
    }
    public class SEAssigned
    {
        public string name = "";
        // SE Assigned
        // user identifiers
    }
    public class NeedsDocumentation
    {
        public string value = "";
        // Needs Documentation
        // that's the column/field name. Not a note that I need to document this...
    }
    //public class Customfield_12400
    //{
    //    // Epic Link
    //    // Not a JSON object. Direct Field variable
    //}

    public class CategoryItem
    {
        [JsonProperty("value")]
        public string value = "";
        //Category
        // Array Item
    }
    //public class Customfield_13105
    //{
    //    // Account Name
    //    // Not a JSON object. Direct Field variable
    //}
    //public class Customfield_13106
    //{
    //    // Copy Number
    //    // Not a JSON object. Direct Field Variable
    //}
    //public class Customfield_13501
    //{
    //    // Planning Rank
    //    // Not a JSON object. Direct Field Variable
    //}
    //public class Customfield_13700
    //{
    //    // Idea ID
    //    // Not a JSON object. Direct Field Variable
    //}
    public class AhaReference
    {
        public string value = "";
        //Aha! Reference
        // queried a few thousand issues and I only ever found this field object to be null so I
        // don't know what's in the JSON object
    }
    public class Regression
    {
        public string value = "";
        //Regression
    }
    //public class Customfield_14701
    //{
    //    //Team
    //    //Values are string integers. Team Number for clarity?
    //    // Not a JSON object. Direct Field Variable 
    //}
    public class FixVersionsItem
    {
        [JsonProperty("name")]
        public string name = "";
        // Fix Versions
        // Array Item
    }
    public class Issuetype
    {
        public string name = "";
        // Issue Type
    }
    //public class Labels
    //{
    //    public string value = "";
    //    //Labels
    //    //Array Item
    //    //Labels isn't an array of JSON objects like most of these, it's just an array of strings
    //    //Leaving this here for a little clarity/documentation.
    //}
    public class Priority
    {
        public string id = "";
    }
    public class Project
    {
        public string name = "";
        //Project
        //e.g. APX
    }
    public class Reporter
    {
        public string name = "";
        // Reporter
        // user identifiers
    }
    public class Resolution
    {
        public string name = "";
        //Resolution
    }
    //public class ResolutionDate
    //{
    //    public string name = "";
    //    //Resolved
    //    //Not a JSON object. Direct Field Variable
    //}
    public class Status
    {
        public string name = "";
        //Status
    }
    //public class Summary
    //{
    //    //Summary
    //    //Not a JSON object. Direct Field Variable
        
    //}
    //public class Updated
    //{
    //    //Updated
    //    //Not a JSON object. Direct Field Variable
    //}
    public class VersionsItem
    {
        [JsonProperty("name")]
        public string name = "";
        //Affects Versions
        //Array Item
    }


    
    
}
