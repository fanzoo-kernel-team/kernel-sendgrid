using System.Reflection;
using Fanzoo.Kernel.Builder;
using Fanzoo.Kernel.DependencyInjection;
using Fanzoo.Kernel.SendGrid.Services;
using Fanzoo.Kernel.Services;
using Microsoft.AspNetCore.Builder;

namespace Fanzoo.Kernel.SendGrid
{
    public static class EmailFactoryBuilderExtensions
    {
        public static EmailFactoryBuilder AddSendGrid(this EmailFactoryBuilder builder)
        {
            builder.WebApplicationBuilder.AddTransient<IEmailService, SendGridEmailService>();

            builder.WebApplicationBuilder.AddSetting<SendGridSettings>(SendGridSettings.SectionName);

            return builder;
        }
    }
}
