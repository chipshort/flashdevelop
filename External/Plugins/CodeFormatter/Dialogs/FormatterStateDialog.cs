using CodeFormatter.Preferences;
using CodeFormatter.Utilities;
using PluginCore;
using ScintillaNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PluginCore.Localization;
using System.Runtime.Serialization;
using System.Windows.Forms.Design;

namespace CodeFormatter.Dialogs
{
    public class FormatterStateDialog : Form, IWindowsFormsEditorService
    {
        readonly ScintillaControl txtExample;
        readonly Dictionary<string, Control> mapping = new Dictionary<string, Control>();

        string formatterFile;
        FormatterDefinition formatter;

        #region Windows Form Designer generated code

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Panel pnlSci;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private CheckBox checkCurrentFile;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pnlSci = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.checkCurrentFile = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Location = new System.Drawing.Point(0, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(764, 494);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            // 
            // pnlSci
            // 
            this.pnlSci.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSci.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSci.Location = new System.Drawing.Point(292, 40);
            this.pnlSci.Name = "pnlSci";
            this.pnlSci.Size = new System.Drawing.Size(458, 456);
            this.pnlSci.TabIndex = 11;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(6, 475);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "&OK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(87, 475);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // checkCurrentFile
            // 
            this.checkCurrentFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkCurrentFile.AutoSize = true;
            this.checkCurrentFile.BackColor = System.Drawing.Color.White;
            this.checkCurrentFile.Location = new System.Drawing.Point(167, 479);
            this.checkCurrentFile.Name = "checkCurrentFile";
            this.checkCurrentFile.Size = new System.Drawing.Size(105, 17);
            this.checkCurrentFile.TabIndex = 13;
            this.checkCurrentFile.Text = "Show current file";
            this.checkCurrentFile.UseVisualStyleBackColor = false;
            this.checkCurrentFile.CheckedChanged += new System.EventHandler(this.checkExampleFile_CheckedChanged);
            // 
            // HaxeAStyleDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 506);
            this.Controls.Add(this.pnlSci);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.checkCurrentFile);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(346, 306);
            this.Name = "HaxeAStyleDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Haxe Formatter Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public FormatterStateDialog(FormatterState state)
        {
            InitializeComponent();
            InitializeLocalization();

            var currentDoc = PluginBase.MainForm.CurrentDocument;
            if (string.IsNullOrEmpty(currentDoc?.SciControl?.Text) || currentDoc.SciControl.ConfigurationLanguage != "haxe")
                checkCurrentFile.Enabled = false;

            this.Font = PluginBase.Settings.DefaultFont;

            //Create Scintilla
            txtExample = new ScintillaControl
            {
                Dock = DockStyle.Fill,
                ConfigurationLanguage = "haxe",
                ViewWhitespace = ScintillaNet.Enums.WhiteSpace.VisibleAlways,
                Lexer = 3
            };
            txtExample.SetProperty("lexer.cpp.track.preprocessor", "0");

            //Read example file
            //var id = "CodeFormatter.Resources.AStyleExample.hx";
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //using (var reader = new StreamReader(assembly.GetManifestResourceStream(id)))
            //{
            //    txtExample.Text = reader.ReadToEnd();
            //}

            this.pnlSci.Controls.Add(txtExample);

            //SetOptions(options);
            InitFromState(state);

            ReformatExample();
        }

        void InitFromState(FormatterState state)
        {
            LoadFormatter(state.File);

            foreach (var o in state.Options.Values)
            {
                var checkOpt = o as CheckOption;
                var selectOpt = o as SelectOption;
                var numberOpt = o as NumberOption;
                Control control;

                if (checkOpt != null)
                {
                    if (mapping.TryGetValue(checkOpt.Id, out control))
                    {
                        var c = control as CheckBox;
                        c.Checked = checkOpt.State;
                        c.Enabled = checkOpt.Enabled;
                    }
                    //ignore else silently
                }
                else if (selectOpt != null)
                {
                    if (mapping.TryGetValue(selectOpt.Id, out control))
                    {
                        var c = control as ComboBox;
                        c.Enabled = selectOpt.Enabled;
                        try
                        {
                            c.SelectedIndex = selectOpt.Selection;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            c.SelectedIndex = 0;
                        }
                        
                    }
                    //ignore else silently
                }
                else if (numberOpt != null)
                {
                    if (mapping.TryGetValue(numberOpt.Id, out control))
                    {
                        var c = control as NumericUpDown;
                        c.Enabled = numberOpt.Enabled;
                        c.Value = int.Parse(numberOpt.Value);
                    }
                    //ignore else silently
                }
            }
        }

        #region Localization

        private void InitializeLocalization()
        {
            //TODO: localization
            this.Text = TextHelper.GetString("Title.AStyleFormatterSettings");
            this.btnSave.Text = TextHelper.GetString("FlashDevelop.Label.Save");
            this.btnCancel.Text = TextHelper.GetString("FlashDevelop.Label.Cancel");
            this.checkCurrentFile.Text = TextHelper.GetString("Label.ShowCurrentFile");
            //this.checkAddBrackets.Text = TextHelper.GetString("Label.AddBrackets");
            //this.checkAttachClasses.Text = TextHelper.GetString("Label.AttachClasses");
            //this.checkBreakClosing.Text = TextHelper.GetString("Label.BreakClosing");
            //this.checkBreakElseifs.Text = TextHelper.GetString("Label.BreakElseifs");
            //this.checkDeleteEmptyLines.Text = TextHelper.GetString("Label.DeleteEmptyLines");
            //this.checkFillEmptyLines.Text = TextHelper.GetString("Label.FillEmptyLines");
            //this.checkForceTabs.Text = TextHelper.GetString("Label.ForceTabs");
            //this.checkIndentCase.Text = TextHelper.GetString("Label.IndentCases");
            //this.checkIndentConditional.Text = TextHelper.GetString("Label.IndentConditionals");
            //this.checkIndentSwitches.Text = TextHelper.GetString("Label.IndentSwitches");
            //this.checkKeepOneLineBlocks.Text = TextHelper.GetString("Label.KeepOneLineBlocks");
            //this.checkKeepOneLineStatements.Text = TextHelper.GetString("Label.KeepOneLineStatements");
            //this.checkPadCommas.Text = TextHelper.GetString("Label.PadCommas");
            //this.checkOneLineBrackets.Text = TextHelper.GetString("Label.AddOneLineBrackets");
            //this.checkPadAll.Text = TextHelper.GetString("Label.PadAllBlocks");
            //this.checkPadBlocks.Text = TextHelper.GetString("Label.PadBlocks");
            //this.checkPadHeaders.Text = TextHelper.GetString("Label.PadHeaders");
            //this.checkRemoveBrackets.Text = TextHelper.GetString("Label.RemoveBracketsFromContitionals");
            //this.checkTabs.Text = TextHelper.GetString("Label.UseTabs");
            //this.checkPadParensIn.Text = TextHelper.GetString("Label.PadParensIn");
            //this.checkPadParensOut.Text = TextHelper.GetString("Label.PadParensOut");
            //this.lblBracketStyle.Text = TextHelper.GetString("Label.BracketStyle");
            //this.lblIndentSize.Text = TextHelper.GetString("Label.IndentSize");
            //this.tabBrackets.Text = TextHelper.GetString("Info.Brackets");
            //this.tabFormatting.Text = TextHelper.GetString("Info.Formatting");
            //this.tabIndents.Text = TextHelper.GetString("Info.Indentation");
            //this.tabPadding.Text = TextHelper.GetString("Info.Padding");
        }

        #endregion

        internal FormatterState ToFormatterState()
        {
            var state = new FormatterState(formatterFile, formatter.Command);
            state.AdditionalArgs = formatter.AdditionalArgs;
            state.Language = formatter.Language;

            foreach (var c in mapping)
            {
                var checkBox = c.Value as CheckBox;
                var numUpDown = c.Value as NumericUpDown;
                var comboBox = c.Value as ComboBox;

                if (checkBox != null)
                {
                    if (checkBox.Enabled)
                        state.Add(new CheckOption(c.Key, checkBox.Checked, GetArg(checkBox), checkBox.Enabled));
                }
                else if (numUpDown != null)
                {
                    if (numUpDown.Enabled)
                        state.Add(new NumberOption(c.Key, (int) numUpDown.Value, GetArg(numUpDown), numUpDown.Enabled));
                }
                else if (comboBox != null)
                {
                    if (comboBox.Enabled)
                        state.Add(new SelectOption(c.Key, comboBox.SelectedIndex, comboBox.SelectedText, GetArg(comboBox), comboBox.Enabled)); //TODO: check if SelectedText works
                }
            }

            return state;
        }

        string GetArg(CheckBox checkBox)
        {
            if (checkBox.Enabled)
            {
                var check = (Check)checkBox.Tag;
                if (checkBox.Checked)
                {
                    if (check.ArgChecked != null)
                        return FormatterHelper.ProcessArgument(check.ArgChecked, GetMapping);
                }
                else if (check.ArgUnchecked != null)
                {
                    return FormatterHelper.ProcessArgument(check.ArgUnchecked, GetMapping);
                }
            }
            return null;
        }

        string GetArg(NumericUpDown numUpDown)
        {
            if (numUpDown.Enabled)
            {
                var num = (Number)numUpDown.Tag;
                if (num.Arg != null)
                    return FormatterHelper.ProcessArgument(num.Arg, GetMapping);
            }
            return null;
        }

        string GetArg(ComboBox comboBox)
        {
            if (comboBox.Enabled)
            {
                var data = (SelectData)comboBox.SelectedItem;
                if (data.Arg != null)
                    return FormatterHelper.ProcessArgument(data.Arg, GetMapping);
            }
            return null;
        }

        string GetMapping(string id)
        {
            Control ctrl;
            if (!mapping.TryGetValue(id, out ctrl))
                throw new FormatterException($"Formatter argument references \"{id}\", but it does not exist");

            return GetValue(ctrl);
        }

        string GetValue(Control ctrl)
        {
            var checkBox = ctrl as CheckBox;
            var numUpDown = ctrl as NumericUpDown;
            var comboBox = ctrl as ComboBox;

            //not sure if we might need to check for Enabled first?
            if (checkBox != null)
                return checkBox.Checked.ToString();

            if (numUpDown != null)
                return numUpDown.Value.ToString("F1");

            if (comboBox != null)
                return ((SelectData)comboBox.SelectedItem).Value;

            return "";
        }

        /// <summary>
        /// Helper method to apply the selected AStyle settings to the text
        /// </summary>
        void ReformatExample()
        {
            AStyleInterface astyle = new AStyleInterface();
            string[] options = ToFormatterState().ToOptions().ToArray();

            var firstLine = txtExample.FirstVisibleLine;

            txtExample.IsReadOnly = false;
            txtExample.Text = (tabControl.SelectedTab?.Tag as Category)?.Code ?? formatter.Code;

            var currentDoc = PluginBase.MainForm.CurrentDocument;
            if (checkCurrentFile.Checked && currentDoc?.SciControl != null)
            {
                txtExample.Text = currentDoc.SciControl.Text;
            }
            //txtExample.TabWidth = (int) numIndentWidth.Value;
            txtExample.TabWidth = PluginBase.Settings.TabWidth;
            txtExample.IsFocus = true;
            txtExample.Text = astyle.FormatSource(txtExample.Text, string.Join(" ", options));
            txtExample.IsReadOnly = true;

            txtExample.FirstVisibleLine = firstLine;
        }

        void ValidateControls()
        {
            foreach (var opt in mapping) //Enable everything
                opt.Value.Enabled = true;

            //Lookup what needs to be disabled
            foreach (var opt in mapping)
            {
                var checkBox = opt.Value as CheckBox;
                var comboBox = opt.Value as ComboBox;

                if (checkBox != null)
                {
                    var check = (Check) checkBox.Tag;
                    if (!checkBox.Enabled)
                        foreach (var sub in check.Suboptions)
                            mapping[sub.Id].Enabled = checkBox.Checked;
                }
                else
                {
                    var select = (SelectData) comboBox?.SelectedItem;
                    if (select?.Disables != null)
                        foreach (var disabled in select.Disables)
                        {
                            Control ctrl;
                            if (!mapping.TryGetValue(disabled, out ctrl))
                                throw new FormatterException($"The formatter references an option \"{disabled}\", but it does not exist");

                            ctrl.Enabled = false;
                        }
                }
            }
        }

        void LoadFormatter(string filename)
        {
            //Reset old ui
            tabControl.TabPages.Clear();
            mapping.Clear();

            formatter = FormatterHelper.ParseFormatter(filename);

            //Initialize categories
            foreach (var cat in formatter.Categories)
            {
                var tp = new TabPage
                {
                    Text = cat.Name,
                    Tag = cat,
                    UseVisualStyleBackColor = true
                };
                AddControls(cat.Options, tp, new Point(Pad, Pad));
                tabControl.TabPages.Add(tp);
            }

            formatterFile = filename;
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void CloseDropDown()
        {
            //throw new NotImplementedException();
        }

        public void DropDownControl(Control control)
        {
            //throw new NotImplementedException();
        }

        public DialogResult ShowDialog(Form dialog)
        {
            return dialog.ShowDialog(this);
        }

        void checkExampleFile_CheckedChanged(object sender, EventArgs e)
        {
            ReformatExample();
        }

        void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == null) return;

            ReformatExample();
        }

        const int SubPad = 20;
        const int Pad = 6;

        /// <summary>
        /// Adds the options in <paramref name="opts"/> as Controls to <paramref name="page"/>.
        /// </summary>
        /// <param name="location">The point to start adding at</param>
        /// <returns></returns>
        int AddControls(FormatOption[] opts, TabPage page, Point location)
        {
            foreach (var opt in opts)
            {
                switch (opt.Type)
                {
                    case "check":
                        var check = opt.CheckData;
                        var checkBox = new CheckBox();

                        checkBox.Text = opt.Name;
                        checkBox.Location = location;
                        checkBox.Tag = check;
                        checkBox.AutoSize = true;
                        checkBox.Checked = check.DefaultValue;
                        checkBox.CheckedChanged += CheckBox_CheckedChanged;

                        page.Controls.Add(checkBox);
                        location.Offset(0, checkBox.Height + Pad);

                        if (check.Suboptions != null)
                        {
                            var subLocation = location;
                            subLocation.Offset(SubPad, 0);
                            location.Y = AddControls(check.Suboptions, page, subLocation);

                            foreach (var sub in check.Suboptions)
                                mapping[sub.Id].Enabled = check.DefaultValue;
                        }

                        AddMapping(opt.Id, checkBox);
                        break;
                    case "select":
                        var select = opt.SelectData;
                        var selLabel = new Label();
                        var comboBox = new ComboBox();

                        selLabel.Text = opt.Name;
                        selLabel.Location = location;

                        comboBox.Items.AddRange(select);

                        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                        comboBox.Location = new Point(location.X + selLabel.Width + Pad, location.Y);
                        comboBox.SelectionChangeCommitted += ComboBox_SelectionChangeCommitted;
                        comboBox.SelectedIndex = 0;

                        page.Controls.Add(selLabel);
                        page.Controls.Add(comboBox);
                        location.Offset(0, comboBox.Height + Pad);

                        AddMapping(opt.Id, comboBox);

                        break;
                    case "number":
                        var number = opt.NumberData;
                        var numLabel = new Label();
                        var numUpDown = new NumericUpDown();

                        numLabel.Text = opt.Name;
                        numLabel.Size = TextRenderer.MeasureText(opt.Name, numLabel.Font);
                        numLabel.Location = new Point(location.X, location.Y + (numUpDown.Height - numLabel.Height) / 2);

                        numUpDown.Minimum = number.Min;
                        numUpDown.Maximum = number.Max;
                        numUpDown.Value = number.DefaultValue;
                        numUpDown.Width = 60;
                        numUpDown.Location = new Point(location.X + numLabel.Width + Pad, location.Y);
                        numUpDown.Tag = number;
                        numUpDown.ValueChanged += NumUpDown_ValueChanged;

                        location.Offset(0, numUpDown.Height + Pad);

                        page.Controls.Add(numLabel);
                        page.Controls.Add(numUpDown);

                        AddMapping(opt.Id, numUpDown);

                        break;
                    default:
                        throw new FormatterException("Type of " + opt.Id + " is invalid: \"" + (opt.Type ?? "null") + "\"");
                }
            }
            return location.Y;
        }

        void AddMapping(string optId, Control checkBox)
        {
            try
            {
                mapping.Add(optId, checkBox);
            }
            catch (ArgumentNullException e)
            {
                throw new FormatterException("Formatter contains an option without Id", e);
            }
            catch (ArgumentException e)
            {
                throw new FormatterException("Formatter contains two options with the same id: " + optId, e);
            }
        }

        void NumUpDown_ValueChanged(object sender, EventArgs e)
        {
            //var numUpDown = (NumericUpDown) sender;
            //var num = (Number) numUpDown.Tag;

            ReformatExample();
        }

        void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox) sender;
            var check = (Check) checkBox.Tag;

            if (checkBox.Checked && check.Unchecks != null)
                foreach (var c in check.Unchecks)
                {
                    Control ctrl;
                    if (!mapping.TryGetValue(c, out ctrl))
                        throw new FormatterException($"Formatter specifies \"{c}\" to be unchecked, but it does not exist");

                    var uncheckMe = ctrl as CheckBox;
                    if (uncheckMe == null)
                        throw new FormatterException($"Formatter specifies \"{c}\" to be unchecked, but it is not of type \"check\"");

                    uncheckMe.Checked = false;
                }

            if (check.Suboptions != null)
                foreach (var c in check.Suboptions)
                {
                    var sub = (CheckBox)mapping[c.Id];
                    sub.Enabled = checkBox.Checked;
                }

            ReformatExample();
        }

        void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var select = (SelectData) comboBox.SelectedItem;

            if (select.Disables != null)
                foreach (var c in select.Disables) //TODO: how to reenable it?
                {
                    Control control;
                    if (!mapping.TryGetValue(c, out control))
                        throw new FormatterException($"The formatter references an option \"{c}\", but it does not exist");

                    control.Enabled = false;
                }

            ReformatExample();
        }
    }

    public class FormatterException : Exception
    {

        public FormatterException()
        {
        }

        public FormatterException(String message)
            : base(message) {
        }

        public FormatterException(String message, Exception innerException)
            : base(message, innerException) {
        }

        protected FormatterException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
