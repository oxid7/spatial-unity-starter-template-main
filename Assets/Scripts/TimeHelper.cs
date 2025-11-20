using System;
using System.Globalization;

public static class TimeHelper
{
    private const int DubaiOffsetHours = 4;   // UTC+4

    public static string GetCurrentDubaiTimestamp()
    {
        DateTime dubaiTime = DateTime.UtcNow.AddHours(DubaiOffsetHours);
        return dubaiTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
