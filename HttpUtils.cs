using System;
using System.IO;
using System.Net;
using System.Text;

namespace NeoModLoader.AutoUpdate;

/// <summary>
/// This class is made as utility to make http request easier. Maybe not, just for myself --inmny.
/// </summary>
public static class HttpUtils
{
    public static string Request(string url, string param = "", string method = "get")
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS1.2=3702

        string result = "";
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        if (req == null) return result;
        req.Method = method;
        req.ContentType = @"application/octet-stream";
        req.UserAgent =
            @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
        byte[] post_data = Encoding.GetEncoding("UTF-8").GetBytes(param);
        HttpWebResponse res;
        if (post_data.Length > 0)
        {
            req.ContentLength = post_data.Length;
            req.Timeout = 15000;
            Stream output_stream = req.GetRequestStream();
            output_stream.WriteAsync(post_data, 0, post_data.Length);
            output_stream.FlushAsync();
            output_stream.Close();
            try
            {
                res = (HttpWebResponse)req.GetResponse();
                Stream input_stream = res.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader sr = new(input_stream, encoding);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
        }
        else
        {
            try
            {
                res = (HttpWebResponse)req.GetResponse();
                Stream input_stream = res.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader sr = new(input_stream, encoding);
                result = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
        }

        return result;
    }
}