using System.Collections.Concurrent;

namespace QuanLyRungPhongHo.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string identifier, bool isEmail = false);
        bool VerifyOtp(string identifier, string otpCode);
        void RevokeOtp(string identifier);
        int GetOtpRemainingTime(string identifier);
    }

    public class OtpService : IOtpService
    {
        private static readonly ConcurrentDictionary<string, OtpData> _otpStorage = 
            new ConcurrentDictionary<string, OtpData>();

        private readonly IEmailService _emailService;
        private const int OTP_EXPIRY_MINUTES = 10;
        private const int OTP_LENGTH = 6;

        public OtpService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        private class OtpData
        {
            public string Code { get; set; } = null!;
            public DateTime ExpiryTime { get; set; }
            public int AttemptCount { get; set; }
        }

        /// <summary>
        /// Generate and send OTP code
        /// </summary>
        public async Task<string> GenerateOtpAsync(string identifier, bool isEmail = false)
        {
            // Tạo mã OTP 6 chữ số ngẫu nhiên
            var random = new Random();
            string otpCode = random.Next(0, 999999).ToString("D6");

            var expiryTime = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);
            
            var otpData = new OtpData
            {
                Code = otpCode,
                ExpiryTime = expiryTime,
                AttemptCount = 0
            };

            _otpStorage.AddOrUpdate(identifier, otpData, (key, old) => otpData);

            if (isEmail)
            {
                // Gửi OTP qua Email
                await _emailService.SendOtpEmailAsync(identifier, otpCode);
            }
            else
            {
                // TODO: Implement SMS service integration
                System.Diagnostics.Debug.WriteLine($"OTP for {identifier}: {otpCode}");
            }

            return otpCode;
        }

        /// <summary>
        /// Verify OTP code with rate limiting (max 3 attempts)
        /// </summary>
        public bool VerifyOtp(string identifier, string otpCode)
        {
            if (!_otpStorage.TryGetValue(identifier, out var otpData))
                return false;

            // Kiểm tra hết hạn
            if (DateTime.UtcNow > otpData.ExpiryTime)
            {
                _otpStorage.TryRemove(identifier, out _);
                return false;
            }

            // Giới hạn số lần nhập sai
            if (otpData.AttemptCount >= 3)
            {
                _otpStorage.TryRemove(identifier, out _);
                return false;
            }

            if (otpData.Code != otpCode)
            {
                otpData.AttemptCount++;
                return false;
            }

            // Xóa OTP sau khi xác nhận thành công
            _otpStorage.TryRemove(identifier, out _);
            return true;
        }

        public void RevokeOtp(string identifier)
        {
            _otpStorage.TryRemove(identifier, out _);
        }

        /// <summary>
        /// Get remaining time in seconds before OTP expires
        /// </summary>
        public int GetOtpRemainingTime(string identifier)
        {
            if (!_otpStorage.TryGetValue(identifier, out var otpData))
                return 0;

            var remainingSeconds = (int)(otpData.ExpiryTime - DateTime.UtcNow).TotalSeconds;
            return Math.Max(0, remainingSeconds);
        }
    }
}