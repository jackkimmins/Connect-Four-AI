using System;

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
}