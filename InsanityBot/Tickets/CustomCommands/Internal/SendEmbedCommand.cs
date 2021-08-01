﻿using DSharpPlus.CommandsNext;

using InsanityBot.Core.Formatters.Embeds;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityBot.Tickets.CustomCommands.Internal
{
    public class SendEmbedCommand
    {
        public void ProcessSendCommand(CommandContext context, Object parameter)
        {
            if(parameter is not String embed)
            {
                throw new ArgumentException("Invalid datatype for embed format string.", nameof(parameter));
            }

            context.Channel.SendMessageAsync((InsanityBot.EmbedFactory.GetFormatter() as EmbedFormatter).Read(embed));
        }
    }
}
