
namespace CodeFormatter.Formatter
{
    interface IFormatter
    {
        string Name { get; }

        FormatterResult Format(string code);

        //Task<FormatterResult> FormatAsync(string code);

    }

    struct FormatterResult
    {
        /// <summary>
        /// True if the formatting failed. In that case <see cref="Result"/> contains the error message
        /// </summary>
        public bool Error;

        /// <summary>
        /// Contains the formatted code if the formatting succeeded or the error message if it failed.
        /// </summary>
        public string Result;

        public FormatterResult(string result, bool error)
        {
            Result = result;
            Error = error;
        }
    }
}
