using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSCMasterAPI.Utils
{
    /// <summary>
    /// The app's clock, independent of the server's time zone.
    /// - Business dates (enrolment EID dates, onboard dates, petty cash dates, "today") use the business time zone
    ///   (setting "BusinessTimeZone", default Asia/Kolkata): use <see cref="Now"/> / <see cref="Today"/>.
    /// - System timestamps (Entrollment.UploadedDate) are stored in UTC: use DateTime.UtcNow.
    /// Never use DateTime.Now / DateTime.Today: they depend on where the server is.
    /// </summary>
    public static class AppClock
    {
        public const string DefaultBusinessTimeZone = "Asia/Kolkata";
        private static TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById(DefaultBusinessTimeZone);

        public static void Configure(string? timeZoneId)
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
                _tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public static TimeZoneInfo BusinessTimeZone => _tz;
        public static DateTime Now => ToBusinessTime(DateTime.UtcNow);
        public static DateTime Today => Now.Date;

        /// <summary>UTC (or server-local) value to business time; values already in business time are unchanged.</summary>
        public static DateTime ToBusinessTime(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified || IsMinOrMax(value))
                return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, _tz), DateTimeKind.Unspecified);
        }

        /// <summary>Business time to UTC; UTC values are unchanged.</summary>
        public static DateTime ToUtc(DateTime businessTime)
        {
            if (businessTime.Kind == DateTimeKind.Utc) return businessTime;
            if (businessTime.Kind == DateTimeKind.Local) return businessTime.ToUniversalTime();
            if (IsMinOrMax(businessTime)) return DateTime.SpecifyKind(businessTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeToUtc(businessTime, _tz);
        }

        private static bool IsMinOrMax(DateTime v) => v < DateTime.MinValue.AddDays(2) || v > DateTime.MaxValue.AddDays(-2);

        /// <summary>EF converter for UTC columns: values read back are marked UTC (serialized with "Z").</summary>
        public static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
            toDb => toDb.Kind == DateTimeKind.Local ? toDb.ToUniversalTime() : toDb,
            fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));
    }

    /// <summary>
    /// JSON DateTime handling: incoming dates with a time zone ("...Z" from a JavaScript Date) are converted to
    /// business time; dates without one (the date pickers send "2026-03-15T00:00:00") are already business time.
    /// Outgoing values are written as-is (UTC values keep their "Z").
    /// </summary>
    public sealed class BusinessDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            AppClock.ToBusinessTime(reader.GetDateTime());

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
    }
}
