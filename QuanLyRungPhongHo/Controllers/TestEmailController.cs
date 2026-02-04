using Microsoft.AspNetCore.Mvc;
using QuanLyRungPhongHo.Services;

namespace QuanLyRungPhongHo.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // Test endpoint: /TestEmail/SendTest?email=your-email@gmail.com
        [HttpGet]
        public async Task<IActionResult> SendTest(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Vui lòng cung c?p ??a ch? email. Ví d?: /TestEmail/SendTest?email=test@example.com");
            }

            try
            {
                // Generate a test OTP
                var testOtp = new Random().Next(0, 999999).ToString("D6");
                
                // Send test email
                var result = await _emailService.SendOtpEmailAsync(email, testOtp);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Email test ?ã ???c g?i thành công ??n {email}",
                        otp = testOtp,
                        note = "Ki?m tra h?p th? c?a b?n (bao g?m c? Spam/Junk)"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Không th? g?i email. Ki?m tra logs ?? bi?t chi ti?t."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "L?i: " + ex.Message
                });
            }
        }

        // Test HTML render: /TestEmail/PreviewOtp
        [HttpGet]
        public async Task<IActionResult> PreviewOtp()
        {
            try
            {
                var testOtp = "123456";
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "OtpEmail.html");
                
                if (System.IO.File.Exists(templatePath))
                {
                    var htmlBody = await System.IO.File.ReadAllTextAsync(templatePath, System.Text.Encoding.UTF8);
                    htmlBody = htmlBody.Replace("{{OTP_CODE}}", testOtp);
                    return Content(htmlBody, "text/html", System.Text.Encoding.UTF8);
                }
                else
                {
                    return NotFound($"Template not found at: {templatePath}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
