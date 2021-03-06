﻿using System;
using System.Drawing;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Web.Client.Utils
{
    public static class InvictusColours
    {
        public static Color ICAP => Color.FromArgb(1, 207, 33, 39);

        public static Color IBA => Color.FromArgb(1, 246, 122, 6);

        public static Color IML => Color.FromArgb(1, 4, 148, 208);

        public static Color IHF => Color.FromArgb(1, 19, 55, 117);

        public static Color IGP => Color.FromArgb(1, 199, 130, 42);

        public static Color C10 => Color.FromArgb(1, 14, 108, 172);

        public static Color C20 => Color.FromArgb(1, 12, 59, 89);

        public static Color EMS => Color.FromArgb(1, 251, 173, 24);

        public static Color GetByFund(Symbol symbol)
        {
            return symbol switch
            {
                Symbol.C10 => C10,
                Symbol.C20 => C20,
                Symbol.IBA => IBA,
                Symbol.IML => IML,
                Symbol.IHF => IHF,
                Symbol.IGP => IGP,
                Symbol.EMS => EMS,
                Symbol.ICAP => ICAP,
                _ => RandomColor(),
            };
        }

        public static Color RandomColor()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());

            return Color.FromArgb(1, r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
        }
    }
}
