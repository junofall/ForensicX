using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    public static class FatTimeConverter
    {
        public static DateTime ConvertFatDateTimeToDateTime(ushort fatDate, ushort fatTime)
        {
            Debug.WriteLine("Date: " + fatDate);
            Debug.WriteLine("Time: " + fatTime);
            DateTime dateTime = ConvertFatDateToDate(fatDate).Add(ConvertFatTimeToTimeSpan(fatTime));
            return dateTime;
        }
        public static DateTime ConvertFatDateToDate(ushort fatDate)
        {
            int year = ((fatDate & 0xFE00) >> 9) + 1980;
            int month = (fatDate & 0x01FF) >> 5;
            int day = fatDate & 0x001F;

            Debug.WriteLine($"{year} : {month} : {day}");
            DateTime date = new DateTime(year, month, day);
            return date;
        }

        public static TimeSpan ConvertFatTimeToTimeSpan(ushort fatTime)
        {
            int hours = (fatTime & 0xF800) >> 11;
            int minutes = (fatTime & 0x07E0) >> 5;
            int seconds = (fatTime & 0x001F) * 2;

            TimeSpan time = new TimeSpan(hours, minutes, seconds);
            return time;
        }
    }
}
