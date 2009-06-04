﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;

namespace ThoughtWorks.CruiseControl.Remote
{
    /// <summary>
    /// A client connection to an old (pre-1.5) version of CruiseControl.NET via HTTP.
    /// </summary>
    public class CruiseServerHttpClient
        : CruiseServerClientBase
    {
        #region Private fields
        private readonly string serverUri;
        private string targetServer;
        private WebClient client;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new <see cref="CruiseServerRemotingClient"/>.
        /// </summary>
        /// <param name="serverUri">The address of the server.</param>
        public CruiseServerHttpClient(string serverUri)
            : this(serverUri, new WebClient())
        {
        }

        /// <summary>
        /// Initialises a new <see cref="CruiseServerRemotingClient"/>.
        /// </summary>
        /// <param name="serverUri">The address of the server.</param>
        /// <param name="client">The <see cref="WebClient"/> to use.</param>
        public CruiseServerHttpClient(string serverUri, WebClient client)
        {
            this.serverUri = serverUri.EndsWith("/") ? serverUri.Substring(0, serverUri.Length - 1) : serverUri;
            this.client = client;
        }
        #endregion

        #region Public properties
        #region TargetServer
        /// <summary>
        /// The server that will be targeted by all messages.
        /// </summary>
        public override string TargetServer
        {
            get
            {
                if (string.IsNullOrEmpty(targetServer))
                {
                    var targetUri = new Uri(serverUri);
                    return targetUri.Host;
                }
                else
                {
                    return targetServer;
                }
            }
            set { targetServer = value; }
        }
        #endregion

        #region IsBusy
        /// <summary>
        /// Is this client busy performing an operation.
        /// </summary>
        public override bool IsBusy
        {
            get { return false; }
        }
        #endregion
        #endregion

        #region Public methods
        #region GetProjectStatus()
        /// <summary>
        /// Gets information about the last build status, current activity and project name.
        /// for all projects on a cruise server
        /// </summary>
        public override ProjectStatus[] GetProjectStatus()
        {
            try
            {
                // Retrieve the XML from the server
                var url = GenerateUrl("XmlStatusReport.aspx");
                var response = client.DownloadString(url);
                if (string.IsNullOrEmpty(response)) throw new CommunicationsException("No data retrieved");

                // Load the XML and parse it
                var document = new XmlDocument();
                document.LoadXml(response);
                var projects = ParseProjects(document.SelectNodes("/Projects/Project"));
                return projects.ToArray();
            }
            catch (Exception error)
            {
                throw new CommunicationsException("Unable to retrieve project status from the remote server", error);
            }
        }
        #endregion

        #region ForceBuild()
        /// <summary>
        /// Forces a build for the named project.
        /// </summary>
        /// <param name="projectName">project to force</param>
        public override void ForceBuild(string projectName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Forces a build for the named project with some parameters.
        /// </summary>
        /// <param name="projectName">project to force</param>
        /// <param name="parameters"></param>
        public override void ForceBuild(string projectName, List<NameValuePair> parameters)
        {
            ForceBuild(projectName);
        }
        #endregion

        #region AbortBuild()
        /// <summary>
        /// Attempts to abort a current project build.
        /// </summary>
        /// <param name="projectName">The name of the project to abort.</param>
        public override void AbortBuild(string projectName)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Request()
        /// <summary>
        /// Sends a build request to the server.
        /// </summary>
        /// <param name="projectName">The name of the project to use.</param>
        /// <param name="integrationRequest"></param>
        public override void Request(string projectName, IntegrationRequest integrationRequest)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region StartProject()
        /// <summary>
        /// Attempts to start a project.
        /// </summary>
        /// <param name="project"></param>
        public override void StartProject(string project)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region StopProject()
        /// <summary>
        /// Stop project.
        /// </summary>
        /// <param name="project"></param>
        public override void StopProject(string project)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SendMessage()
        /// <summary>
        /// Sends a message for a project.
        /// </summary>
        /// <param name="projectName">The name of the project to use.</param>
        /// <param name="message"></param>
        public override void SendMessage(string projectName, Message message)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region GetCruiseServerSnapshot()
        /// <summary>
        /// Gets the projects and integration queues snapshot from this server.
        /// </summary>
        public override CruiseServerSnapshot GetCruiseServerSnapshot()
        {
            try
            {
                string response;

                try
                {
                    // Retrieve the XML from the server - 1.3 or later
                    var url = GenerateUrl("XmlServerReport.aspx");
                    response = client.DownloadString(url);
                }
                catch (Exception error)
                {
                    // Retrieve the XML from the server - ealier than 1.3
                    var url = GenerateUrl("XmlStatusReport.aspx");
                    response = client.DownloadString(url);
                }
                if (string.IsNullOrEmpty(response)) throw new CommunicationsException("No data retrieved");

                // Load the XML and parse it
                var document = new XmlDocument();
                document.LoadXml(response);

                var snapshot = new CruiseServerSnapshot();
                if (document.DocumentElement.Name == "projects")
                {
                    snapshot.ProjectStatuses = ParseProjects(document.SelectNodes("/Projects/Project")).ToArray();
                }
                else
                {
                    snapshot.ProjectStatuses = ParseProjects(document.SelectNodes("/CruiseControl/Projects/Project")).ToArray();
                }
                return snapshot;
            }
            catch (Exception error)
            {
                throw new CommunicationsException("Unable to retrieve project status from the remote server", error);
            }
        }
        #endregion
        #endregion

        #region Private methods
        #region GenerateUrl()
        /// <summary>
        /// Generate a URL to the server.
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        private string GenerateUrl(string pageUrl)
        {
            // Assumption - if a URI contains a "." then it already has a page in it
            var lastSlash = serverUri.LastIndexOf('/');
            if (serverUri.IndexOf('.', lastSlash) > 0)
            {
                return serverUri;
            }
            else
            {
                return string.Format("{0}/{1}", serverUri, pageUrl);
            }
        }
        #endregion

        #region ParseProjects()
        /// <summary>
        /// Parse an array of project definitions.
        /// </summary>
        /// <param name="projectNodes"></param>
        /// <returns></returns>
        private List<ProjectStatus> ParseProjects(XmlNodeList projectNodes)
        {
            var projects = new List<ProjectStatus>();

            foreach (XmlElement node in projectNodes)
            {
                var project = new ProjectStatus{
                    Activity = new ProjectActivity(RetrieveAttributeValue(node, "activity", "Unknown")),
                    BuildStage = node.GetAttribute("BuildStage"),
                    BuildStatus = RetrieveAttributeValue(node, "lastBuildStatus", IntegrationStatus.Unknown),
                    Category = node.GetAttribute("category"),
                    LastBuildDate = RetrieveAttributeValue(node, "lastBuildTime", DateTime.MinValue),
                    LastBuildLabel = node.GetAttribute("lastBuildLabel"),
                    Name = node.GetAttribute("name"),
                    NextBuildTime = RetrieveAttributeValue(node, "nextBuildTime", DateTime.MaxValue),
                    ServerName = node.GetAttribute("serverName"),
                    WebURL = node.GetAttribute("webUrl")
                };
                projects.Add(project);
            }

            return projects;
        }
        #endregion

        #region RetrieveAttributeValue()
        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string RetrieveAttributeValue(XmlElement element, string attributeName, string defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }

        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private DateTime RetrieveAttributeValue(XmlElement element, string attributeName, DateTime defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            var dateValue = string.IsNullOrEmpty(value)
                ? defaultValue
                : DateTime.Parse(value);
            return dateValue;
        }

        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private TEnum RetrieveAttributeValue<TEnum>(XmlElement element, string attributeName, TEnum defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            var enumValue = string.IsNullOrEmpty(value)
                ? defaultValue
                : (TEnum)Enum.Parse(typeof(TEnum), value);
            return enumValue;
        }
        #endregion
        #endregion
    }
}