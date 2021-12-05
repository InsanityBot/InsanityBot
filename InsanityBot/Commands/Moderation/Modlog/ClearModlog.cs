﻿namespace InsanityBot.Commands.Moderation.Modlog;
using System;
using System.IO;
using System.Threading.Tasks;

using CommandLine;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using global::InsanityBot.Core.Attributes;

using Microsoft.Extensions.Logging;

using static global::InsanityBot.Commands.StringUtilities;

public class ClearModlog : BaseCommandModule
{
	[Command("clearmodlog")]
	[RequirePermission("insanitybot.moderation.clear_modlog")]
	public async Task ClearModlogCommand(CommandContext ctx,
		DiscordMember member,
		String arguments = "usedefault")
	{

		if(arguments.StartsWith('-'))
		{
			await this.ParseClearModlogCommand(ctx, member, arguments);
			return;
		}
		await this.ExecuteClearModlogCommand(ctx, member, false, arguments);
	}

	private async Task ParseClearModlogCommand(CommandContext ctx,
		DiscordMember member,
		String arguments = "usedefault")
	{
		String cmdArguments = arguments;
		try
		{
			if(!arguments.Contains("-r") && !arguments.Contains("--reason"))
			{
				cmdArguments += " --reason usedefault";
			}

			await Parser.Default.ParseArguments<ClearModlogOptions>(cmdArguments.Split(' '))
				.WithParsedAsync(async o =>
				{
					await this.ExecuteClearModlogCommand(ctx, member, o.Silent, String.Join(' ', o.Reason));
				});
		}
		catch(Exception e)
		{
			DiscordEmbedBuilder failed = InsanityBot.Embeds["insanitybot.error"]
				.WithDescription(GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.clear_modlog.failure"], ctx, member));

			InsanityBot.Client.Logger.LogError($"{e}: {e.Message}");

			await ctx.Channel?.SendMessageAsync(embed: failed.Build());
		}
	}

	private async Task ExecuteClearModlogCommand(CommandContext ctx,
		DiscordMember member,
		Boolean silent,
		String reason)
	{
		if(silent)
		{
			await ctx.Message?.DeleteAsync();
		}

		String ClearReason = reason switch
		{
			"usedefault" => GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.no_reason_given"],
				ctx, member),
			_ => GetFormattedString(reason, ctx, member)
		};

		DiscordEmbedBuilder embedBuilder = null;
		DiscordEmbedBuilder moderationEmbedBuilder = InsanityBot.Embeds["insanitybot.modlog.clear_modlog"];

		moderationEmbedBuilder.AddField("Moderator", ctx.Member?.Mention, true)
			.AddField("Member", member.Mention, true)
			.AddField("Reason", ClearReason, true);

		try
		{
			File.Delete($"./data/{member.Id}/modlog.json");
			embedBuilder = new DiscordEmbedBuilder
			{
				Description = GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.clear_modlog.success"],
					ctx, member),
				Color = DiscordColor.SpringGreen,
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = "InsanityBot 2020-2021"
				}
			};
			_ = InsanityBot.MessageLogger.LogMessage(new DiscordMessageBuilder
			{
				Embed = moderationEmbedBuilder.Build()
			}, ctx);
		}
		catch(Exception e)
		{
			embedBuilder = InsanityBot.Embeds["insanitybot.error"]
				.WithDescription(GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.clear_modlog.failure"], ctx, member));

			InsanityBot.Client.Logger.LogError($"{e}: {e.Message}");
		}
		finally
		{
			if(!silent)
			{
				await ctx.Channel?.SendMessageAsync(embed: embedBuilder.Build());
			}
		}
	}
}

public class ClearModlogOptions : ModerationOptionBase
{

}