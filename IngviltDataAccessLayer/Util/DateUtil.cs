using Ingvilt.Core;
using System;

namespace Ingvilt.Util {
    public partial class DateUtil {
        private static readonly LoggingService LOGGING_SERVICE;

        static DateUtil() {
            LOGGING_SERVICE = DependencyInjectionContainer.Container.Resolve<LoggingService>();
        }

        public static string FormatDateWithoutTime(DateTime date) {
            return FormatDate(date, "MMMM dd, yyyy");
        }

        public static string FormatDate(DateTime date) {
            return FormatDate(date, "MMMM dd, yyyy h:mm tt");
        }

        public static string FormatDate(DateTime date, string format) {
            try {
                return date.ToLocalTime().ToString(format);
            } catch (FormatException exception) {
                LOGGING_SERVICE.Error($"Exception when formatting date: {format} for {date}");
                LOGGING_SERVICE.Error(exception);
                return null;
            }
        }

        public static int GetAge(DateTime birthDate, DateTime endDate) {
            return (int)Math.Floor((endDate - birthDate).TotalDays / 365.25); ;
        }
    }
}
