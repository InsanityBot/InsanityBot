﻿using CommandLine;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using InsanityBot.Core.Services.Internal.Modlogs;
using InsanityBot.Utility.Modlogs.Reference;
using InsanityBot.Utility.Modlogs.SafeAccessInterface;
using InsanityBot.Utility.Permissions;
using InsanityBot.Utility.Timers;

using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading.Tasks;

using static InsanityBot.Commands.StringUtilities;
using static System.Convert;

namespace InsanityBot.Commands.Moderation
{
    public partial class Mute : BaseCommandModule
    {
        [Command("tempmute")]
        [Aliases("temp-mute")]
        [Description("Temporarily mutes an user.")]
        public async Task TempmuteCommand(CommandContext ctx,

            [Description("The user to mute")]
            DiscordMember member,

            [Description("Duration of the mute")]
            String time,

            [Description("Reason of the mute")]
            [RemainingText]
            String Reason = "usedefault")
        {
            if(time.StartsWith('-'))
            {
                await this.ParseTempmuteCommand(ctx, member, String.Join(' ', time, Reason));
                return;
            }
            await this.ExecuteTempmuteCommand(ctx, member,
                                time.ParseTimeSpan(TemporaryPunishmentType.Mute),
                                Reason, false, false);
        }

        private async Task ParseTempmuteCommand(CommandContext ctx,
            DiscordMember member,
            String arguments)
        {
            String cmdArguments = arguments;
            try
            {
                if(!arguments.Contains("-r") && !arguments.Contains("--reason"))
                {
                    cmdArguments += " --reason usedefault";
                }

                await Parser.Default.ParseArguments<TempmuteOptions>(cmdArguments.Split(' '))
                    .WithParsedAsync(async o =>
                    {
                        await this.ExecuteTempmuteCommand(ctx, member,
                                o.Time.ParseTimeSpan(TemporaryPunishmentType.Mute),
                                String.Join(' ', o.Reason), o.Silent, o.DmMember);
                    });
            }
            catch(Exception e)
            {
                DiscordEmbedBuilder failed = new()
                {
                    Description = GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.tempmute.failure"],
                        ctx, member),
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "InsanityBot 2020-2021"
                    }
                };
                InsanityBot.Client.Logger.LogError($"{e}: {e.Message}");

                await ctx.Channel.SendMessageAsync(embed: failed.Build());
            }
        }

        private async Task ExecuteTempmuteCommand(CommandContext ctx,
            DiscordMember member,
            TimeSpan time,
            String Reason,
            Boolean Silent,
            Boolean DmMember)
        {
            if(!ctx.Member.HasPermission("insanitybot.moderation.tempmute"))
            {
                await ctx.Channel.SendMessageAsync(InsanityBot.LanguageConfig["insanitybot.error.lacking_permission"]);
                return;
            }

            String MuteReason = Reason switch
            {
                "usedefault" => GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.no_reason_given"],
                                ctx, member),
                _ => GetFormattedString(Reason, ctx, member)
            };

            DiscordEmbedBuilder embedBuilder = null;

            DiscordEmbedBuilder moderationEmbedBuilder = new()
            {
                Title = "TEMPMUTE",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "InsanityBot 2020-2021"
                }
            };

            moderationEmbedBuilder.AddField("Moderator", ctx.Member.Mention, true)
                .AddField("Member", member.Mention, true)
                .AddField("Duration", time.ToString(), true)
                .AddField("Reason", MuteReason, true);

            try
            {
                MuteStartingEvent();

                Timer callbackTimer = new(DateTime.Now.Add(time), $"tempmute_{member.Id}");
                moderationEmbedBuilder.AddField("Timer GUID", callbackTimer.Guid.ToString(), true);
                TimeHandler.AddTimer(callbackTimer);

                _ = member.TryAddModlogEntry(ModlogEntryType.mute, MuteReason);
                embedBuilder = new DiscordEmbedBuilder
                {
                    Description = GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.mute.success"],
                        ctx, member),
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "InsanityBot 2020-2021"
                    }
                };
                _ = member.GrantRoleAsync(InsanityBot.HomeGuild.GetRole(
                    ToUInt64(InsanityBot.Config["insanitybot.identifiers.moderation.mute_role_id"])),
                    MuteReason);
                _ = InsanityBot.ModlogQueue.QueueMessage(ModlogMessageType.Moderation, new DiscordMessageBuilder
                {
                    Embed = moderationEmbedBuilder
                });

            }
            catch
            {
                embedBuilder = new DiscordEmbedBuilder
                {
                    Description = GetFormattedString(InsanityBot.LanguageConfig["insanitybot.moderation.mute.failure"], ctx, member),
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "InsanityBot 2020-2021"
                    }
                };
            }
            finally
            {
                if(embedBuilder == null)
                {
                    InsanityBot.Client.Logger.LogError(new EventId(1131, "Tempmute"),
                        "Could not execute tempmute command, an unknown exception occured.");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());
                }
            }
        }


        public static void InitializeUnmute(String Identifier, Guid guid)
        {
            if(!Identifier.StartsWith("tempmute_"))
            {
                return;
            }

            try
            {
                File.Delete($"./cache/timers/{Identifier}");

                new Mute().ExecuteUnmuteCommand(null, GetMember(Identifier),
                    true, false, true, "timer_guid", guid).GetAwaiter().GetResult();

                UnmuteCompletedEvent();
            }
            catch(Exception e)
            {
                InsanityBot.Client.Logger.LogError(new EventId(1132, "Unmute"), $"Could not unmute user {Identifier[9..]}");
                System.Console.WriteLine($"{e}: {e.Message}\n{e.StackTrace}");
            }
        }

        private static DiscordMember GetMember(String Identifier)
        {
            Task<DiscordMember> thing = InsanityBot.HomeGuild.GetMemberAsync(ToUInt64(Identifier[9..]));
            return thing.GetAwaiter().GetResult();
        }

        public static event TimedActionCompleteEventHandler UnmuteCompletedEvent;
        public static event TimedActionStartEventHandler MuteStartingEvent;
    }

    public class TempmuteOptions : ModerationOptionBase
    {
        [Option('t', "time", Default = "default", Required = false)]
        public String Time { get; set; }
    }
}
