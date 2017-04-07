using PluginCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LintingHelper
{
    public delegate void LintCallback(List<LintingResult> results);

    public interface ILintProvider
    {
        /// <summary>
        /// Lint the given files. Always make sure to call the callback, even if no results were found.
        /// Pass null in this case.
        /// </summary>
        void LintAsync(string[] files, LintCallback callback);
    }
}
