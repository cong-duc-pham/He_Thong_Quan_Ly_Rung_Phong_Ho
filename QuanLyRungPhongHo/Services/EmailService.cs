using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;

namespace QuanLyRungPhongHo.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(Encoding.UTF8, fromName, fromEmail));
                message.To.Add(new MailboxAddress(Encoding.UTF8, "", toEmail));
                message.Subject = subject;

                // Use TextPart with explicit UTF-8 charset
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(username, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "Mã OTP Đặt Lại Mật Khẩu";
            var body = GetOtpEmailHtml(otpCode);
            return await SendEmailAsync(toEmail, subject, body);
        }

        private string GetOtpEmailHtml(string otpCode)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"vi\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>Mã OTP</title>");
            html.AppendLine("</head>");
            html.AppendLine("<body style=\"margin:0;padding:0;font-family:Arial,sans-serif;background-color:#f4f4f4;\">");

            // Main container
            html.AppendLine("    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:#f4f4f4;padding:20px;\">");
            html.AppendLine("        <tr><td align=\"center\">");
            html.AppendLine("            <table width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);\">");

            // Header
            html.AppendLine("                <tr>");
            html.AppendLine("                    <td style=\"background-color:#2e7d32;padding:30px 20px;text-align:center;border-radius:8px 8px 0 0;\">");
            html.AppendLine("                        <h1 style=\"margin:0;color:#ffffff;font-size:24px;font-weight:bold;\">");
            html.AppendLine("                            Hệ Thống Quản Lý Rừng Phòng Hộ");
            html.AppendLine("                        </h1>");
            html.AppendLine("                    </td>");
            html.AppendLine("                </tr>");

            // Content
            html.AppendLine("                <tr>");
            html.AppendLine("                    <td style=\"padding:40px 30px;\">");

            html.AppendLine("                        <p style=\"font-size:16px;color:#333;margin:0 0 20px 0;line-height:1.6;\">");
            html.AppendLine("                            Xin chào,");
            html.AppendLine("                        </p>");

            html.AppendLine("                        <p style=\"font-size:16px;color:#333;margin:0 0 20px 0;line-height:1.6;\">");
            html.AppendLine("                            Bạn đã yêu cầu đặt lại mật khẩu. Mã OTP xác thực của bạn là:");
            html.AppendLine("                        </p>");

            // OTP Code box
            html.AppendLine("                        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:30px 0;\">");
            html.AppendLine("                            <tr><td align=\"center\">");
            html.AppendLine("                                <div style=\"background-color:#e8f5e9;border:2px dashed #4caf50;border-radius:8px;padding:20px;display:inline-block;\">");
            html.AppendLine($"                                    <span style=\"font-size:36px;font-weight:bold;color:#2e7d32;letter-spacing:8px;\">{otpCode}</span>");
            html.AppendLine("                                </div>");
            html.AppendLine("                            </td></tr>");
            html.AppendLine("                        </table>");

            html.AppendLine("                        <p style=\"font-size:16px;color:#333;margin:0 0 20px 0;line-height:1.6;\">");
            html.AppendLine("                            Mã OTP này có hiệu lực trong <strong style=\"color:#2e7d32;\">10 phút</strong>.");
            html.AppendLine("                        </p>");

            html.AppendLine("                        <p style=\"font-size:16px;color:#333;margin:0 0 20px 0;line-height:1.6;\">");
            html.AppendLine("                            Vui lòng nhập mã này vào trang xác nhận để tiếp tục đặt lại mật khẩu.");
            html.AppendLine("                        </p>");

            // Warning box
            html.AppendLine("                        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:#fff3cd;border-left:4px solid #ffc107;border-radius:4px;margin:25px 0;\">");
            html.AppendLine("                            <tr><td style=\"padding:15px 20px;\">");
            html.AppendLine("                                <p style=\"font-weight:bold;color:#856404;margin:0 0 10px 0;font-size:16px;\">");
            html.AppendLine("                                    ⚠ Lưu ý bảo mật:");
            html.AppendLine("                                </p>");
            html.AppendLine("                                <ul style=\"color:#856404;margin:0;padding-left:20px;font-size:14px;\">");
            html.AppendLine("                                    <li style=\"margin:8px 0;\">Không chia sẻ mã OTP này với bất kỳ ai</li>");
            html.AppendLine("                                    <li style=\"margin:8px 0;\">Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>");
            html.AppendLine("                                    <li style=\"margin:8px 0;\">Liên hệ quản trị viên nếu bạn nghi ngờ có hành vi truy cập trái phép</li>");
            html.AppendLine("                                </ul>");
            html.AppendLine("                            </td></tr>");
            html.AppendLine("                        </table>");

            html.AppendLine("                    </td>");
            html.AppendLine("                </tr>");

            // Footer
            html.AppendLine("                <tr>");
            html.AppendLine("                    <td style=\"background-color:#f8f9fa;padding:20px;text-align:center;border-top:1px solid #e0e0e0;border-radius:0 0 8px 8px;\">");
            html.AppendLine("                        <p style=\"font-size:13px;color:#888;margin:0 0 5px 0;\">");
            html.AppendLine("                            Email này được gửi tự động. Vui lòng không trả lời.");
            html.AppendLine("                        </p>");
            html.AppendLine("                        <p style=\"font-size:13px;color:#888;margin:5px 0 0 0;\">");
            html.AppendLine("                            &copy; 2025 Hệ Thống Quản Lý Rừng Phòng Hộ");
            html.AppendLine("                        </p>");
            html.AppendLine("                    </td>");
            html.AppendLine("                </tr>");

            html.AppendLine("            </table>");
            html.AppendLine("        </td></tr>");
            html.AppendLine("    </table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}