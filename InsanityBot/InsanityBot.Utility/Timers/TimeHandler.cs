﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace InsanityBot.Utility.Timers
{
    public static class TimeHandler
    {
        public static void Start()
        {
            Countdown = new System.Timers.Timer
            {
                Interval = 250
            };
            Countdown.Elapsed += CountdownElapsed;

            Countdown.Start();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void CountdownElapsed(Object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Directory.Exists("./cache/timers"))
            {
                Directory.CreateDirectory("./cache/timers");
                return;
            }

            //knowing that it exists, proceed to read contents

            if (Directory.GetFiles("./cache/timers").Length == 0)
                return;

            //ok, it exists and has file contents. time to read.

            List<Timer> ActiveTimers = new();

            StreamReader reader;

            foreach (String s in Directory.GetFiles("./cache/timers"))
            {
                //keep this from throwing a fatal error
                //if an exception occurs, it just means the timer adding procedure took a little longer than usual
                try
                {
                    reader = new StreamReader(File.OpenRead(s));
                    ActiveTimers.Add(JsonConvert.DeserializeObject<Timer>(reader.ReadToEnd()));
                    reader.Close();
                }
                catch { }
            }

            foreach (Timer t in ActiveTimers)
            {
                if (t == null)
                    continue;
                if (!t.CheckExpiry())
                    continue;
                else
                    return;
            }

            Countdown.Start();
        }

        public static void AddTimer(Timer timer)
        {
            Countdown.Stop();

            StreamWriter writer;

            if (!File.Exists($"./cache/timers/{timer.Identifier}"))
                File.Create($"./cache/timers/{timer.Identifier}").Close();
            writer = new StreamWriter(File.Open($"./cache/timers/{timer.Identifier}", FileMode.Truncate));

            writer.Write(JsonConvert.SerializeObject(timer));

            writer.Close();

            Thread.Sleep(50);
            Countdown.Start();
        }

        public static void ReenableTimer()
        {
            Thread.Sleep(250);

            Countdown.Start();
        }

        public static void DisableTimer()
        {
            Countdown.Stop();
        }

        private static System.Timers.Timer Countdown { get; set; }
    }
}
