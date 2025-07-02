using iTextSharp.text;
using iTextSharp.text.pdf;
using WebPortalAPI.Models;
using System.Text.Json;

namespace WebPortalAPI.Services
{
    public class PdfService
    {
        private readonly string _logoPath;
        private readonly string _templatePath;

        public PdfService(IWebHostEnvironment env)
        {
            _logoPath = Path.Combine(env.WebRootPath, "images", "logo.png");
            _templatePath = Path.Combine(env.WebRootPath, "templates", "challan-template.pdf");
        }

        public byte[] GenerateChallanPdf(Challan challan)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add Logo if exists
                if (File.Exists(_logoPath))
                {
                    Image logo = Image.GetInstance(_logoPath);
                    logo.ScaleToFit(100f, 100f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    document.Add(logo);
                }

                // Add Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Fee Challan", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Add Challan Details
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                // Challan Info Table
                PdfPTable challanInfo = new PdfPTable(2);
                challanInfo.WidthPercentage = 100;
                challanInfo.SpacingBefore = 10f;
                challanInfo.SpacingAfter = 10f;

                // Add Challan Details
                AddTableRow(challanInfo, "Challan No:", challan.ChallanNo.ToString(), boldFont, normalFont);
                AddTableRow(challanInfo, "Generated Date:", challan.GeneratedDate.ToString("dd-MM-yyyy"), boldFont, normalFont);

                // Add Applicant Details
                AddTableRow(challanInfo, "Applicant Name:", challan.Applicant.FullName, boldFont, normalFont);
                AddTableRow(challanInfo, "CNIC:", challan.Applicant.Cnic, boldFont, normalFont);
                AddTableRow(challanInfo, "Mobile No:", challan.Applicant.MobileNo, boldFont, normalFont);

                // Add Fee Details
                AddTableRow(challanInfo, "Fee Title:", challan.FeeTitle.Title, boldFont, normalFont);
                AddTableRow(challanInfo, "Amount:", $"PKR {challan.FeeAmount:N2}", boldFont, normalFont);
                
                if (challan.FeeTitle.HasExpiry && challan.FeeTitle.ExpiryDate.HasValue)
                {
                    AddTableRow(challanInfo, "Expiry Date:", challan.FeeTitle.ExpiryDate.Value.ToString("dd-MM-yyyy"), boldFont, normalFont);
                }

                document.Add(challanInfo);

                // Add Re-checking details if present
                if (!string.IsNullOrEmpty(challan.Details) && challan.FeeTitle.Title == "Re-Checking of Answer Sheet")
                {
                    var recheckingDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(challan.Details);
                    
                    Paragraph recheckingTitle = new Paragraph("Re-Checking Details", boldFont);
                    recheckingTitle.SpacingBefore = 15f;
                    document.Add(recheckingTitle);

                    PdfPTable recheckingInfo = new PdfPTable(2);
                    recheckingInfo.WidthPercentage = 100;
                    recheckingInfo.SpacingBefore = 10f;

                    AddTableRow(recheckingInfo, "Number of Subjects:", recheckingDetails["numberOfSubjects"].ToString(), boldFont, normalFont);
                    AddTableRow(recheckingInfo, "Roll No:", recheckingDetails["rollNo"].ToString(), boldFont, normalFont);
                    AddTableRow(recheckingInfo, "Category:", recheckingDetails["category"].ToString(), boldFont, normalFont);
                    
                    var subjects = JsonSerializer.Deserialize<List<string>>(recheckingDetails["subjects"].ToString());
                    AddTableRow(recheckingInfo, "Subjects:", string.Join(", ", subjects), boldFont, normalFont);

                    document.Add(recheckingInfo);
                }

                // Add Payment Instructions
                Paragraph instructions = new Paragraph("Payment Instructions:", boldFont);
                instructions.SpacingBefore = 20f;
                document.Add(instructions);

                Paragraph instructionsList = new Paragraph(
                    "1. Please pay this challan at any designated bank branch.\n" +
                    "2. Keep the paid challan copy safe for future reference.\n" +
                    "3. Challan is valid until the expiry date mentioned above.",
                    normalFont
                );
                document.Add(instructionsList);

                // Add QR Code or Barcode if needed
                // TODO: Implement if required

                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            table.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = 0 });
        }
    }
} 