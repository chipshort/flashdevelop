using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using CodeFormatter.Dialogs;
using CodeFormatter.Preferences;

namespace CodeFormatter.Utilities
{
    class FormatterStateEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var entry = (KeyValuePair<string, FormatterState>) value;
            var dialog = new HaxeAStyleDialog(entry.Value);

            //TODO: Collection editor seems to mess stuff up
            var result = dialog.ShowDialog(PluginCore.PluginBase.MainForm);
            return result == DialogResult.OK ? new KeyValuePair<string, FormatterState>(entry.Key, dialog.ToFormatterState()) : value;
        }
    }
}
