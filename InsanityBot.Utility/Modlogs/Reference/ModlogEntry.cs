﻿using System;

namespace InsanityBot.Utility.Modlogs.Reference
{
    public struct ModlogEntry
    {
        public ModlogEntryType Type { get; set; }

        public DateTimeOffset Time { get; set; }

        public String Reason { get; set; }

        public override Boolean Equals(Object obj)
        {
            if(((ModlogEntry)obj).Time == this.Time)
            {
                return true;
            }

            return false;
        }

        public override Int32 GetHashCode() => base.GetHashCode();

        public static Boolean operator ==(ModlogEntry left, ModlogEntry right) => left.Equals(right);

        public static Boolean operator !=(ModlogEntry left, ModlogEntry right) => !(left == right);
    }
}
