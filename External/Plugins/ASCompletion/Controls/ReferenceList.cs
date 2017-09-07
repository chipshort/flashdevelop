﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using ASCompletion.Model;
using PluginCore;
using PluginCore.Localization;
using Timer = System.Timers.Timer;

namespace ASCompletion.Controls
{
    class ReferenceList
    {
        static readonly Timer fadingTimer;
        static readonly ListView listView;

        static ReferenceList()
        {
            listView = new ListView
            {
                Visible = false,
                Width = 300,
                Height = 200,
                Columns = {""},
                View = View.Details,
                ShowGroups = true,
                MultiSelect = false,
                FullRowSelect = false,
                HeaderStyle = ColumnHeaderStyle.None
            };

            listView.Groups.Add("implementors", TextHelper.GetString("Label.ImplementedBy"));
            listView.Groups.Add("implements", TextHelper.GetString("Label.Implements"));
            listView.Groups.Add("overriders", TextHelper.GetString("Label.OverriddenBy"));
            listView.Groups.Add("overrides", TextHelper.GetString("Label.Overrides"));

            fadingTimer = new Timer
            {
                Interval = 800,
                SynchronizingObject = (Form) PluginBase.MainForm
            };
            
            listView.MouseEnter += ListView_MouseEnter;
            listView.MouseLeave += ListView_MouseLeave;
            listView.DoubleClick += ListView_DoubleClick;
            listView.KeyPress += ListView_KeyPress;

            fadingTimer.Elapsed += FadingTimer_Elapsed;

            PluginBase.MainForm.Controls.Add(listView);
        }

        static void ListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                ListView_DoubleClick(null, null);
            }
            else if (e.KeyChar == (char) Keys.Escape)
            {
                listView.Hide();
                fadingTimer.Stop();
            }

        }

        static void ListView_MouseLeave(object sender, EventArgs e)
        {
            fadingTimer.Start();
        }

        static void ListView_MouseEnter(object sender, EventArgs e)
        {
            fadingTimer.Stop();
        }

        static void FadingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!listView.RectangleToScreen(new Rectangle(0, 0, listView.Width, listView.Height)).Contains(Control.MousePosition))
            {
                listView.Hide();
                fadingTimer.Stop();
            }
        }

        static void ListView_DoubleClick(object sender, EventArgs e)
        {
            var selection = listView.Items[listView.SelectedIndices[0]];
            var reference = (Reference)selection.Tag;

            listView.Hide();
            fadingTimer.Stop();
            PluginBase.MainForm.OpenEditableDocument(reference.File, false);
            PluginBase.MainForm.CurrentDocument.SciControl.GotoLine(reference.Line);
        }

        internal static void Show(Reference[] implementors, Reference[] implemented, Reference[] overriders, Reference[] overridden)
        {
            listView.Hide();
            listView.Items.Clear();

            var implementorsGroup = listView.Groups["implementors"];
            var implementedGroup = listView.Groups["implements"];
            var overridersGroup = listView.Groups["overriders"];
            var overriddenGroup = listView.Groups["overrides"];

            AddItems(implementorsGroup, implementors);
            AddItems(implementedGroup, implemented);
            AddItems(overridersGroup, overriders);
            AddItems(overriddenGroup, overridden);

            var p = ((Form)PluginBase.MainForm).PointToClient(Cursor.Position);
            p.Offset(-2, -2);
            listView.Location = p;

            listView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Width = listView.Columns[0].Width + 30;

            listView.BringToFront();
            listView.Show();
            listView.Focus();
            
            if (listView.Items.Count > 0)
                listView.Height = Math.Min(listView.Items[listView.Items.Count - 1].Bounds.Bottom + 10, 500);

            fadingTimer.Start();
            ListView_MouseEnter(null, null);
        }

        internal static Reference[] ConvertCache(MemberModel member, HashSet<ClassModel> list)
        {
            var array = new Reference[list.Count];

            var i = 0;
            foreach (var m in list)
            {
                array[i++] = new Reference
                {
                    File = m.InFile.FileName,
                    Line = m.Members.Search(member.Name, 0, 0).LineFrom,
                    Type = m.QualifiedName
                };
            }
            return array;
            //return list.Select(m => new Reference
            //{
            //    File = m.InFile.FileName,
            //    Line = m.Members.Search(member.Name, 0, 0).LineFrom,
            //    Type = m.QualifiedName
            //});
        }

        static void AddItems(ListViewGroup group, Reference[] items)
        {
            for (var i = 0; i < items.Length; ++i)
            {
                var item = new ListViewItem
                {
                    Text = items[i].Type,
                    Tag = items[i],
                    Group = group
                };
                listView.Items.Add(item);
            }
            
        }
    }

    struct Reference
    {
        public string Type;
        public string File;
        public int Line;
    }
}
