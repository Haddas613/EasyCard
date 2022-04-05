using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shared.Helpers.IO
{
    public static class FileNameHelpers
    {
        private static readonly char[] InvalidFilenameCharacters = Path.GetInvalidFileNameChars();

        public static string RemoveIllegalFilenameCharacters(string input)
        {
            Span<char> outputSpan = stackalloc char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (!InvalidFilenameCharacters.Contains(input[i]))
                {
                    outputSpan[i] = input[i];
                }
                else
                {
                    outputSpan[i] = '_';
                }
            }

            return new string(outputSpan);
        }
    }
}
