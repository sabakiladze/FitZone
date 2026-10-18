using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application
{
    public static class VerificationEmailTemplate
    {
        public static string Build(string firstName, string code, int expires)
        {
            var name = WebUtility.HtmlEncode(firstName);
            var safeCode = WebUtility.HtmlEncode(code);

            return
                "<div style='background:#f4f5f7;padding:32px 0;font-family:Segoe UI,Arial,sans-serif;'>" +
                  "<div style='max-width:460px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;'>" +

                    "<div style='background:#111827;padding:22px;text-align:center;'>" +
                      "<span style='color:#ffffff;font-size:26px;font-weight:700;letter-spacing:2px;'>FIT" +
                      "<span style='color:#22c55e;'>ZONE</span></span>" +
                    "</div>" +

                    "<div style='padding:28px 30px 8px 30px;'>" +
                      $"<h2 style='margin:0 0 10px 0;color:#111827;font-size:20px;'>Hi {name},</h2>" +
                      "<p style='margin:0;color:#4b5563;font-size:15px;line-height:1.6;'>" +
                        "Use the code below to verify your email address.</p>" +
                    "</div>" +

                    "<div style='text-align:center;padding:18px 30px 22px 30px;'>" +
                      "<span style='display:inline-block;background:#f0fdf4;border:2px dashed #22c55e;" +
                      "border-radius:10px;padding:14px 28px;font-size:32px;font-weight:700;" +
                      $"letter-spacing:8px;color:#111827;'>{safeCode}</span>" +
                    "</div>" +

                    "<div style='padding:0 30px 28px 30px;'>" +
                      $"<p style='margin:0 0 6px 0;color:#4b5563;font-size:14px;'>This code expires in <b>{expires} minutes</b>.</p>" +
                      "<p style='margin:0;color:#9ca3af;font-size:13px;'>If you didn't create a FitZone account, ignore this email.</p>" +
                    "</div>" +

                  "</div>" +
                "</div>";
        }
    }
}
