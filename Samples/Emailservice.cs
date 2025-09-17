namespace Neon.Samples;

public class EmailService
{

    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendWelcomeEmail(string email, string name)
    {
        _logger.LogInformation($"Sending welcome email to {email} for {name}");

        // simulate email sending
        await Task.Delay(2000);

        if (Random.Shared.Next(1, 10) == 1)
        {
            throw new InvalidOperationException("SMTP Server temporarily unavailable");
        }

        _logger.LogInformation($"Welcome email successfully sent to {email}");
    }
   public void SendNotification(string userId, string message)
        {
            _logger.LogInformation("Sending notification to user {UserId}: {Message}", userId, message);
            
            // Simulate notification sending
            Thread.Sleep(500);
            
            _logger.LogInformation("Notification sent to user {UserId}", userId);
        }

    public async Task SendNewsLetter(int subscriptionId)
    {

        _logger.LogInformation($"Processing newsletter for {subscriptionId}");

        await Task.Delay(1000);
        _logger.LogInformation($"Newsletter sent successfully to user with subscription id {subscriptionId}");
    }

}