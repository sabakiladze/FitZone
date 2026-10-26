using System.Net;

namespace FitZone.Application.Helpers
{
    public static class TrainerEmailTemplates
    {
        public static string GenerateAddedBody(string fullName, string specialization, int experienceYears)
        {
            var content =
                Paragraph("A new trainer has been added to FitZone.") +
                "<table style='width:100%;border-collapse:collapse;'>" +
                  Row("Full name", fullName) +
                  Row("Specialization", specialization) +
                  Row("Experience", $"{experienceYears} years") +
                "</table>";

            return Wrap("New trainer added", content);
        }

        public static string GenerateUpdatedBody( string fullName, string specialization, int experienceYears)
        {
            var content =
                Paragraph("A trainer's details have been updated. Current values:") +
                "<table style='width:100%;border-collapse:collapse;'>" +
                  Row("Full name", fullName) +
                  Row("Specialization", specialization) +
                  Row("Experience", $"{experienceYears} years") +
                "</table>";

            return Wrap("Trainer updated", content);
        }

        public static string GenerateDeletedBody(string fullName)
        {
            var content =
                Paragraph("A trainer has been removed from FitZone.") +
                "<table style='width:100%;border-collapse:collapse;'>" +
                  Row("Full name", fullName) +
                  Row("Removed at (UTC)", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")) +
                "</table>";

            return Wrap("Trainer removed", content);
        }

        private static string Paragraph(string text)
        {
            return
                "<p style='margin:0 0 14px 0;color:#4b5563;font-size:15px;line-height:1.6;'>" +
                WebUtility.HtmlEncode(text) +
                "</p>";
        }
        
        private static string Row(string label, string value)
        {
            return
                "<tr>" +
                  $"<td style='padding:8px 0;color:#6b7280;font-size:14px;width:40%;'>{WebUtility.HtmlEncode(label)}</td>" +
                  $"<td style='padding:8px 0;color:#111827;font-size:14px;font-weight:600;'>{WebUtility.HtmlEncode(value)}</td>" +
                "</tr>";
        }

        private static string Wrap(string title, string contentHtml)
        {
            return
                "<div style='background:#f4f5f7;padding:32px 0;font-family:Segoe UI,Arial,sans-serif;'>" +
                  "<div style='max-width:460px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;'>" +

                    "<div style='background:#111827;padding:22px;text-align:center;'>" +
                      "<span style='color:#ffffff;font-size:26px;font-weight:700;letter-spacing:2px;'>FIT" +
                      "<span style='color:#22c55e;'>ZONE</span></span>" +
                    "</div>" +

                    "<div style='padding:28px 30px;'>" +
                      $"<h2 style='margin:0 0 14px 0;color:#111827;font-size:20px;'>{WebUtility.HtmlEncode(title)}</h2>" +
                      contentHtml +
                    "</div>" +

                  "</div>" +
                "</div>";
        }
    }
}