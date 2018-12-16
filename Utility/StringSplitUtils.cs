using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTools
{
    /// <summary>
    /// Utility methods for parsing strings using delimiters
    /// </summary>
    /// <remarks>
    /// By ORelio (c) 2016-2018 - CDDL 1.0
    /// </remarks>
    class StringSplitUtils
    {
        /// <summary>
        /// Split a string using the specified delimiter
        /// </summary>
        /// <param name="toSplit">String to split</param>
        /// <param name="delimiter">Delimiter</param>
        /// <returns>Splitted string</returns>
        public static string[] SplitStringWithDelimiter(string toSplit, string delimiter)
        {
            if (String.IsNullOrEmpty(toSplit) || String.IsNullOrEmpty(delimiter))
                return new string[] { };
            return toSplit.Split(new string[] { delimiter }, StringSplitOptions.None);
        }

        /// <summary>
        /// Find the first occurence of a substring in a string to parse using a start and end delimiter
        /// </summary>
        /// <param name="toParse">String to parse</param>
        /// <param name="startDelimiter">Start delimiter</param>
        /// <param name="endDelimiter">End delimiter</param>
        /// <returns>First occurence of substring or null if not found</returns>
        public static string FindStringWithDelimiters(string toParse, string startDelimiter, string endDelimiter)
        {
            if (String.IsNullOrEmpty(toParse) || String.IsNullOrEmpty(startDelimiter))
                return null;

            string[] splittedStart = SplitStringWithDelimiter(toParse, startDelimiter);

            if (splittedStart.Length < 2)
                return null;

            string[] splittedEnd = SplitStringWithDelimiter(splittedStart[1], endDelimiter);

            if (splittedEnd.Length < 2)
                return null;

            return splittedEnd[0];
        }

        /// <summary>
        /// Find the first occurence of a substring in a string to parse using several start delimiters and one end delimiter
        /// </summary>
        /// <example>
        /// inputString = "[u]word1[/u][b][u]word2[/u][/b][b][u]word3[/u][/b]"
        /// FindStringWithDelimiters(inputString, "[u]") throws ArgumentException
        /// FindStringWithDelimiters(inputString, "[u]", "[/u]") returns "word1"
        /// FindStringWithDelimiters(inputString, "[b]", "[u]", "[/u]") returns "word2"
        /// FindStringWithDelimiters(inputString, "[/b]", "[u]", "[/u]") returns "word3"
        /// </example>
        /// <param name="toParse">String to parse</param>
        /// <param name="delimiters">One or more start delimiters, and one end delimiter</param>
        /// <returns>First occurence of substring or null if not found</returns>
        public static string FindStringWithDelimiters(string toParse, params string[] delimiters)
        {
            if (delimiters.Length < 2)
                throw new ArgumentException("Must provide at least 2 delimiters.", "delimiters");

            Queue<string> delims = new Queue<string>(delimiters);

            while (delims.Count > 1)
            {
                string[] splittedStart = SplitStringWithDelimiter(toParse, delims.Dequeue());

                if (splittedStart.Length < 2)
                    return null;

                toParse = splittedStart[1];
            }

            string[] splittedEnd = SplitStringWithDelimiter(toParse, delims.Dequeue());

            if (splittedEnd.Length < 2)
                return null;

            return splittedEnd[0];
        }
    }
}
