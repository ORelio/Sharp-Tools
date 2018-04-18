using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SharpTools
{
    /// <summary>
    /// Allows to localize the app in different languages
    /// </summary>
    /// <remarks>
    /// By ORelio (c) 2015-2018 - CDDL 1.0
    /// </remarks>
    public static class Translations
    {
        private static Dictionary<string, string> translations;

        /// <summary>
        /// Return a tranlation for the requested text
        /// </summary>
        /// <param name="msg_name">text identifier</param>
        /// <returns>returns translation for this identifier</returns>
        public static string Get(string msg_name)
        {
            if (translations.ContainsKey(msg_name))
                return translations[msg_name];

            return msg_name.ToUpper();
        }

        /// <summary>
        /// Initialize translations depending on system language.
        /// English is the default for all unknown system languages.
        /// </summary>
        static Translations()
        {
            translations = new Dictionary<string, string>();

            /*
             * External translation files
             * These files are loaded from the installation directory as:
             * Lang/abc.ini, e.g. Lang/eng.ini which is the default language file
             * Useful for adding new translations of fixing typos without recompiling
             */

            string systemLanguage = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
            string langDir = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Lang" + Path.DirectorySeparatorChar;
            string langFileSystemLanguage = langDir + systemLanguage + ".ini";
            string langFile = File.Exists(langFileSystemLanguage) ? langFileSystemLanguage : langDir + "eng.ini";

            if (File.Exists(langFile))
            {
                foreach (string lineRaw in File.ReadAllLines(langFile, Encoding.UTF8))
                {
                    //This only handles a subset of the INI format:
                    //key=value pairs, no sections, no inline comments.
                    string line = lineRaw.Trim();
                    string translationName = line.Split('=')[0];
                    if (line.Length > (translationName.Length + 1))
                    {
                        string translationValue = line.Substring(translationName.Length + 1);
                        translations[translationName] = translationValue;
                    }
                }
            }

            /* 
             * Hardcoded translation data
             * This data is used as fallback if no translation file could be loaded
             * Useful for standalone exe portable apps
             */

            else if (systemLanguage == "fra")
            {
                translations["about"] = "A Propos";
                //Ajouter de nouvelles traductions ici
            }
            //Add new languages here as 'else if' blocks
            //English is the default language in 'else' block below
            else
            {
                translations["about"] = "About";
                //Add new translations here
            }
        }
    }
}