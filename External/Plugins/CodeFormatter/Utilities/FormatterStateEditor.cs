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
            var formatter = (FormatterState) value;
            
            var dialog = new HaxeAStyleDialog(formatter);

            var result = dialog.ShowDialog(PluginCore.PluginBase.MainForm);
            if (result == DialogResult.OK)
                return dialog.ToFormatterState();

            return value;
        }
    }
}
