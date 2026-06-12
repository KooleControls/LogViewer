using System.Text;

namespace LogViewer.Devices.Gateway.Kc2
{
    /// <summary>
    /// A reply to a KC2 command. The device answers a command "XYZW" with a frame
    /// "R" + "YZW" + ackByte + args, where ackByte is 0x06 (ACK) or 0x15 (NAK).
    /// </summary>
    public class Kc2Reply
    {
        public const byte Ack = 0x06;
        public const byte Nak = 0x15;

        private readonly byte[] data;

        public Kc2Reply(byte[] rawFrame)
        {
            data = rawFrame ?? Array.Empty<byte>();
        }

        /// <summary>True when the frame is at least the 5-byte header (R + 3 tag + ack).</summary>
        public bool IsValid => data.Length >= 5 && data[0] == (byte)'R';

        public bool IsAck => IsValid && data[4] == Ack;
        public bool IsNak => IsValid && data[4] == Nak;

        /// <summary>The 3-byte tag identifying which command this reply belongs to (cmd[1..3]).</summary>
        public byte[] Tag => IsValid ? new[] { data[1], data[2], data[3] } : Array.Empty<byte>();

        /// <summary>The argument bytes after the ack byte, or empty.</summary>
        public byte[] Args
        {
            get
            {
                if (data.Length <= 5)
                    return Array.Empty<byte>();
                byte[] args = new byte[data.Length - 5];
                Array.Copy(data, 5, args, 0, args.Length);
                return args;
            }
        }

        public string ArgsAsString => Encoding.ASCII.GetString(Args);

        /// <summary>Does this reply correspond to the given outbound command bytes?</summary>
        public bool MatchesCommand(byte[] command)
        {
            return IsValid
                && command.Length >= 4
                && data[1] == command[1]
                && data[2] == command[2]
                && data[3] == command[3];
        }
    }
}
