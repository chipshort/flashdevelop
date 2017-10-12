using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using CodeFormatter.Utilities;

namespace CodeFormatter.Preferences
{
    [Serializable]
    [Editor(typeof(FormatterStateEditor), typeof(UITypeEditor))]
    public class FormatterState
    {
        public string File { get; }
        public string Command { get; }
        public string Language { get; set; }

        public Dictionary<string, FormatterOption> Options { get; set; } = new Dictionary<string, FormatterOption>();
        public string[] AdditionalArgs { get; set; }

        public FormatterState(string file, string cmd)
        {
            File = file;
            Command = cmd;
        }

        public void Add(FormatterOption opt)
        {
            Options.Add(opt.Id, opt);
        }

        /// <summary>
        /// Helper method to create a list of flags from the currently selected options.
        /// </summary>
        public IEnumerable<string> ToOptions()
        {
            return AdditionalArgs.Concat(Options.Values.Select(o => FormatterHelper.ProcessArgument(o.Arg, id => Options[id].Value)));
        }

        public override string ToString()
        {
            return File;
        }
    }

    [Serializable]
    public class FormatterOption
    {
        public string Id { get; set; }
        public bool Enabled { get; set; }
        public string Arg { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    internal class CheckOption : FormatterOption
    {
        public bool State { get; set; }

        public CheckOption(string id, bool state, string arg, bool enabled)
        {
            Id = id;
            Arg = arg;
            State = state;
            Value = state.ToString();
            Enabled = enabled;
        }
    }

    [Serializable]
    internal class SelectOption : FormatterOption
    {
        public int Selection { get; set; }

        public SelectOption(string id, int selectedIndex, string selectedValue, string arg, bool enabled)
        {
            Id = id;
            Arg = arg;
            Selection = selectedIndex;
            Value = selectedValue;
            Enabled = enabled;
        }
    }

    [Serializable]
    internal class NumberOption : FormatterOption
    {
        public NumberOption(string id, int value, string arg, bool enabled)
        {
            Id = id;
            Arg = arg;
            Value = value.ToString();
            Enabled = enabled;
        }
    }
}
