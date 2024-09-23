using ProfanitiesProtector;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromAddress;
    private readonly string smtpServer;
    private readonly int port;
    
    
    public EmailService()
    {
        smtpServer = "smtp.gmail.com";
        port = 587;
        _fromAddress = "profanitiesprotector@gmail.com";
        string password = "iqxxqsbyorfwpcsk";

        _smtpClient = new SmtpClient(smtpServer)
        {
            Port = port,
            Credentials = new NetworkCredential(_fromAddress, password),
            EnableSsl = true
        };
    }

    public void SendEmail(string toAddress, AnalyzedChat chat)
    {
        var MessagesOutString = string.Join("\n", chat.DetectedMessagesOut);
        var MessagesInString = string.Join("\n", chat.DetectedMessagesIn);
        //var ImagesOutString = string.Join("\n", chat.DetectedImagesOut);
        //var ImagesInString = string.Join("\n", chat.DetectedImagesIn);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromAddress),
            Subject = "Profanities Protector Notification",
            Body = "We have detected suspected cyberbullying sent on your child's WhatsApp account.\r\n" +
            $"Chat name:{chat.ChatName}\r\n" +
            (MessagesOutString != "" ? "Your child send those messages:\r\n" + MessagesOutString:"")+"\r\n"+
            (MessagesInString != "" ? "Your child got those messages:\r\n" + MessagesInString : "")+ "\r\n",
            //(ImagesOutString != "" ? "Your child send those images:\r\n" + ImagesOutString : "")+
            //(ImagesInString != "" ? "Your child send those messages:\r\n" + ImagesInString : "") +
            //"Image:",
            IsBodyHtml = false
        };

        mailMessage.To.Add(toAddress);

        try
        {
            _smtpClient.Send(mailMessage);
            Console.WriteLine("Email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
}
