namespace BankingPaymentsAPI.Services.Notification
{
    public interface IEmailNotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

}
