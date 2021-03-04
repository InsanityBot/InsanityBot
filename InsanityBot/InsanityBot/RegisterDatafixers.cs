﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InsanityBot.Datafixers.Language;
using InsanityBot.Datafixers.Main;
using InsanityBot.Utility.Config;
using InsanityBot.Utility.Datafixers;
using InsanityBot.Utility.Language;

namespace InsanityBot
{
    public partial class InsanityBot
    {
        public static void RegisterDatafixers()
        {
            DataFixerLower.AddDatafixer(new Main0001_AddModlogScrolling(), typeof(MainConfiguration));
            DataFixerLower.AddDatafixer(new Main0002_AddChannelLocking(), typeof(MainConfiguration));

            DataFixerLower.AddDatafixer(new Lang0001_AddModlogScrolling(), typeof(LanguageConfiguration));
            DataFixerLower.AddDatafixer(new Lang0002_AddChannelLocking(), typeof(LanguageConfiguration));
        }
    }
}
