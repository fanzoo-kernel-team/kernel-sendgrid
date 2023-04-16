using Fanzoo.Kernel.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using Attachment = SendGrid.Helpers.Mail.Attachment;

namespace Fanzoo.Kernel.SendGrid.Services
{
    public class SendGridSettings
    {
        public const string SectionName = $"{EmailServiceFactorySettings.SectionName}:Settings";

        public string? From { get; set; } = default!;

        public string ApiKey { get; set; } = default!;
    }

    public sealed class SendGridEmailService : IEmailService
    {
        private readonly SendGridSettings _settings;

        public SendGridEmailService(IOptions<SendGridSettings> sendGridSettings)
        {
            _settings = sendGridSettings.Value;
        }

        public string Name => "SendGrid";

        public async ValueTask SendEmailAsync(string[] to, string subject, string? from = null, string[]? cc = null, string[]? bcc = null, string? htmlContent = null, string? plainTextContent = null, EmailAttachment[]? attachments = null)
        {
            var client = new SendGridClient(_settings.ApiKey);

            from ??= _settings.From;

            if (from is null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            var message = new SendGridMessage
            {
                From = from.ToEmailAddress(),
                Subject = subject,
                HtmlContent = htmlContent,
                PlainTextContent = plainTextContent
            };

            message.AddTos(to.Select(t => t.ToEmailAddress()).ToList());

            if (cc is not null)
            {
                message.AddCcs(cc.Select(b => b.ToEmailAddress()).ToList());
            }

            if (bcc is not null)
            {
                message.AddBccs(bcc.Select(b => b.ToEmailAddress()).ToList());
            }

            if (attachments is not null)
            {
                message.AddAttachments(attachments.Select(a => a.ToSendGridAttachment()));
            }

            var result = await client.SendEmailAsync(message);

            if (result.IsSuccessStatusCode is not true)
            {
                var body = await result.Body.ReadAsStringAsync();

                throw new InvalidOperationException($"Sending message failed.\n\n{body}");
            }
        }
    }

    static file class Extensions
    {
        public static EmailAddress ToEmailAddress(this string value)
        {
            //use MailKit's parser :)
            var mailboxAddress = MailboxAddress.Parse(value);

            return new(mailboxAddress.Address, mailboxAddress.Name);
        }

        public static Attachment ToSendGridAttachment(this EmailAttachment attachment) => new()
        {
            Content = Convert.ToBase64String(attachment.Data),
            Filename = attachment.Filename,
            Type = attachment.MIMEType
        };
    }
}
