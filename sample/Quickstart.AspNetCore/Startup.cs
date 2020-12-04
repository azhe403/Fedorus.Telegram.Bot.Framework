using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quickstart.AspNetCore.Handlers;
using Quickstart.AspNetCore.Services;
using System;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Framework.Extensions;

namespace Quickstart.AspNetCore
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<EchoBot>()
                .Configure<BotOptions<EchoBot>>(Configuration.GetSection("EchoBot"))
                .AddScoped<TextEchoer>()
                .AddScoped<PingCommand>()
                .AddScoped<StartCommand>()
                .AddScoped<UpdateLogger>()
                .AddScoped<StickerHandler>()
                .AddScoped<WeatherReporter>()
                .AddScoped<ExceptionHandler>()
                .AddScoped<UpdateMembersList>()
                .AddScoped<CallbackQueryHandler>()
                ;
            services.AddTransient<OnlineCourcesBot>()
                .Configure<BotOptions<OnlineCourcesBot>>(Configuration.GetSection("OnlineCourcesBot"));
            services.AddScoped<IWeatherService, WeatherService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // get bot updates from Telegram via long-polling approach during development
                // this will disable Telegram webhooks
                app.UseTelegramBotLongPolling<EchoBot>(ConfigureBot(), startAfter: TimeSpan.FromSeconds(2));
            }
            //else
            {
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton(app.ApplicationServices.GetService<EchoBot>());
                // use Telegram bot webhook middleware in higher environments
                app.UseTelegramBotWebhook<OnlineCourcesBot>(ConfigureBot());
                app.UseTelegramBotWebhook<EchoBot>(ConfigureBot());
                // and make sure webhook is enabled
                app.EnsureWebhookSet<OnlineCourcesBot>();
                app.EnsureWebhookSet<EchoBot>();
            }

            app.Run(async context => { await context.Response.WriteAsync("Hello World!"); });
        }

        private IBotBuilder ConfigureBot()
        {
            return new BotBuilder()
                    .Use<ExceptionHandler>()
                    .Use<UpdateLogger>()

                    // .Use<CustomUpdateLogger>()
                    .UseWhen<UpdateMembersList>(When.MembersChanged)
                    .UseWhen(When.NewMessage, msgBranch => msgBranch
                        .UseWhen(When.NewTextMessage, txtBranch => txtBranch
                                .Use<TextEchoer>()
                                .UseWhen(When.NewCommand, cmdBranch => cmdBranch
                                    .UseCommand<PingCommand>("ping")
                                    .UseCommand<StartCommand>("start")
                                )
                            //.Use<NLP>()
                        )
                        .UseWhen<StickerHandler>(When.StickerMessage)
                        .UseWhen<WeatherReporter>(When.LocationMessage)
                    )
                    .UseWhen<CallbackQueryHandler>(When.CallbackQuery)

                // .Use<UnhandledUpdateReporter>()
                ;
        }
    }
}