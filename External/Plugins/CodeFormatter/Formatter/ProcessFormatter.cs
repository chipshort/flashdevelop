using System.Diagnostics;
using System.IO;
using CodeFormatter.Preferences;
using CodeFormatter.Utilities;

namespace CodeFormatter.Formatter
{
    class ProcessFormatter : IFormatter
    {
        readonly FormatterState formatter;

        public string Name { get; }

        public ProcessFormatter(FormatterState state)
        {
            formatter = state;
            Name = state.File;
        }

        public FormatterResult Format(string code)
        {
            var options = string.Join(" ", formatter.ToOptions());

            var formatterFolder = FormatterHelper.GetFormatterFolder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(formatter.Command)
                {
                    Arguments = options,
                    UseShellExecute = !File.Exists(formatter.Command) &&
                                      !File.Exists(Path.Combine(formatterFolder, formatter.Command)),
                    CreateNoWindow = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = formatterFolder
                }
            };

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
            {
                return new FormatterResult(error, true);
            }

            return new FormatterResult(output, false);
        }
    }
}
