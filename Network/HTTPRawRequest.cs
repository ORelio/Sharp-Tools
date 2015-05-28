using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;

namespace SharpTools
{
    /// <summary>
    /// Utility class for performing raw HTTP requests
    /// HTTP Implementation from scratch built directly upon TcpClient.
    /// By ORelio - (c) 2014-2015 - Available under the CDDL-1.0 license
    /// </summary>
    public static class HTTPRawRequest
    {
        /// <summary>
        /// Default port for a HTTP request
        /// </summary>
        public const int HTTP = 80;

        /// <summary>
        /// Default port for a HTTPS request
        /// </summary>
        public const int HTTPS = 443;

        /// <summary>
        /// Get a random user agent for making requests
        /// </summary>
        /// <returns>A valid user agent string from Firefox, IE, Chrome or Opera</returns>
        public static string GetUserAgent()
        {
            string[] userAgents = new string[]
            {
                "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko", //IE 11 Windows 8.1
                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)", //IE 9 Windows 7
                "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; SLCC1; .NET CLR 1.1.4322)", //IE 8 WinXP
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36", //Chrome 31 Windows 7
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.2 (KHTML, like Gecko) Chrome/22.0.1216.0 Safari/537.2", //Chrome 22 Windows 7
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36", //Chrome 38 Windows 8.1
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.125 Safari/537.36", //Chrome 36 Windows 7
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36 OPR/25.0.1614.63", //Opera 25 Windows 8.1
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.13 Safari/537.36", //Chrome 39 Mac OS
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_4) AppleWebKit/537.77.4 (KHTML, like Gecko) Version/7.0.5 Safari/537.77.4", //Safari 7 Mac OS
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_5) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1", //Safari 6 Mac OS
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv:32.0) Gecko/20100101 Firefox/32.0", //FF 32 Mac OS
                "Mozilla/5.0 (X11; Ubuntu; Linux i686; rv:32.0) Gecko/20100101 Firefox/32.0", //FF 32 Ubuntu
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:28.0) Gecko/20100101 Firefox/28.0", //FF 28 Windows 7
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0" //FF 32 Windows 7
            };

            return userAgents[new Random().Next(0, userAgents.Length)];
        }

        /// <summary>
        /// Get a set of headers for an HTTP POST request
        /// </summary>
        /// <param name="formData">Data of the submitted form</param>
        /// <param name="host">Target host</param>
        /// <param name="resourceUrl">Requested resource</param>
        /// <param name="userAgent">User agent of the request eg firefox</param>
        /// <param name="referrer">Referrer of the request</param>
        /// <param name="cookies">Cookies for the request</param>
        /// <param name="postDataResult">Will contain encoded data to submit with request</param>
        /// <returns>HTTP Headers</returns>
        public static List<string> GetPOSTHeaders(
            IEnumerable<KeyValuePair<string, string>> formData, ref byte[] postDataResult, string host,
            string resourceUrl, string referrer = null, string userAgent = null,
            IEnumerable<KeyValuePair<string, string>> cookies = null)
        {
            List<string> headers = GetGETHeaders(host, resourceUrl, referrer, userAgent, cookies);
            headers[0] = String.Format("POST {0} HTTP/1.1", resourceUrl);
            headers.Add("Content-Type: application/x-www-form-urlencoded");

            List<string> tmpFormData = new List<string>();
            foreach (var field in formData)
                tmpFormData.Add(WebUtility.HtmlEncode(field.Key + '=' + field.Value));
            string formDataBuilt = String.Join("&", tmpFormData.ToArray());
            postDataResult = Encoding.ASCII.GetBytes(formDataBuilt);

            headers.Add("Content-Length: " + postDataResult.Length);

            return headers;
        }

        /// <summary>
        /// Get a set of headers for an HTTP GET request
        /// </summary>
        /// <param name="host">Target host</param>
        /// <param name="resourceUrl">Requested resource</param>
        /// <param name="userAgent">User agent of the request eg firefox</param>
        /// <param name="referrer">Referrer of the request</param>
        /// <param name="cookies">Cookies for the request</param>
        /// <returns>HTTP Headers</returns>
        public static List<string> GetGETHeaders(string host,
            string resourceUrl, string referrer = null, string userAgent = null,
            IEnumerable<KeyValuePair<string, string>> cookies = null)
        {
            List<string> headers = new List<string>();

            if (userAgent == null)
                userAgent = GetUserAgent();

            headers.Add(String.Format("GET {0} HTTP/1.1", resourceUrl));
            headers.Add("Host: " + host);
            headers.Add("User-Agent: " + userAgent);
            headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            headers.Add("Accept-Language: fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            headers.Add("Accept-Encoding: gzip, deflate");

            if (referrer != null)
                headers.Add("Referer: " + referrer);

            if (cookies != null && cookies.Count() > 0)
            {
                List<string> tmpCookieData = new List<string>();
                foreach (var field in cookies)
                    tmpCookieData.Add(WebUtility.HtmlEncode(field.Key + '=' + field.Value));
                string cookieDataBuilt = String.Join("; ", tmpCookieData.ToArray());
                headers.Add("Cookie: " + cookieDataBuilt);
            }

            headers.Add("Connection: keep-alive");

            return headers;
        }

        /// <summary>
        /// Do an HTTP request with the provided custom HTTP readers
        /// </summary>
        /// <param name="host">Host to connect to (eg my.site.com)</param>
        /// <param name="port">Port to connect to (usually port 80)</param>
        /// <param name="requestHeaders">HTTP headers</param>
        /// <param name="requestBody">Request body, eg in case of POST request</param>
        /// <returns>Request result, as a byte array, already un-gzipped if compressed</returns>
        public static HTTPRequestResult DoRequest(string host, IEnumerable<string> requestHeaders, int port = 80, byte[] requestBody = null)
        {
            //Connect to remote host
            TcpClient client = new TcpClient(host, port);
            Stream stream = client.GetStream();

            //Using HTTPS ?
            if (port == HTTPS)
            {
                //Authenticate Host
                SslStream ssl = new SslStream(client.GetStream());
                ssl.AuthenticateAsClient(host);
                stream = ssl;

                //Build and send headers
                ssl.Write(Encoding.ASCII.GetBytes(String.Join("\r\n", requestHeaders.ToArray()) + "\r\n\r\n"));

                //Send body if there is a body to send
                if (requestBody != null)
                {
                    ssl.Write(requestBody);
                    ssl.Flush();
                }
            }
            else //HTTP
            {
                //Build and send headers
                client.Client.Send(Encoding.ASCII.GetBytes(String.Join("\r\n", requestHeaders.ToArray()) + "\r\n\r\n"));

                //Send body if there is a body to send
                if (requestBody != null) { client.Client.Send(requestBody); }
            }

            //Read response headers
            string statusLine = Encoding.ASCII.GetString(ReadLine(stream));
            if (statusLine.StartsWith("HTTP/1.1"))
            {
                int responseStatusCode = 0;
                string[] statusSplitted = statusLine.Split(' ');
                if (statusSplitted.Length == 3 && int.TryParse(statusSplitted[1], out responseStatusCode))
                {
                    //Response is a valid HTTP response, read all headers
                    List<string> responseHeaders = new List<string>();
                    responseHeaders.Add(statusLine);
                    byte[] responseBody = null;
                    string line = "";
                    do
                    {
                        line = Encoding.ASCII.GetString(ReadLine(stream));
                        responseHeaders.Add(line);
                    } while (line.Length > 0);

                    //Read response length
                    int responseLength = -1;
                    try
                    {
                        string lengthStr = responseHeaders.First(str => str.StartsWith("Content-Length: ")).Substring(16);
                        if (!String.IsNullOrEmpty(lengthStr)) { responseLength = int.Parse(lengthStr); }
                    }
                    catch { }

                    //Then, read response body
                    if (responseHeaders.Contains("Transfer-Encoding: chunked"))
                    {
                        //Chunked data in several sends
                        List<byte> responseBuffer = new List<byte>();
                        int chunkLength = 0;
                        do
                        {
                            //Read all data chunk by chunk, first line is length, second line is data
                            string headerLine = Encoding.ASCII.GetString(ReadLine(stream));
                            bool lengthConverted = true;
                            try { chunkLength = Convert.ToInt32(headerLine, 16); }
                            catch (FormatException) { lengthConverted = false; }
                            if (lengthConverted)
                            {
                                if (chunkLength > 0)
                                {
                                    byte[] chunkContent = ReadLine(stream);
                                    responseBuffer.AddRange(chunkContent);
                                }
                            }
                            else
                            {
                                //Bad chunk length, invalid response
                                return new HTTPRequestResult()
                                {
                                    Status = (HttpStatusCode)502,
                                    Headers = responseHeaders,
                                    Body = null
                                };
                            }
                            //Last chunk is empty
                        } while (chunkLength > 0);
                        responseBody = responseBuffer.ToArray();
                    }
                    else if (responseLength > -1)
                    {
                        //Full data in one send
                        int receivedLength = 0;
                        byte[] received = new byte[responseLength];
                        while (receivedLength < responseLength)
                            receivedLength += stream.Read(received, receivedLength, responseLength - receivedLength);
                        responseBody = received;
                    }
                    else if (!responseHeaders.Contains("Connection: keep-alive"))
                    {
                        //Connection close, full read is possible
                        responseBody = ReadFully(stream);
                    }
                    else
                    {
                        //Cannot handle keep-alive without content length.
                        return new HTTPRequestResult()
                        {
                            Status = (HttpStatusCode)417,
                            Headers = null,
                            Body = null
                        };
                    }

                    //Decompress gzipped data if necessary
                    if (responseHeaders.Contains("Content-Encoding: gzip"))
                    {
                        MemoryStream inputStream = new MemoryStream(responseBody, false);
                        GZipStream decomp = new GZipStream(inputStream, CompressionMode.Decompress);
                        byte[] decompressed = ReadFully(decomp);
                        responseBody = decompressed;
                    }

                    //Finally, return the result :)
                    return new HTTPRequestResult()
                    {
                        Status = (HttpStatusCode)responseStatusCode,
                        Headers = responseHeaders,
                        Body = responseBody
                    };
                }
            }

            //Invalid response, service is anavailable
            return new HTTPRequestResult()
            {
                Status = (HttpStatusCode)503,
                Headers = null,
                Body = null
            };
        }

        /// <summary>
        /// Read all the data from a stream to a byte array
        /// </summary>
        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Read a line, \r\n ended, char by char from a stream, without consuming more bytes.
        /// </summary>
        /// <param name="input">input stream</param>
        /// <returns>the line as a byte array</returns>
        private static byte[] ReadLine(Stream input)
        {
            List<byte> result = new List<byte>();
            int b = 0x00;
            while (true)
            {
                b = input.ReadByte();
                if (b == '\r')
                {
                    int c = input.ReadByte();
                    if (c == '\n')
                    {
                        break;
                    }
                    else
                    {
                        result.Add((byte)b);
                        result.Add((byte)c);
                    }
                }
                else result.Add((byte)b);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// Represents an HTTP request result
    /// </summary>
    public class HTTPRequestResult
    {
        /// <summary>
        /// HTTP status code of the response
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// HTTP headers of the response
        /// </summary>
        public IEnumerable<string> Headers { get; set; }

        /// <summary>
        /// Body of the response, as byte array
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// Quick check of response status
        /// </summary>
        public bool Successfull
        {
            get
            {
                return Headers != null && Status == HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Quick check if response has been received
        /// </summary>
        public bool HasResponded
        {
            get
            {
                return Headers != null && Body != null;
            }
        }

        /// <summary>
        /// Get response body as string
        /// </summary>
        public string BodyAsString
        {
            get
            {
                return Encoding.UTF8.GetString(Body);
            }
        }

        /// <summary>
        /// Get cookies that server sent along with the response
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> NewCookies
        {
            get
            {
                var cookies = new List<KeyValuePair<string, string>>();
                foreach (string header in Headers)
                {
                    if (header.StartsWith("Set-Cookie: "))
                    {
                        string[] headerSplitted = header.Split(' ');
                        if (headerSplitted.Length > 1)
                        {
                            string[] cookieSplitted = headerSplitted[1].Split(';');
                            foreach (string cookie in cookieSplitted)
                            {
                                string[] keyValue = cookie.Split('=');
                                if (keyValue.Length == 2)
                                {
                                    cookies.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                                }
                            }
                        }
                    }
                }
                return cookies;
            }
        }
    }
}
