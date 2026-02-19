using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Reporting.NETCore;
using RamadanReportApp.Data;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace RamadanReportApp.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult RamadanReport()
        {
            //  Fetch data
            var data = _context.RamadanSchedule
            .Select(r => new
            {
                r.RamadanDate,
                HijriDate = r.HijriDate ?? "",
                SehriTime =
                    (r.SehriTime.Hours % 12 == 0 ? 12 : r.SehriTime.Hours % 12)
                    + ":" + r.SehriTime.Minutes.ToString("D2")
                    + (r.SehriTime.Hours >= 12 ? " PM" : " AM"),
                IftarTime =
                    (r.IftarTime.Hours % 12 == 0 ? 12 : r.IftarTime.Hours % 12)
                    + ":" + r.IftarTime.Minutes.ToString("D2")
                    + (r.IftarTime.Hours >= 12 ? " PM" : " AM")
            })
            .ToList();



            if (!data.Any())
                return Content("No Ramadan data found.");

            // 2️⃣ Load RDLC file (sanitize if it contains 8-digit ARGB colors like #AARRGGBB)
            string origPath = Path.Combine(_env.ContentRootPath, "Report", "RamadanReport.rdlc");
            string reportPathToUse = origPath;
            string tempFile = null;

            try
            {
                string xml = System.IO.File.ReadAllText(origPath);

                // Replace any 8-digit hex color (#AARRGGBB) with 6-digit hex (#RRGGBB)
                bool modified = false;
                string fixedXml = Regex.Replace(xml, "#([0-9A-Fa-f]{8})", match =>
                {
                    modified = true;
                    string hex8 = match.Groups[1].Value;
                    // drop first two chars (AA) and keep RRGGBB
                    return "#" + hex8.Substring(2);
                });

                if (modified)
                {
                    tempFile = Path.Combine(Path.GetTempPath(), "RamadanReport_fixed_" + Guid.NewGuid().ToString("N") + ".rdlc");
                    System.IO.File.WriteAllText(tempFile, fixedXml);
                    reportPathToUse = tempFile;
                }

                LocalReport report = new LocalReport();
                report.ReportPath = reportPathToUse;

                // Enable external images
                report.EnableExternalImages = true;

                // Add dataset (must match RDLC dataset name)
                report.DataSources.Add(new ReportDataSource("RamadanTable", data));

                // Convert logo to Base64 for RDLC parameter
                string logoPath = Path.Combine(_env.WebRootPath, "Images", "logo-light.png");
                string logoBase64 = "";
                if (System.IO.File.Exists(logoPath))
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(logoPath);
                    logoBase64 = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                }

                // Set RDLC parameter (must match parameter name "logobg")
                report.SetParameters(new[]
                {
                    new ReportParameter("logobg", logoBase64)
                });

                //  Render PDF
                byte[] pdfBytes = report.Render("PDF");

                // Return PDF inline (view in browser)
                return File(pdfBytes, "application/pdf");
            }
            finally
            {
                // Clean up temporary sanitized file if we created one
                if (!string.IsNullOrEmpty(tempFile) && System.IO.File.Exists(tempFile))
                {
                    try { System.IO.File.Delete(tempFile); } catch { /* ignore cleanup errors */ }
                }
            }
        }
    }
}
