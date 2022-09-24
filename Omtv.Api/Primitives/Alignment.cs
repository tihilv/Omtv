using System;

namespace Omtv.Api.Primitives
{
    public enum Alignment: byte
    {
        Before = 0,
        Center = 1,
        After = 2,
    }

    public static class AlignmentExtensions
    {
        public static Alignment? Parse(String? value)
        {
            if (value == null)
                return null;

            return Enum.Parse<Alignment>(value, true);
        }
    }
}