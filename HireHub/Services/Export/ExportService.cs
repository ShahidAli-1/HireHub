using System.Globalization;
using System.Text;
using HireHub.Models;

namespace HireHub.Services.Export
{
    public class ExportService : IExportService
    {
        public byte[] ExportPaymentsToCsv(List<Payment> payments)
        {
            var sb = new StringBuilder();
            // Header
            sb.AppendLine("PaymentId,Date,User,Email,Amount,Purpose,Method,Reference,Status,ReviewedAt");

            foreach (var p in payments)
            {
                sb.AppendLine(string.Join(",",
                    p.PaymentId,
                    p.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    CsvEscape(p.User?.FullName ?? ""),
                    CsvEscape(p.User?.Email ?? ""),
                    p.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    CsvEscape(p.Purpose),
                    CsvEscape(p.Method),
                    CsvEscape(p.ReferenceNumber ?? ""),
                    p.Status,
                    p.ReviewedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""
                ));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] ExportUsersToCsv(List<User> users)
        {
            var sb = new StringBuilder();
            sb.AppendLine("UserId,FullName,Email,Role,Plan,IsActive,IsVerified,Joined,LastLogin");

            foreach (var u in users)
            {
                sb.AppendLine(string.Join(",",
                    u.UserId,
                    CsvEscape(u.FullName),
                    CsvEscape(u.Email),
                    u.Role,
                    u.CurrentPlan,
                    u.IsActive ? "Yes" : "No",
                    u.IsVerified ? "Yes" : "No",
                    u.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    u.LastLoginAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""
                ));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        public byte[] ExportApplicationsToCsv(List<Application> applications)
        {
            var sb = new StringBuilder();
            // Header
            sb.AppendLine("ApplicationId,Date,JobTitle,CandidateName,Email,Phone,Location,Status,CoverLetter");

            foreach (var app in applications)
            {
                sb.AppendLine(string.Join(",",
                    app.ApplicationId,
                    app.AppliedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    CsvEscape(app.Job?.Title ?? ""),
                    CsvEscape(app.JobSeeker?.FullName ?? ""),
                    CsvEscape(app.JobSeeker?.Email ?? ""),
                    CsvEscape(app.JobSeeker?.Phone ?? ""),
                    CsvEscape(app.JobSeeker?.JobSeekerProfile?.Location ?? ""),
                    app.Status,
                    CsvEscape(app.CoverLetter ?? "")
                ));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}