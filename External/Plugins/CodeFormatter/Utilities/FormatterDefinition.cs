using System;
using CodeFormatter.Preferences;

namespace CodeFormatter.Utilities
{
    class FormatterDefinition
    {
        /// <summary>
        /// Unique identifier for this formatter
        /// </summary>
        //public string Id { get; set; }

        /// <summary>
        /// The type of this formatter (can be "$AStyle" or the command to run the formatter for now)
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The language this formatter works with
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// An array of categories with options this formatter supports
        /// </summary>
        public Category[] Categories { get; set; }

        /// <summary>
        /// Global example code, used if a category does not provide one (optional)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Array of additional arguments that should always be added
        /// </summary>
        public string[] AdditionalArgs { get; set; }

        public FormatterState ToFormatterState(string filename)
        {
            var state = new FormatterState(filename);
            state.AdditionalArgs = AdditionalArgs;

            foreach (var cat in Categories)
            {
                foreach (var opt in cat.Options)
                {
                    switch (opt.Type)
                    {
                        //TODO: process args (might need replacing
                        case "check":
                            var isChecked = opt.CheckData.DefaultValue;
                            state.Add(new CheckOption(opt.Id, isChecked, isChecked ? opt.CheckData.ArgChecked : opt.CheckData.ArgUnchecked, true));
                            break;
                        case "select":
                            var selected = opt.SelectData[0];
                            state.Add(new SelectOption(opt.Id, 0, selected.Value, selected.Arg, true));
                            break;
                        case "number":
                            state.Add(new NumberOption(opt.Id, opt.NumberData.DefaultValue, opt.NumberData.Arg, true));
                            break;
                    }
                }
            }

            return state;
        }
    }

    class Category
    {
        /// <summary>
        /// The text shown in the UI element
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Example code for this category. This is used instead of the global code in <see cref="FormatterDefinition"/> (optional)
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// An array of options in this category
        /// </summary>
        public FormatOption[] Options { get; set; }
    }

    class FormatOption
    {
        /// <summary>
        /// Unique identifier for this option
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The text shown in the UI element
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of option this is. One of the following:
        /// "check", "select" or "number"
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The data of this option if it is of type "check" (optional otherwise)
        /// </summary>
        public Check CheckData { get; set; }
        /// <summary>
        /// The data of this option if it is of type "select" (the first element is the default selection) (optional otherwise)
        /// </summary>
        public SelectData[] SelectData { get; set; }
        /// <summary>
        /// The data of this option if it is of type "number" (optional otherwise)
        /// </summary>
        public Number NumberData { get; set; }
    }

    class Number
    {
        public int Min { get; set; }
        public int Max { get; set; }

        /// <summary>
        /// The default value for this number option
        /// </summary>
        public int DefaultValue { get; set; }
        /// <summary>
        /// The argument this number should be inserted into. (optional)
        /// </summary>
        public string Arg { get; set; }
    }

    class Check
    {
        /// <summary>
        /// An array of ids of options that should be unchecked if this SelectData is selected. They have to be of type "check". (optional)
        /// </summary>
        public string[] Unchecks { get; set; }
        //public string[] Checks { get; set; } // maybe?

        /// <summary>
        /// The argument that should be added if this option is checked. (optional)
        /// </summary>
        public string ArgChecked { get; set; }
        /// <summary>
        /// The argument that should be added if this option is not checked. (optional)
        /// </summary>
        public string ArgUnchecked { get; set; }

        /// <summary>
        /// The default state of this option.
        /// </summary>
        public bool DefaultValue { get; set; }

        /// <summary>
        /// An array of options that are suboptions of this one.
        /// This means, they are only available if the parent option is checked too.
        /// </summary>
        public FormatOption[] Suboptions { get; set; }
    }

    class SelectData
    {
        /// <summary>
        /// The text shown in the UI element
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// This will be inserted in place of ${ID_OF_SELECT} in arguments (optional, unless it is actually used)
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The argument to add if this SelectData is selected (optional)
        /// </summary>
        public string Arg { get; set; }
        /// <summary>
        /// An array of ids of options that should be disabled if this SelectData is selected (optional)
        /// </summary>
        public string[] Disables { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
