/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Configuration;

using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Services.DocumentService;
using Newtonsoft.Json.Linq;

namespace ASC.Api.Settings
{
    [DataContract(Name = "buildversion", Namespace = "")]
    public class BuildVersion
    {
        [DataMember]
        public string CommunityServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DocumentServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MailServer { get; set; }

        public static BuildVersion GetCurrentBuildVersion()
        {
            return new BuildVersion
                {
                    CommunityServer = GetCommunityVersion(),
                    DocumentServer = GetDocumentVersion(),
                    MailServer = GetMailVersion()
                };
        }

        private static string GetCommunityVersion()
        {
            return WebConfigurationManager.AppSettings["version.number"] ?? "8.5.0";
        }

        private static string GetDocumentVersion()
        {
            if (string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl))
                return null;

            return DocumentServiceConnector.GetVersion();
        }

        private static string GetMailVersion()
        {
            String url = null;

            // GetTenantServer() throw System.IO.InvalidDataException if no mail servers registered.
            try
            {
                var serverDal = new Mail.Server.Dal.ServerDal(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                url = serverDal.GetTenantServer().ApiVersionUrl;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof (SettingsApi)).Warn(e.Message, e);
            }

            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                using (var client = new WebClient())
                {
                    var response = Encoding.UTF8.GetString(client.DownloadData(url));
                    return JObject.Parse(response)["global_vars"]["value"].ToString();
                }
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(SettingsApi)).Error(e.Message, e);
            }

            return null;
        }
    }
}
