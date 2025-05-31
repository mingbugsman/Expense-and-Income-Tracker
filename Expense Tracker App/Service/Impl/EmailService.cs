using MailKit.Net.Smtp;
using MimeKit;
using Expense_Tracker_App.Service;
using MailKit.Security;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        Console.WriteLine($"Attempting to send email to: {to}");
        Console.WriteLine($"Using SMTP settings - Host: {emailSettings["Host"]}, Port: {emailSettings["Port"]}");
        Console.WriteLine($"From email: {emailSettings["Mail"]}");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(emailSettings["DisplayName"], emailSettings["Mail"]));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = body };

        using var smtp = new SmtpClient();
        try
        {
            Console.WriteLine("Connecting to SMTP server...");
            await smtp.ConnectAsync(emailSettings["Host"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);
            Console.WriteLine("Connected successfully, attempting authentication...");
            
            await smtp.AuthenticateAsync(emailSettings["Mail"], emailSettings["Password"]);
            Console.WriteLine("Authentication successful, sending email...");
            
            await smtp.SendAsync(email);
            Console.WriteLine("Email sent successfully");
            
            await smtp.DisconnectAsync(true);
            Console.WriteLine("Disconnected from SMTP server");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendEmailAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
