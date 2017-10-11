using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeFormatter.Dialogs;
using CodeFormatter.Preferences;
using LitJson;
using PluginCore.Helpers;

namespace CodeFormatter.Utilities
{
    class FormatterHelper
    {
        /// <summary>
        /// Helper method to create a list of flags from the saved <see cref="FormatterState"/>.
        /// </summary>
        internal static List<string> GetOptions(FormatterState state)
        {
            return state.Options.Select(o => o.Value.Arg).Concat(state.AdditionalArgs).ToList();
        }
        
        /// <summary>
        /// Creates an enumerable of formatter names (only the file names without extension)
        /// </summary>
        internal static IEnumerable<string> FindFormatters()
        {
            var formatterDir = Path.Combine(PathHelper.DataDir, "CodeFormatter", "Formatters");

            if (!Directory.Exists(formatterDir)) Directory.CreateDirectory(formatterDir);

            foreach (var file in Directory.GetFiles(formatterDir, "*.json"))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        internal static FormatterDefinition ParseFormatter(string name)
        {
            var formatterDir = Path.Combine(PathHelper.DataDir, "CodeFormatter", "Formatters");

            //Load formatter definition
            FormatterDefinition def;
            using (TextReader reader = File.OpenText(Path.Combine(formatterDir, name + ".json")))
            {
                def = JsonMapper.ToObject<FormatterDefinition>(reader);
            }

            return def;
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
