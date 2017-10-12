using CodeFormatter.Handlers;
using CodeFormatter.Utilities;

namespace CodeFormatter.Formatter
{
    class ASFormatter : IFormatter
    {
        readonly bool isComplete;
        readonly Settings settings;

        public string Name { get; } = "FlashDevelop - ASPrettyPrinter";

        public ASFormatter(bool isCompleteFile, Settings settingsObj)
        {
            isComplete = isCompleteFile;
            settings = settingsObj;
        }

        public FormatterResult Format(string code)
        {
            var asPrinter = new ASPrettyPrinter(isComplete, code);
            FormatUtility.configureASPrinter(asPrinter, settings);
            var resultData = asPrinter.print(0);
            var error = resultData == null;
            
            return new FormatterResult(resultData, error);
        }
    }
}
