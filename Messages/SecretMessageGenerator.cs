using System.Text;

namespace _00AI.Messages
{
    public static class SecretMessageGenerator
    {
        private static readonly List<string> Villains =
        [
            "Dr. NoCode",
            "Bluescreen Spectre",
            "Kernel Panic",
            "Syntax Terror",
            "Null Pointer Nightmare",
            "Firewall Fiend",
            "Trojan Horseman",
            "Malware Maven",
            "Phishing Phantom", // Added comma
        ];

        private static readonly Random random = new();

        // Define delegates for encryption functions
        private delegate string EncryptionAlgorithm(string text, int? key = null);

        // List of available encryption algorithms
        private static readonly List<(string Name, EncryptionAlgorithm Algorithm)> Algorithms =
        [
            ("Caesar Cipher", CaesarCipher),
            ("Reverse Cipher", ReverseCipher),
        ];

        public static string GenerateSecretMessage()
        {
            // Select sender and recipient
            int senderIndex = random.Next(Villains.Count);
            int recipientIndex;
            do
            {
                recipientIndex = random.Next(Villains.Count);
            } while (recipientIndex == senderIndex);

            string sender = Villains[senderIndex];
            string recipient = Villains[recipientIndex];

            // Generate a simple message (can be expanded later)
            string originalMessage =
                $"Meeting at the usual place. Bring the source code. Don't be late!";

            // Randomly select an encryption algorithm
            int algorithmIndex = random.Next(Algorithms.Count);
            var (Name, Algorithm) = Algorithms[algorithmIndex];

            // Encrypt the message using the selected algorithm
            string encryptedMessage;
            int? key = null;
            if (Name == "Caesar Cipher")
            {
                key = random.Next(1, 26); // Random shift for Caesar
                encryptedMessage = Algorithm(originalMessage, key);
            }
            else
            {
                encryptedMessage = Algorithm(originalMessage);
            }

            // Format the output string including the algorithm name
            return $"From: {sender}\nTo: {recipient}\nAlgorithm: {Name}{(key.HasValue ? $" (Key: {key})" : "")}\nMessage: {encryptedMessage}";
        }

        // Existing Caesar Cipher (modified to fit delegate signature)
        private static string CaesarCipher(string text, int? key)
        {
            if (!key.HasValue)
                throw new ArgumentNullException(nameof(key), "Caesar cipher requires a key.");
            int shift = key.Value;
            StringBuilder result = new StringBuilder();

            foreach (char character in text)
            {
                if (char.IsLetter(character))
                {
                    char d = char.IsUpper(character) ? 'A' : 'a';
                    result.Append((char)((((character + shift) - d) % 26) + d));
                }
                else
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }

        // New Reverse Cipher
        private static string ReverseCipher(string text, int? key = null) // Key ignored
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
