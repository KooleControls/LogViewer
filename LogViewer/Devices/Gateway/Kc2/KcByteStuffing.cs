namespace LogViewer.Devices.Gateway.Kc2
{
    /// <summary>
    /// Byte-stuffing for the KC2 protocol. Special characters ("&lt;", "&gt;", "#") are escaped
    /// using "#" as the escape character, encoded as escape + index-of-special-char.
    /// Ported from the proven KCObjects.KCStuffing implementation.
    /// </summary>
    public static class KcByteStuffing
    {
        // The set of characters that must be stuffed. Index 2 ('#') is the escape character.
        private static readonly byte[] StuffChars = { (byte)'<', (byte)'>', (byte)'#' };
        private const int EscapeIndex = 2;

        /// <summary>Stuff a payload so it contains no raw framing characters.</summary>
        public static byte[] Stuff(byte[] buffer)
        {
            int needed = CalculateNeededSpace(buffer);
            byte[] target = new byte[needed];
            int count = 0;

            foreach (byte b in buffer)
            {
                int found = -1;
                for (int j = 0; j < StuffChars.Length; j++)
                {
                    if (StuffChars[j] == b)
                        found = j;
                }

                if (found != -1)
                {
                    target[count] = StuffChars[EscapeIndex];
                    if (count < target.Length - 1) count++;
                    target[count] = (byte)found;
                }
                else
                {
                    target[count] = b;
                }
                if (count < target.Length - 1) count++;
            }

            return target;
        }

        /// <summary>Reverse the stuffing of a framed payload.</summary>
        public static byte[] UnStuff(byte[] buffer)
        {
            byte[] target = new byte[buffer.Length];
            int count = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == StuffChars[EscapeIndex])
                {
                    i++;
                    if (i < buffer.Length)
                    {
                        if (buffer[i] < StuffChars.Length)
                        {
                            if (count < target.Length)
                                target[count++] = StuffChars[buffer[i]];
                        }
                    }
                }
                else
                {
                    if (count < target.Length)
                        target[count++] = buffer[i];
                }
            }

            Array.Resize(ref target, count);
            return target;
        }

        private static int CalculateNeededSpace(byte[] buffer)
        {
            int count = 0;
            foreach (byte b in buffer)
            {
                foreach (byte stuff in StuffChars)
                {
                    if (stuff == b)
                        count++;
                }
                count++;
            }
            return count;
        }
    }
}
