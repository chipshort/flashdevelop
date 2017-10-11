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

            var result = dialog.ShowDialog(PluginCore.PluginBase.MainForm);
            if (result == DialogResult.OK)
            {
                var newState = dialog.ToFormatterState();
                //need to copy the values manually to the old object, because KeyValuePair.Value is read-only
                entry.Value.File = newState.File;
                entry.Value.AdditionalArgs = newState.AdditionalArgs;
                entry.Value.Options = newState.Options;
            }

            return entry;
        }
    }
}
