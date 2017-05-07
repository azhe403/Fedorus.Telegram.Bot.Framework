﻿using System.Threading.Tasks;
using NetTelegramBot.Framework;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using SampleEchoBot.Services;

namespace SampleEchoBot.Commands
{
    public interface IMessageForwarder : IMessageHandler<EchoBot>
    {

    }

    public class MessageForwarder : MessageHandlerBase<EchoBot>, IMessageForwarder
    {
        public override bool CanHandle(Update update)
        {
            return update.Message.Chat != null;
        }

        public override async Task HandleMessageAsync(Update update)
        {
            var req = new ForwardMessage(update.Message.Chat.Id, update.Message.Chat.Id, update.Message.MessageId);
            await Bot.MakeRequestAsync(req);
        }
    }
}
