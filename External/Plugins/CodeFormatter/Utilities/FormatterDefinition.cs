using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFormatter.Utilities
{
    class FormatterDefinition
    {
        /**
	     * The type of this formatter (can be "AStyle" or the command to run the formatter for now)
	     */
        public string Type { get; set; }
	
        /**
         * The language this formatter works with
         */
        public string Language { get; set; }

        public Category[] Categories { get; set; }

        /**
         * Global example code, used if a category does not provide one
         */
        public string Code { get; set; }
    }

    class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public FormatOption[] Options { get; set; }
    }

    class FormatOption //T can be Number, Check or Select
    {
        public string Name { get; set; }
        public string Type { get; set; } //"check | select | number",
        public Check CheckData { get; set; }
        public Select SelectData { get; set; }
        public Number NumberData { get; set; }
    }

    class Number
    {
        public int Min { get; set; } //2 for tab width
        public int Max { get; set; } //20 for tab width
        public int DefaultValue { get; set; }
        /// <summary>
        /// The argument this number should be inserted into. If this number is used elsewhere, this will be null
        /// </summary>
        public string Arg { get; set; }
    }

    class Check
    {
        public string[] Unchecks { get; set; }
        //public var checks: Array<String>; // maybe?

        public string ArgEnabled { get; set; }
        public string ArgDisabled { get; set; } //optional, if it does not exist, no argument is passed
        public bool DefaultValue { get; set; }

        public FormatOption[] Suboptions { get; set; }
    }

    class Select
    {
        public SelectData[] Options { get; set; }
        public string DefaultValue { get; set; }
    }

    class SelectData
    {
        public string Name { get; set; }
        public string Arg { get; set; }
        public string[] Disables { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
