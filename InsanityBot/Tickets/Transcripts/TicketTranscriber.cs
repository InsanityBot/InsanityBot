﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace InsanityBot.Tickets.Transcripts
{
    public class TicketTranscriber
    {
        private readonly List<ITranscriber> Transcribers = new();

        public void RegisterTranscriber<T>() 
            where T : ITranscriber, new()
        {
            Transcribers.Add(new T());
        }

        public async Task Transcribe(DiscordTicket ticket)
        {
            foreach(var v in Transcribers)
            {
                await v.Transcribe(ticket);
            }
        }
    }
}
