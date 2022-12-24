using System;
using System.Net;

namespace ConnectFourAI;

public static class Utilities
{
    public static string BytesToString(long i)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (i == 0) return "0" + suf[0];
        long bytes = Math.Abs(i);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(i) * num).ToString() + suf[place];
    }

    public static string MakeGetRequest(string URL)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        request.Method = "GET";
        request.ContentType = "application/x-www-form-urlencoded";
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:10.0.2) Gecko/20100101 Firefox/10.0.2";
        request.Timeout = 10000;

        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}