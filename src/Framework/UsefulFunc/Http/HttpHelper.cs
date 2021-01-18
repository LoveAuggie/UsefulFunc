using Auggie.Lib.Extend;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.Http
{
    public class HttpRequest
    {
        #region 请求配置属性
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public HttpMethod Method { get; set; }
        /// <summary>
        /// 请求头信息
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 请求数据类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 请求Body字符串
        /// </summary>
        public string RequestBodyString { get; set; }
        /// <summary>
        /// x-www-form-urlencode用字典
        /// </summary>
        public Dictionary<string, string> RequestBodyDic { get; set; }
        /// <summary>
        /// form-data用，可以传递文件等信息
        /// </summary>
        public Dictionary<string, FormData> RequestBodyForm { get; set; }
        /// <summary>
        /// 处理好的byte数组
        /// </summary>
        public byte[] RequestBodyBytes { get; set; }
        /// <summary>
        /// 请求编码
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 是否自动跳转
        /// </summary>
        public bool AutoRedirect { get; set; }
        /// <summary>
        /// 请求超时时间（毫秒）
        /// </summary>
        public int TimeOut { get; set; }

        #region 部分重要头信息的额外配置
        public string User_Agent { get { return Headers.Read("User-Agent"); } set { Headers["User-Agent"] = value; } }
        public string Referer { get { return Headers.Read("Referer"); } set { Headers["Referer"] = value; } }
        public bool Connection { get { return Headers.Read("Connection")=="Keep-Alive"; } set { Headers["user-agent"] = value ? "Keep-Alive" : "Close"; } }
        public string Authorization { get { return Headers.Read("Authorization"); } set { Headers["Authorization"] = value; } }
        public CookieCollection Cookies { get; set; }
        public string CookieString { get { return Cookies.GetString(); } set { this.Cookies.SetString(value); } }
        #endregion
        #endregion

        private HttpWebRequest webRequest;
        private HttpWebResponse webResponse;
        private HttpResult httpResult;
        static HttpRequest()
        {
            // 启用https连接
            ServicePointManager.ServerCertificateValidationCallback = HttpsCallBack;
            // 启用所有的https协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        private static bool HttpsCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 执行请求并返回
        /// </summary>
        /// <returns></returns>
        public HttpResult Proc()
        {
            try
            {
                CreateRequest();
                DoProc();
                return this.httpResult;
            }
            catch (Exception ex)
            {
                return new HttpResult() { StatusCode = 500, ErrorMessage = $"InnerError:{ex.Message}"};
            }
        }

        /// <summary>
        /// 执行请求并返回请求内容字符串
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            Proc();
            return this.httpResult.ResHtml;
        }
        /// <summary>
        /// 执行请求并返回二进制数组
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Proc();
            return this.httpResult.ResBytes;
        }
        /// <summary>
        /// 执行请求并将结果保存为文件
        /// </summary>
        /// <param name="fileName"></param>
        public void DownloadFile(string fileName)
        {
            Proc();
            File.WriteAllBytes(fileName, this.httpResult.ResBytes);
        }
        private void CreateRequest()
        {
            this.webRequest= WebRequest.Create(Address) as HttpWebRequest;
            webRequest.Timeout = this.TimeOut;
            webRequest.ReadWriteTimeout = this.TimeOut;
            webRequest.ContinueTimeout = this.TimeOut;
            webRequest.AllowAutoRedirect = this.AutoRedirect;
            webRequest.CookieContainer = new CookieContainer();
            webRequest.Method = this.Method.ToString();
            // 头信息
            if (this.Cookies != null && this.Cookies.Count > 0)
            {
                webRequest.CookieContainer.Add(this.Cookies);
            }
            #region 头信息
            foreach (var kv in this.Headers)
            {
                switch (kv.Key.ToLower())
                {
                    case "user-agent":
                        webRequest.UserAgent = kv.Value;
                        break;
                    case "referer":
                        webRequest.Referer = kv.Value;
                        break;
                    case "date":
                        webRequest.Date = DateTime.Parse(kv.Value);
                        break;
                    case "expert":
                        webRequest.Expect = kv.Value;
                        break;
                    case "host":
                        webRequest.Host = kv.Value;
                        break;
                    case "accept":
                        webRequest.Accept = kv.Value;
                        break;
                    case "connection":
                        webRequest.KeepAlive = kv.Value == "Keep-Alive";
                        break;
                    default:
                        webRequest.Headers[kv.Key] = kv.Value;
                        break;
                }
            }
            #endregion

            #region 请求信息
            if (this.Method == HttpMethod.POST || this.Method == HttpMethod.PUT)
            {
                if (this.Encoding == null) this.Encoding = Encoding.UTF8;

                byte[] rBytes = null;
                if (!string.IsNullOrEmpty(this.RequestBodyString))
                {
                    rBytes = this.Encoding.GetBytes(this.RequestBodyString);
                }
                else if (this.RequestBodyBytes != null)
                {
                    rBytes = this.RequestBodyBytes;
                }
                else if (this.RequestBodyDic != null)
                {
                    this.ContentType = "application/x-www-form-urlencode";
                    rBytes = GetRequestBytes(this.RequestBodyDic);
                }
                else if (this.RequestBodyForm != null)
                {
                    rBytes = GetRequestBytes(this.RequestBodyForm);
                }
                if (rBytes == null)
                    throw new Exception("Post,Put 方法，请指定数据Body！");

                webRequest.ContentType = this.ContentType;
                webRequest.ContentLength = rBytes.Length;
                using (var steam = webRequest.GetRequestStream())
                {
                    steam.Write(rBytes, 0, rBytes.Length);
                }
            }
            #endregion
        }

        private byte[] GetRequestBytes(Dictionary<string, string> requestBodyDic)
        {
            this.RequestBodyString = string.Join("&", requestBodyDic.Select(t => $"{t.Key}={System.Web.HttpUtility.UrlEncode(t.Value)}");

            return this.Encoding.GetBytes(this.RequestBodyString);
        }

        private byte[] GetRequestBytes(Dictionary<string, FormData> requestBodyForm)
        {
            var memStream = new MemoryStream();
            // 边界符
            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            // 边界符
            var beginBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            // 最后的结束符
            var endBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            // 设置属性
            this.ContentType = "multipart/form-data; boundary=" + boundary;

            Dictionary<string, string> strDic = requestBodyForm.Where(t => !t.Value.isFile).ToDictionary(k => k.Key, v => v.Value.value);

            if (strDic != null && strDic.Count > 0)
            {
                // 写入字符串的Key
                var stringKeyHeader = "\r\n--" + boundary +
                                       "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                       "\r\n\r\n{1}\r\n";

                foreach (byte[] formitembytes in from string key in strDic.Keys
                                                 select string.Format(stringKeyHeader, key, strDic[key])
                                                     into formitem
                                                 select Encoding.UTF8.GetBytes(formitem))
                {
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }
            }

            var fileDic = requestBodyForm.Where(t => t.Value.isFile).ToDictionary(k => k.Key, v => v.Value.value);

            if (fileDic != null && fileDic.Count > 0)
            {
                foreach (var file in fileDic)
                {
                    // 写入文件
                    string filePartHeader = "\r\n--" + boundary +
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                         "Content-Type: application/octet-stream\r\n\r\n";
                    var header = string.Format(filePartHeader, file.Key, file.Value);
                    var headerbytes = Encoding.UTF8.GetBytes(header);

                    memStream.Write(beginBoundary, 0, beginBoundary.Length);
                    memStream.Write(headerbytes, 0, headerbytes.Length);

                    var buffer = new byte[1024];
                    int bytesRead; // =0

                    var fileStream = new FileStream(file.Value, FileMode.Open, FileAccess.Read);
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }
            }

            // 写入最后的结束边界符
            memStream.Write(endBoundary, 0, endBoundary.Length);

            webRequest.ContentLength = memStream.Length;

            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            return tempBuffer;
        }

        private void DoProc()
        {
            webResponse = webRequest.GetResponse() as HttpWebResponse;

            this.httpResult = new HttpResult();
            httpResult.Request = this;


            // 头信息
            httpResult.StatusCode = (int)webResponse.StatusCode;
            if (httpResult.Headers != null)
            {
                foreach (var key in webResponse.Headers.AllKeys)
                {
                    httpResult.Headers[key] = webResponse.Headers[key];
                }
            }
            using (var ms = new MemoryStream())
            {
                using (var stream = webResponse.GetResponseStream())
                {
                    Stream readStem = stream;
                    if (webResponse.ContentType.Contains("gzip"))
                    {
                        readStem = new GZipStream(stream, CompressionMode.Decompress);
                    }
                    else if (webResponse.ContentType.Contains("deflate"))
                    {
                        readStem = new DeflateStream(stream, CompressionMode.Decompress);
                    }

                    using (var gzstream = new GZipStream(readStem, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        int bRead = 0;
                        do
                        {
                            bRead = gzstream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, bRead);
                        } while (bRead > 0);
                    }
                    readStem.Dispose();
                }

                httpResult.ResBytes = ms.ToArray();
                httpResult.ResHtml = this.Encoding.GetString(httpResult.ResBytes);
            }
        }
    }

    public class HttpResult
    {
        /// <summary>
        /// 原请求信息
        /// </summary>
        public HttpRequest Request { get; set; }
        /// <summary>
        /// 请求返回数据字符串
        /// </summary>
        public string ResHtml { get; set; }
        /// <summary>
        /// 请求返回二进制数组
        /// </summary>
        public byte[] ResBytes { get; set; }
        /// <summary>
        /// 请求返回状态码
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 请求出错时的错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 请求返回Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// 请求返回头信息
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }

    public class FormData
    {
        public string value;

        public bool isFile;
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
