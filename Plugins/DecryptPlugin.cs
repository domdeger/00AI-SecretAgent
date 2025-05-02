using System.Text;
using Microsoft.SemanticKernel;

public class DecryptPlugin
{
    [KernelFunction]
    public IEnumerable<string> GetWordsWithLength(string text, int length) =>
        text.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length == length);

    [KernelFunction]
    public int CharFrequency(string text, char character) => text.Count(c => c == character);

    [KernelFunction]
    public string DecryptCaesarCipher(string cipherText, int shift)
    {
        StringBuilder decryptedText = new();

        foreach (char c in cipherText)
        {
            if (char.IsLetter(c))
            {
                char baseChar = char.IsUpper(c) ? 'A' : 'a';
                // Modulo operation handles wrapping correctly, including negative shifts
                int shifted = (c - baseChar - shift) % 26;
                if (shifted < 0)
                {
                    shifted += 26; // Ensure positive result for modulo
                }
                decryptedText.Append((char)(baseChar + shifted));
            }
            else
            {
                decryptedText.Append(c); // Keep non-alphabetic characters as they are
            }
        }

        return decryptedText.ToString();
    }

    [KernelFunction]
    public string DecryptReverseCipher(string cipherText)
    {
        char[] charArray = cipherText.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
