using NoteTrackerV3.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    public partial class Pad : Form
    {
        private Sizer _sizer = null;
        private NotePd _pad = null;
        private MenuBar _menu = null;

        private MouseEvent deleteFileEvent = null;

        private int sizerWidth = 10, sizerHeight = 10;

        public Pad()
        {
            this.Icon = Resources.favicon;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            //this.BackColor = settings.colorOf("ButtonBGColor");
            this.TransparencyKey = this.BackColor = Color.Lime;

            this.TopMost = settings.BoolOf("AlwaysOnTop");
            this.Name = "PadWindow";
            this.Visible = settings.BoolOf("PadWindow");
            this.Width = settings.IntOf("NotepadWidth");
            this.Height = settings.IntOf("NotepadHeight");
            this.Location = new Point(settings.IntOf("NotepadX"), settings.IntOf("NotepadY"));

            _pad = new NotePd(this);
            _pad.Width -= sizerWidth;
            _sizer = new Sizer(this, new Point(this.Width - sizerWidth, this.Height - sizerHeight), sizerWidth, sizerHeight);

            _menu = new TitleBar(this, this.Width-sizerWidth, settings.IntOf("ButtonHeight"), 0, settings.colorOf("ButtonBGColor"));
            var b = this._menu.addMenuItem("menu", 50, 20, 0, 0);
            b.setForm(this);
            var c = _menu.addSubMenu("new", b);

            c.addMouseEvent(new MouseEvent(this._pad.newPad, MouseEvents.LEFT_CLICK));
            this.Controls.Add(c);
            c = _menu.addSubMenu("save", b);
            c.addMouseEvent(new MouseEvent(processSave, MouseEvents.LEFT_CLICK));
            this.Controls.Add(c);
            c = _menu.addSubMenu("hide", b);
            this.Controls.Add(c);
            c.addMouseEvent(new MouseEvent(processHide, MouseEvents.LEFT_CLICK));

            b = this._menu.addMenuItem("load", 50, 20, 50, 0);
            b.setForm(this);
            b.addOnHover(fillLoadMenu);

            b = this._menu.addMenuItem("clear", 50, 20, 100, 0);
            b.setForm(this);
            b.addMouseEvent(new MouseEvent(this._pad.clearComplete, MouseEvents.LEFT_CLICK));

            deleteFileEvent = new MouseEvent(this.deleteFileHandler, MouseEvents.LEFT_CLICK);

            _menu.BringToFront();

            this.Controls.Add(_sizer);
            this.Controls.Add(_menu);
            this.Controls.Add(_pad);

            this.FormClosing += Pad_FormClosing;
            Messenger.SettingChanged += Messenger_SettingChanged;
        }

        private void processHide(Object o = null, EventArgs e = null)
        {
            ((MenuEntity)o).getParent().closeMenu();
            this.Visible = false;
        }

        public void processSave(Object o = null, EventArgs e = null)
        {
            try
            {
                String fname = this._pad.Text.Substring(0, this._pad.Text.IndexOf('\n'));
                fname = fname.Replace('/', '-');
                String fpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                fpath = fpath.Substring(0, fpath.LastIndexOf("\\"));
                if (fname.Length >= 0)
                {
                    this._pad.SaveFile(fpath + "\\" + fname + ".rtf");
                }
            }
            catch
            {
                //MessageBox.Show("Failed to save" + ex);
            }
        }

        public void save()
        {
            settings.set("NotepadWidth", this.Width.ToString());
            settings.set("NotepadHeight", this.Height.ToString());
            settings.set("NotepadX", this.Location.X.ToString());
            settings.set("NotepadY", this.Location.Y.ToString());
            this.processSave();
        }

        private void Messenger_SettingChanged(string arg1, string arg2, string old)
        {
            //this.BackColor = settings.colorOf("ButtonBGColor");
            this.TopMost = settings.BoolOf("AlwaysOnTop");
            this._sizer.BackColor = settings.colorOf("ButtonFGColor");
        }
        private void Pad_FormClosing(object sender, FormClosingEventArgs e)
        {
            save();
        }

        private void loadFromMenuItem(Object o, EventArgs e)
        {
            try
            {
                MenuEntity mb = (MenuEntity)o;
                this._pad.LoadFile(mb.Name);
                this._pad.getDate(RegexOptions.RightToLeft);
                this._pad.updateTextColour(@"\[\s\d{1,2}\/\d{1,2}\/\d{4}\s\]", settings.colorOf("NotepadDateColor")); //date
                this._pad.updateTextColour(@"\[(\d+|\s\d):\d+\w+\]", settings.colorOf("NotepadTimeColor")); //time
            }
            catch (Exception ex)
            {
                MessageBox.Show("LOAD FROM MENU ITEM " + ex);
            }
        }

        void fillLoadMenu(Object o, EventArgs e = null)
        {
            try
            {
                MenuEntity mb = (MenuEntity)o;
                String fpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                fpath = fpath.Substring(0, fpath.LastIndexOf("\\"));
                List<Tuple<string, string>> list = new List<Tuple<string, string>>();
                if (Directory.Exists(fpath))
                {
                    string[] files = Directory.GetFiles(fpath);
                    foreach (string fname in files)
                    {
                        Match match = Regex.Match(fname, @"\[\s*\d{1,2}\-\d{1,2}\-\d{4}\s*\]");
                        if (match.Success)
                        {
                            string name = fname.Substring(fname.LastIndexOf('[') + 1);
                            name = name.Substring(0, name.LastIndexOf(']') - 1).Replace(" ", "");
                            list.Add(Tuple.Create(fname.Substring(fname.LastIndexOf('\\') + 1), name));
                        }
                    }
                }

                var orderedList = list.OrderBy(x =>
                {
                    DateTime dt;
                    DateTime.TryParse(x.Item2, out dt);
                    return dt;
                });

                foreach (var t in orderedList)
                {
                    var b = mb.addSubMenu(t.Item2);
                    if (b != null)
                    {
                        this.Controls.Add(b);
                        b.Name = t.Item1;
                        b.addMouseEvent(new MouseEvent(this.loadFromMenuItem, MouseEvents.LEFT_CLICK));
                        b.addMouseEvent(new MouseEvent(this.rightClickMenu, MouseEvents.MIDDLE_CLICK));
                        b.BringToFront();
                    }
                }
                mb.showSubMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FILL LOAD MENU " + ex);
            }
        }
        void rightClickMenu(Object o, EventArgs e)
        {
            try
            {
                MenuEntity b = (MenuEntity)o;
                var pos = PointToClient(Cursor.Position);
                var newb = new MenuEntity("delete", pos.X, pos.Y);

                this.Controls.Add(newb);
                newb.setParent(b);
                newb.bgColorOn = Color.DarkMagenta;
                newb.destroyOnLostFocus = true;
                newb.addMouseEvent(deleteFileEvent);
                newb.BringToFront();
                newb.Focus();
            }
            catch { }
        }
        void deleteFileHandler(Object o, EventArgs e)
        {
            try
            {
                MenuEntity b = (MenuEntity)o;
                MenuEntity p = b.getParent();
                String fpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                fpath = fpath.Substring(0, fpath.LastIndexOf("\\")) + "\\";
                File.Delete(fpath + p.Name);
                p.getParent().remove(p);
                p.Dispose();
                ((MenuEntity)o).destroyASAP = true;
            }
            catch (Exception ex) { MessageBox.Show("Unable to delete file" + ex); }
        }
    }
}
