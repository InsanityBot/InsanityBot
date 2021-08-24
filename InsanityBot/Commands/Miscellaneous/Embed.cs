﻿using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using InsanityBot.Core.Formatters.Embeds;
using InsanityBot.Utility.Permissions;

namespace InsanityBot.Commands.Miscellaneous
{
    public class Embed : BaseCommandModule
    {
        [Command("embed")]
        public async Task SayEmbedCommand(CommandContext ctx,
            [RemainingText]
            String text)
        {
            if(!ctx.Member.HasPermission("insanitybot.miscellaneous.say.embed"))
            {
                await ctx.Channel?.SendMessageAsync(InsanityBot.LanguageConfig["insanitybot.error.lacking_permission"]);
                return;
            }

            _ = ctx.Message?.DeleteAsync();
            _ = ctx.Channel?.SendMessageAsync((InsanityBot.EmbedFactory.GetFormatter() as EmbedFormatter).Read(text));
        }
    }
}
