﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectCmdlet.cs" company="The CruiseControl.NET Team">
//   Copyright (C) 2011 by The CruiseControl.NET Team
// 
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ThoughtWorks.CruiseControl.PowerShell.Cmdlets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Retrieves a log.
    /// </summary>
    [Cmdlet("Get", "Log", DefaultParameterSetName = "PathSet")]
    public class GetLog
        : PSCmdlet
    {
        #region Public properties
        #region Path
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [Parameter(ParameterSetName = "PathSet", Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }
        #endregion

        #region Project
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        [Parameter(ParameterSetName = "ProjectSet", Mandatory = true, Position = 1, ValueFromPipeline = true)]
        [ValidateNotNull]
        public Project Project { get; set; }
        #endregion

        #region Server
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [Parameter(ParameterSetName = "ServerSet", Mandatory = true, Position = 1, ValueFromPipeline = true)]
        [ValidateNotNull]
        public ServerFolder Server { get; set; }
        #endregion
        #endregion

        #region Protected methods
        #region
        /// <summary>
        /// Processes a record.
        /// </summary>
        protected override void ProcessRecord()
        {
            var logExposers = new List<IExposeLog>();
            if (this.Path != null)
            {
                ProviderInfo info;
                var paths = this.GetResolvedProviderPathFromPSPath(this.Path, out info);
                if (info.ModuleName != typeof(ClientCmdletProvider).Namespace)
                {
                    var record = new ErrorRecord(
                        new Exception("Invalid provider"),
                        "Validation",
                        ErrorCategory.InvalidArgument,
                        this.Path);
                    this.WriteError(record);
                }

                var driveName = this.Path.Substring(0, this.Path.IndexOf(':'));
                var drive = info.Drives.First(d => d.Name.Equals(driveName, StringComparison.InvariantCultureIgnoreCase));
                var clientDrive = drive as ClientDriveInfo;
                var statuses = clientDrive.Client.GetProjectStatus();
                foreach (var path in paths)
                {
                    var projectName = path.Substring(path.LastIndexOf('\\') + 1);
                    if (string.IsNullOrEmpty(projectName))
                    {
                        logExposers.Add(clientDrive.RootFolder);
                    }
                    else
                    {
                        var status = statuses
                            .FirstOrDefault(s => s.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
                        if (status == null)
                        {
                            var record = new ErrorRecord(
                                new Exception("Invalid project"),
                                "Validation",
                                ErrorCategory.InvalidArgument,
                                this.Path);
                            this.WriteError(record);
                        }

                        logExposers.Add(Project.Wrap(clientDrive.Client, status));
                    }
                }
            }
            else if (this.Server != null)
            {
                logExposers.Add(this.Server);
            }
            else
            {
                logExposers.Add(this.Project);
            }

            foreach (var logExposer in logExposers)
            {
                var log = logExposer.GetLog();
                var lastPos = 0;
                var pos = log.IndexOf(Environment.NewLine);
                string line;
                while (pos >= 0)
                {
                    line = log.Substring(lastPos, pos - lastPos);
                    this.WriteObject(line);
                    lastPos = pos + 2;
                    pos = log.IndexOf(Environment.NewLine, lastPos);
                }

                line = log.Substring(lastPos);
                if (!string.IsNullOrEmpty(line))
                {
                    this.WriteObject(line);
                }
            }
        }
        #endregion
        #endregion
    }
}
