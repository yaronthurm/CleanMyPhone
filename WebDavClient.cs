using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CleanMyPhone
{
    public class WebDavClient
    {
        private static readonly HttpMethod PropFind = new HttpMethod("PROPFIND");
        private const int HttpStatusCode_MultiStatus = 207;

        private const string PropFindRequestContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
            "<propfind xmlns=\"DAV:\">" +
            "  <propname/>" +
            "  <prop>" +
            "    <creationdate/>" +
            "    <getlastmodified/>" +
            "    <displayname/>" +
            "    <getcontentlength/>" +
            "    <getcontenttype/>" +
            "    <resourcetype/>" +
            "  </prop> " +
            "</propfind>";

        
        private readonly HttpClient _client;
                                       
        public string Server { get; set; }

        public int Port { get; set; }



        public WebDavClient(NetworkCredential credential = null)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (credential != null)
            {
                handler.Credentials = credential;
                handler.PreAuthenticate = true;
            }

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.ExpectContinue = false;
            _client.Timeout = TimeSpan.FromMinutes(3);
        }


        public async Task<TryGetResult<IEnumerable<Item>>> TryListItems(string path)
        {
            var req = new HttpRequestMessage(PropFind, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");
            req.Headers.Add("Depth", "1");
            req.Content = new StringContent(PropFindRequestContent, Encoding.UTF8, "text/xml");

            HttpResponseMessage res = await _client.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.NotFound)
                return new TryGetResult<IEnumerable<Item>> { Found = false };
            else if (res.StatusCode == HttpStatusCode.OK || (int)res.StatusCode == HttpStatusCode_MultiStatus)
            {
                var ret = ParseResponse(body);
                return new TryGetResult<IEnumerable<Item>> { Found = true, Value = ret };
            }
            else
                throw new Exception($"Failed retrieving items from '{path}'. Status: {(int)res.StatusCode} Res: {body}");
        }

        public async Task<IEnumerable<Item>> ListItems(string path)
        {
            var tryGetResult = await TryListItems(path);
            if (tryGetResult.Found)
                return tryGetResult.Value;
            throw new Exception($"Path '{path}' was not found");
        }

        public async Task<TryGetResult<Item>> TryGetItem(string path)
        {
            var req = new HttpRequestMessage(PropFind, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");
            req.Headers.Add("Depth", "0");
            req.Content = new StringContent(PropFindRequestContent, Encoding.UTF8, "text/xml");

            HttpResponseMessage res = await _client.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.NotFound)
                return new TryGetResult<Item> { Found = false };
            else if (res.StatusCode == HttpStatusCode.OK || (int)res.StatusCode == HttpStatusCode_MultiStatus)
            {
                var ret = ParseResponse(body).FirstOrDefault();
                return new TryGetResult<Item> { Found = true, Value = ret };
            }
            else
                throw new Exception($"Failed retrieving item {path}. Status: {(int)res.StatusCode} Res: {body}");
        }

        public async Task<Item> GetItem(string path)
        {
            var tryGetResult = await TryGetItem(path);
            if (tryGetResult.Found)
                return tryGetResult.Value;
            throw new Exception($"Item {path} was not found");
        }

        public async Task<string> GetContentAsString(string path)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");
            req.Headers.Add("translate", "f");

            HttpResponseMessage res = await _client.SendAsync(req);

            if (res.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed retrieving file.");

            var ret = await res.Content.ReadAsStringAsync();
            return ret;
        }

        public async Task<Stream> GetContentAsStream(string path)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");
            req.Headers.Add("translate", "f");

            HttpResponseMessage res = await _client.SendAsync(req);

            if (res.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed retrieving file.");

            return await res.Content.ReadAsStreamAsync();
        }

        public async Task Delete(string path)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");

            HttpResponseMessage res = await _client.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (res.StatusCode != HttpStatusCode.OK && res.StatusCode != HttpStatusCode.NoContent)
                throw new Exception($"Failed deleting path {path}");
        }

        public async Task CreateDir(string path)
        {
            var req = new HttpRequestMessage(new HttpMethod("MKCOL"), $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Headers.Connection.Add("Keep-Alive");

            HttpResponseMessage res = await _client.SendAsync(req);

            if (res.StatusCode != HttpStatusCode.OK &&
                res.StatusCode != HttpStatusCode.Created &&
                res.StatusCode != HttpStatusCode.NoContent)
                throw new Exception("Failed creating directory.");
        }

        public async Task CreateOrUpdateFile(string path, string content)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, $"http://{Server}:{Port}/{path.TrimStart('/')}");
            req.Content = new StringContent(content);
            req.Headers.Connection.Add("Keep-Alive");

            HttpResponseMessage res = await _client.SendAsync(req);

            if (res.StatusCode != HttpStatusCode.OK &&
                res.StatusCode != HttpStatusCode.Created &&
                res.StatusCode != HttpStatusCode.NoContent)
                throw new Exception("Failed uploading content.");
        }

        public static IEnumerable<Item> ParseResponse(string xmlBody)
        {
            /*
            <?xml version="1.0" encoding="UTF-8"?>
            <d:multistatus xmlns:d="DAV:" xmlns:cal="urn:ietf:params:xml:ns:caldav" xmlns:card="urn:ietf:params:xml:ns:carddav" xmlns:cs="http://calendarserver.org/ns/">
               <d:response>
                  <d:href>/DCIM/</d:href>
                  <d:propstat>
                     <d:prop>
                        <d:resourcetype>
                           <d:collection />
                        </d:resourcetype>
                        <d:getlastmodified>Sat, 14 Jan 2017 07:46:28 GMT+00:00</d:getlastmodified>
                        <d:getcontentlength />
                        <d:getcontenttype>text/html</d:getcontenttype>
                        <d:displayname>DCIM</d:displayname>
                        <d:creationdate />
                     </d:prop>
                     <d:status>HTTP/1.1 200 OK</d:status>
                  </d:propstat>
               </d:response>
               <d:response>
                  <d:href>/DCIM/Camera/</d:href>
                  <d:propstat>
                     <d:prop>
                        <d:resourcetype>
                           <d:collection />
                        </d:resourcetype>
                        <d:getlastmodified>Fri, 20 Oct 2017 11:02:07 GMT+00:00</d:getlastmodified>
                        <d:getcontentlength />
                        <d:getcontenttype>text/html</d:getcontenttype>
                        <d:displayname>Camera</d:displayname>
                        <d:creationdate />
                     </d:prop>
                     <d:status>HTTP/1.1 200 OK</d:status>
                  </d:propstat>
               </d:response>
               <d:response>
                  <d:href>/DCIM/.thumbnails/</d:href>
                  <d:propstat>
                     <d:prop>
                        <d:resourcetype>
                           <d:collection />
                        </d:resourcetype>
                        <d:getlastmodified>Fri, 20 Oct 2017 11:07:42 GMT+00:00</d:getlastmodified>
                        <d:getcontentlength />
                        <d:getcontenttype>text/html</d:getcontenttype>
                        <d:displayname>.thumbnails</d:displayname>
                        <d:creationdate />
                     </d:prop>
                     <d:status>HTTP/1.1 200 OK</d:status>
                  </d:propstat>
               </d:response>
               <d:response>
                  <d:href>/DCIM/Restored/</d:href>
                  <d:propstat>
                     <d:prop>
                        <d:resourcetype>
                           <d:collection />
                        </d:resourcetype>
                        <d:getlastmodified>Sat, 14 Jan 2017 07:56:59 GMT+00:00</d:getlastmodified>
                        <d:getcontentlength />
                        <d:getcontenttype>text/html</d:getcontenttype>
                        <d:displayname>Restored</d:displayname>
                        <d:creationdate />
                     </d:prop>
                     <d:status>HTTP/1.1 200 OK</d:status>
                  </d:propstat>
               </d:response>
            </d:multistatus>
            */
            XDocument doc = XDocument.Load(new StringReader(xmlBody));
            var ret = doc.Root.ElementsByLocalName("response")
                .Select(x => new
                {
                    href = x.ValueOfFirstElement("href"),
                    prop = x.ElementsByLocalName("propstat").First().ElementsByLocalName("prop").First(),
                    status = x.ElementsByLocalName("propstat").First().ValueOfFirstElement("status"),
                })
                .Select(x => new Item
                {
                    Href = x.href,
                    Status = x.status,
                    LastModified = x.prop.ValueOfFirstElement("getlastmodified"),
                    ContentLength = ToLongNullable(x.prop.ValueOfFirstElement("getcontentlength")),
                    ContentType = x.prop.ValueOfFirstElement("getcontenttype"),
                    DisplayName = x.prop.ValueOfFirstElement("displayname"),
                    CreationDate = x.prop.ValueOfFirstElement("creationdate"),
                    IsCollection = x.prop.ElementsByLocalName("resourcetype").First().ElementsByLocalName("collection").Any()
                })
                .ToArray();

            return ret;
        }

        private static long? ToLongNullable(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return long.Parse(value);
        }
    }


    public class TryGetResult<T>
    {
        public T Value;
        public bool Found;
    }

    public static class XML_Extentions
    {
        public static IEnumerable<XElement> ElementsByLocalName(this XElement currentElement, string name)
        {
            return currentElement.Elements().Where(x => x.Name.LocalName == name);
        }

    public static string ValueOfFirstElement(this XElement currentElement, string name)
    {
        return currentElement.ElementsByLocalName(name).First().Value;
    }

    public static string GetAttributeValue(this XElement currentElement, string name)
        {
            return currentElement.Attribute(name)?.Value;
        }

        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, Func<T, IComparable> s)
        {
            var groups = source.GroupBy(s);
            foreach (var group in groups)
                yield return group.First();
        }

        public static string ClearForTSV(this string input)
        {
            return input.Replace("\t", " ")?.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ")
                .Replace("<", "").Replace(">", "").Replace("=", "").Replace("\"", "");
        }
    }


    public class Item
    {
        public long? ContentLength;
        public string ContentType;
        public string CreationDate;
        public string DisplayName;
        public string Href;
        public bool IsCollection;
        public string LastModified;
        internal string Status;
    }
}