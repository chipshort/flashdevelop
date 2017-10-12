using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using PluginCore.Helpers;

namespace CodeFormatter.Utilities
{
    class FormatterHelper
    {
        /// <summary>
        /// Creates an enumerable of formatter names (only the file names without extension)
        /// </summary>
        internal static IEnumerable<string> FindFormatters()
        {
            var formatterDir = GetFormatterFolder();

            foreach (var file in Directory.GetFiles(formatterDir, "*.json"))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        internal static FormatterDefinition ParseFormatter(string name)
        {
            var formatterDir = GetFormatterFolder();

            //Load formatter definition
            FormatterDefinition def;
            using (TextReader reader = File.OpenText(Path.Combine(formatterDir, name + ".json")))
            {
                def = JsonMapper.ToObject<FormatterDefinition>(reader);
            }

            return def;
        }

        /// <summary>
        /// Gets the folder where formatters are stored and makes sure it exists
        /// </summary>
        internal static string GetFormatterFolder()
        {
            var formatterDir = Path.Combine(PathHelper.DataDir, "CodeFormatter", "Formatters");

            if (!Directory.Exists(formatterDir))
                Directory.CreateDirectory(formatterDir);

            return formatterDir;
        }

        internal static string ProcessArgument(string arg, Func<string, string> getMapping)
        {
            if (arg == null) return null;

            var replacer = new Regex("\\${([A-z]+)}");

            return replacer.Replace(arg, m =>
            {
                var id = m.Groups[1].Value;

                return getMapping(id);
            });
        }
    }
}
