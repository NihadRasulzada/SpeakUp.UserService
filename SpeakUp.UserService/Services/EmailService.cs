namespace SpeakUp.UserService.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public string GenerateConfirmationLink(Guid confirmationId)
    {
        var baseUrl = _configuration["ConfirmationLinkBase"] + confirmationId;
        
        return baseUrl;
    }

    public async Task SendEmail(string toEmailAddress, string content)
    {
        // Send Email
    }
}