﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Sample.Bots.EchoBot;
using NetTelegram.Bot.Sample.Bots.GreeterBot;
using RecurrentTasks;

namespace NetTelegram.Bot.Sample
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables("SampleEchoBot_")
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var echoBotOptions = new BotOptions<EchoBot>();
            Configuration.GetSection("EchoBot").Bind(echoBotOptions);

            services.AddTelegramBot(echoBotOptions)
                .AddUpdateHandler<TextMessageEchoer>()
                .Configure();
            services.AddTask<BotUpdateGetterTask<EchoBot>>();

            services.AddTelegramBot<GreeterBot>(Configuration.GetSection("GreeterBot"))
                .AddUpdateHandler<StartCommand>()
                .AddUpdateHandler<PhotoForwarder>()
                .AddUpdateHandler<HiCommand>()
                .Configure();
            services.AddTask<BotUpdateGetterTask<GreeterBot>>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseTelegramBotWebhook<EchoBot>(ensureWebhookEnabled: !env.IsDevelopment());

            app.UseTelegramBotWebhook<GreeterBot>(ensureWebhookEnabled: !env.IsDevelopment());

            if (env.IsDevelopment())
            {
                app.StartTask<BotUpdateGetterTask<EchoBot>>(TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(3));

                app.StartTask<BotUpdateGetterTask<GreeterBot>>(TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(3));
            }

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
