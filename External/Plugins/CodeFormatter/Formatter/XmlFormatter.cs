using CodeFormatter.Handlers;
using CodeFormatter.Utilities;

namespace CodeFormatter.Formatter
{
    class XmlFormatter : IFormatter
    {
        readonly Settings settings;

        public string Name { get; } = "FlashDevelop - (M)XMLFormatter";

        public XmlFormatter(Settings settingsObj)
        {
            settings = settingsObj;
        }

        public FormatterResult Format(string code)
        {
            MXMLPrettyPrinter mxmlPrinter = new MXMLPrettyPrinter(code);
            FormatUtility.configureMXMLPrinter(mxmlPrinter, settings);
            var resultData = mxmlPrinter.print(0);
            var error = resultData == null;

            return new FormatterResult(resultData, error);
        }
    }
}
