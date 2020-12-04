using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace Quickstart.AspNetCore
{
    public class OnlineCourcesBot : BotBase
    {
        public OnlineCourcesBot(IOptions<BotOptions<OnlineCourcesBot>> options)
            : base(options.Value)
        {
        }
    }
}