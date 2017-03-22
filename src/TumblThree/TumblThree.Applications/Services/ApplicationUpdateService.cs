﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Waf.Applications;
using System.Xml.Linq;
using TumblThree.Domain;

namespace TumblThree.Applications.Services
{
    /// <summary>
    /// </summary>
    [Export(typeof(IApplicationUpdateService))]
    public class ApplicationUpdateService : IApplicationUpdateService
    {
        private string downloadLink;
        private string version;

        [ImportingConstructor]
        public ApplicationUpdateService()
        {
        }

        public string GetLatestReleaseFromServer()
        {
            version = null;
            downloadLink = null;
            try
            {
                var request =
                    WebRequest.Create(new Uri("https://api.github.com/repos/johanneszab/tumblthree/releases/latest"))
                        as HttpWebRequest;
                request.Method = "GET";
                request.ProtocolVersion = HttpVersion.Version11;
                request.ContentType = "application/json";
                request.ServicePoint.Expect100Continue = false;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.UserAgent = ApplicationInfo.ProductName;
                request.KeepAlive = false;
                string result;
                using (var resp = request.GetResponse() as HttpWebResponse)
                {
                    var reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
                System.Web.Script.Serialization.JavaScriptSerializer jsonDeserializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                jsonDeserializer.MaxJsonLength = 2147483644;
                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(result), new System.Xml.XmlDictionaryReaderQuotas());
                var root = XElement.Load(jsonReader);
                version = root.Element("tag_name").Value;
                downloadLink = root.Element("assets").Element("item").Element("browser_download_url").Value;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
                return exception.Message;
            }
            return null;
        }

        public bool IsNewVersionAvailable()
        {
            try
            {
                var newVersion = new Version(version.Substring(1));
                if (newVersion > new Version(ApplicationInfo.Version))
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return false;
        }

        public string GetNewAvailableVersion()
        {
            return version;
        }

        public Uri GetDownloadUri()
        {
            if (downloadLink == null)
            {
                return null;
            }
            return new Uri(downloadLink);
        }
    }
}
