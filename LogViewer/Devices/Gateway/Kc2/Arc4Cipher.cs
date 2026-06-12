using System.Text;

namespace LogViewer.Devices.Gateway.Kc2
{
    /// <summary>
    /// KC-specific ARC4/RC4 stream cipher used by the KC2 TCP protocol.
    /// Note: this is NOT standard RC4 - the state array is 128 bytes (mod 128), not 256.
    ///
    /// The cipher is stateless per call: each <see cref="Process"/> re-keys from scratch, so the
    /// same keystream is produced every time. Because it is a symmetric XOR, encrypt == decrypt.
    /// The session keystream depends only on (publicKey, privateKey):
    ///   1. derive an 8-byte session key by RC4'ing the public key with the private key,
    ///   2. RC4 the payload with that derived key.
    /// Ported from the proven KCObjects.Arc4Encryption implementation.
    /// </summary>
    public class Arc4Cipher
    {
        private readonly byte[] privateKey = new byte[10];
        private readonly byte[] publicKey = new byte[8];

        /// <summary>The default device private key (PRIV_KEY_DEFAULT_KC), equal to setting "000000".</summary>
        public static byte[] DefaultPrivateKey => new byte[] { 0x46, 0x12, 0x2F, 0x5E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };

        public Arc4Cipher()
        {
            DefaultPrivateKey.CopyTo(privateKey, 0);
        }

        public Arc4Cipher(byte[] privKey)
        {
            SetPrivateKey(privKey);
        }

        /// <summary>Set the 8-byte public key received from the device on connect.</summary>
        public void SetPublicKey(byte[] pubKey)
        {
            if (pubKey != null && pubKey.Length >= 8)
                Array.Copy(pubKey, publicKey, 8);
        }

        public void SetPrivateKey(string installationCode)
        {
            if (installationCode != null)
                SetPrivateKey(Encoding.ASCII.GetBytes(installationCode));
        }

        public void SetPrivateKey(byte[] privKey)
        {
            if (privKey == null)
                return;

            if (privKey.Length == 4)
            {
                privKey.CopyTo(privateKey, 0);
            }
            else if (privKey.Length == 6)
            {
                // Same fixed prefix the firmware/220-tool use for a 6-char installation code.
                privateKey[0] = 0x46;
                privateKey[1] = 0x12;
                privateKey[2] = 0x2F;
                privateKey[3] = 0x5E;
                privateKey[4] = privKey[0];
                privateKey[5] = privKey[1];
                privateKey[6] = privKey[2];
                privateKey[7] = privKey[3];
                privateKey[8] = privKey[4];
                privateKey[9] = privKey[5];
            }
            else if (privKey.Length == 10)
            {
                privKey.CopyTo(privateKey, 0);
            }
        }

        /// <summary>Key scheduling algorithm over a 128-byte state.</summary>
        private static void Setup(byte[] m, byte[] key, int length)
        {
            for (int i = 0; i < 128; i++)
                m[i] = (byte)i;

            int j = 0, k = 0;
            for (int i = 0; i < 128; i++)
            {
                int a = m[i];
                j = (j + a + key[k]) % 128;
                m[i] = m[j];
                m[j] = (byte)a;
                if (++k >= length)
                    k = 0;
            }
        }

        /// <summary>Pseudo-random generation algorithm; XORs the keystream into <paramref name="data"/>.</summary>
        private static void Crypt(byte[] m, byte[] data, int length)
        {
            int x = 0, y = 0;
            for (int i = 0; i < length; i++)
            {
                x = (x + 1) % 128;
                int a = m[x];
                y = (y + a) % 128;
                int b = m[y];
                m[x] = (byte)b;
                m[y] = (byte)a;
                data[i] ^= m[(a + b) % 128];
            }
        }

        /// <summary>
        /// Encrypt or decrypt <paramref name="data"/> in place (symmetric).
        /// </summary>
        public void Process(byte[] data, int length)
        {
            byte[] m = new byte[128];

            // Derive the session key from the public key using the private key.
            byte[] sessionKey = new byte[8];
            Array.Copy(publicKey, sessionKey, 8);
            Setup(m, privateKey, 10);
            Crypt(m, sessionKey, 8);

            // Encrypt the payload with the derived session key.
            Setup(m, sessionKey, 8);
            Crypt(m, data, length);
        }

        /// <summary>Returns a new array containing the processed (encrypted/decrypted) bytes.</summary>
        public byte[] Process(byte[] data)
        {
            byte[] copy = (byte[])data.Clone();
            Process(copy, copy.Length);
            return copy;
        }
    }
}
