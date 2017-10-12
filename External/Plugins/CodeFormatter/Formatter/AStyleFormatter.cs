using System.Collections.Generic;

namespace CodeFormatter.Formatter
{
    class AStyleFormatter : IFormatter
    {
        readonly string options;

        public string Name { get; }

        public AStyleFormatter(string opts, string name = "AStyle")
        {
            options = opts;
            Name = name;
        }

        public AStyleFormatter(IEnumerable<string> opts, string name = "AStyle") : this(string.Join(" ", opts), name)
        {
        }

        public FormatterResult Format(string code)
        {
            var asi = new AStyleInterface();

            var resultData = asi.FormatSource(code, options);
            //TODO: detect errors
            return new FormatterResult(resultData == string.Empty ? null : resultData, false);
        }
    }
}
