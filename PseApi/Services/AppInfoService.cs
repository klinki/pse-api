using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PseApi.Services
{
    public class AppInfoService
    {
        public string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public DateTime BuildDate
        {
            get
            {
                var attribute = Assembly.GetExecutingAssembly().GetCustomAttributes(false).First(x => x.GetType().Name == "TimestampAttribute");

                string timestamp = (string)attribute.GetType()
                    .GetProperty("Timestamp")
                    .GetValue(attribute);

                return DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            }
        }
    }
}
