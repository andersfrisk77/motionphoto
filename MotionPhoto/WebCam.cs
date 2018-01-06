using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MotionPhoto
{
    public class WebCam
    {
        private string baseUrl;
        private string login;
        private string password;
        public Action<Stream> NewImage;
        public Action<string> LogMessage;
        public WebCam(string baseUrl, string login, string password)
        {
            this.baseUrl = baseUrl;
            this.login = login;
            this.password = password;
        }
        public async Task Listen()
        {
            using (var handler = new HttpClientHandler())
            {
                handler.Credentials = new System.Net.NetworkCredential(this.login, this.password);
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                    var url = this.baseUrl + "/config/notify_stream.cgi";
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    using (var response = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead))
                    {
                        using (var body = await response.Content.ReadAsStreamAsync())
                        using (var reader = new StreamReader(body))
                        {
                            while (!reader.EndOfStream)
                            {
                                var read = reader.ReadLine();
                                this.LogMessage?.Invoke(read);
                                if (read == "md1=on")
                                {
                                    var now = DateTime.Now.ToString("o");
                                    this.LogMessage?.Invoke($"Found motion change. Timestamp {now}");
                                    Stream data = null;
                                    try
                                    {
                                        data = await this.ReadImage();
                                        this.LogMessage?.Invoke($"Read image success. Timestamp {now}");
                                    }
                                    catch (Exception e)
                                    {
                                        this.LogMessage?.Invoke($"Read image failed. Timestamp {now}. Message {e.Message}");
                                        continue;
                                    }
                                    if(data != null)
                                        this.NewImage?.Invoke(data);
                                    data?.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }
        public async Task<Stream> ReadImage()
        {
            var result = new System.IO.MemoryStream();
            using (var handler = new HttpClientHandler())
            {
                handler.Credentials = new System.Net.NetworkCredential(this.login, this.password);
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                    var url = this.baseUrl + "/image/jpeg.cgi";
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    using (var response = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead))
                    {
                        using (var body = await response.Content.ReadAsStreamAsync())
                        {
                            body.CopyTo(result);
                            result.Position = 0;
                        }
                    }
                }
            }
            return result;
        }
    }
}
