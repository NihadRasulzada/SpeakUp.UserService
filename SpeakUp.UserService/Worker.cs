using SpeakUp.Common;
using SpeakUp.Common.Events.User;
using SpeakUp.Common.Infratructure;
using SpeakUp.UserService.Services;
using System.Text;
using System.Text.Json;

namespace SpeakUp.UserService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly UserService.Services.UserService userService;
    private readonly EmailService emailService;

    public Worker(ILogger<Worker> logger, UserService.Services.UserService userService, EmailService emailService)
    {
        _logger = logger;
        this.userService = userService;
        this.emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var userEmailChangedConsumer = await (await (await QueueFactory.CreateBasicConsumerAsync())
                .EnsureExchangeAsync(SpeakUpConstants.UserExchangeName))
            .EnsureQueueAsync(SpeakUpConstants.UserEmailChangedQueueName, SpeakUpConstants.UserExchangeName);

        userEmailChangedConsumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var user = JsonSerializer.Deserialize<UserEmailChangedEvent>(message);

                var confirmationId = await userService.CreateEmailConfirmation(user);
                var link = emailService.GenerateConfirmationLink(confirmationId);
                await emailService.SendEmail(user.NewEmailAddress, link);
                
                _logger.LogInformation("Email confirmation sent to {Email}", user.NewEmailAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserEmailChangedEvent");
            }
        };

        await userEmailChangedConsumer.StartConsumingAsync(SpeakUpConstants.UserEmailChangedQueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}