using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    static class StoreStates
    {
        private static Panel states = null;
        private static List<KeyValuePair<string, Panel>> stores = null;
        private static RadioButton selectedState = null;
        private static Panel shownStore = null;

        private static StoreForm p_parent;

        public static int getWidth()
        {
            if (states != null)
                return states.Width;
            return -1;
        }

        public static string getShownState()
        {
            string ret = "";
            if (selectedState != null)
            {
                ret = selectedState.Text;
            }
            return ret;
        }
        
        public static string[] getSelectedStores()
        {
            List<string> ret = new List<string>();

            if (shownStore != null)
            {
                foreach (CheckBox x in shownStore.Controls)
                {
                    if (x.Checked) ret.Add(x.Text);
                }
            }

            return ret.ToArray();
        }

        public static void reset()
        {
            foreach(RadioButton i in states.Controls)
            {
                i.Checked = false;
            }
            foreach(var i in stores)
            {
                foreach (CheckBox cb in i.Value.Controls)
                    cb.Checked = false;
            }
            selectedState = null;
            shownStore = null;
        }

        public static void init(StoreForm parent)
        {
            if (stores != null)
            {
                stores.Clear();
            }
            else
            {
                p_parent = parent;
                states = new Panel();
                stores = new List<KeyValuePair<string, Panel>>();
                parent.Controls.Add(states);
                states.BackColor = settings.colorOf("ButtonBGColor");
                states.ForeColor = settings.colorOf("ButtonFGColor");

                states.Height = 20;

                states.Location = new Point(0, 20);
                Messenger.SettingChanged += Messenger_SettingChanged;
            }
            string fname = settings.valueOf("StoresFile");
            if (File.Exists(fname))
            {
                try
                {
                    StreamReader sr = new StreamReader(fname);
                    Panel curPanel = null;
                    RadioButton rb = null;
                    RadioButton lastrb = null;

                    CheckBox cb = null, lastcb = null;

                    int longest = 0;

                    while (!sr.EndOfStream)
                    {
                        string currentLine = sr.ReadLine();
                        if (currentLine.Length > 0)
                        {
                            if (currentLine[0] != '\t')
                            {
                                rb = new RadioButton();
                                if (lastrb != null) rb.Location = new Point(lastrb.Location.X + lastrb.Width, 0);
                                else rb.Location = new Point(0, 0);
                                rb.AutoSize = true;
                                rb.BackColor = settings.colorOf("ButtonBGColor");
                                rb.ForeColor = settings.colorOf("ButtonFGColor");
                                rb.Margin = new Padding(0, 0, 0, 0);
                                rb.Appearance = Appearance.Button;
                                rb.Text = currentLine;
                                rb.CheckedChanged += Cb_CheckedChanged;
                                rb.LostFocus += p_parent.lostme;

                                states.Controls.Add(rb);
                                curPanel = new Panel();
                                curPanel.Location = new Point(0, 40);
                                curPanel.Height = 20;
                                curPanel.BackColor = Color.Red;
                                curPanel.BringToFront();
                                curPanel.Hide();
                                curPanel.BackColor = settings.colorOf("ButtonBGColor");
                                curPanel.ForeColor = settings.colorOf("ButtonFGColor");
                                parent.Controls.Add(curPanel);
                                stores.Add(new KeyValuePair<string, Panel>(currentLine, curPanel));

                                lastrb = rb;
                                states.Width = lastrb.Location.X + lastrb.Width;
                                lastcb = null;
                            }
                            else if (curPanel != null)
                            {
                                cb = new CheckBox();
                                cb.Text = currentLine.Trim('\t');
                                cb.AutoSize = true;
                                cb.BackColor = settings.colorOf("ButtonBGColor");
                                cb.ForeColor = settings.colorOf("ButtonFGColor");

                                cb.Appearance = Appearance.Button;
                                cb.Margin = new Padding(0, 0, 0, 0);
                                cb.LostFocus += p_parent.lostme;

                                if (lastcb != null) cb.Location = new Point(lastcb.Location.X + lastcb.Width, 0);
                                else cb.Location = new Point(0, 0);

                                curPanel.Controls.Add(cb);

                                cb.CheckedChanged += Cb_CheckedChanged1;

                                lastcb = cb;
                                curPanel.Width = lastcb.Location.X + lastcb.Width;
                                if (curPanel.Width > longest) longest = curPanel.Width;
                            }
                        }
                    }
                    states.Width = longest;
                    foreach (var x in stores)
                    {
                        x.Value.Width = longest;
                    }
                    sr.Close();
                }
                catch { }
            }
        }

        private static void Cb_CheckedChanged1(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                cb.BackColor = settings.colorOf("ButtonBGColorHover");
                cb.ForeColor = settings.colorOf("ButtonFGColorHover");

            }
            else
            {
                cb.BackColor = settings.colorOf("ButtonBGColor");
                cb.ForeColor = settings.colorOf("ButtonFGColor");
            }
        }

        private static void Cb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            rb.Capture = false;
            if (rb.Checked)
            {
                rb.BackColor = settings.colorOf("ButtonBGColorHover");
                rb.ForeColor = settings.colorOf("ButtonFGColorHover");

                var found = stores.Find(p => p.Key == rb.Text);
                if(found.Key != null){
                    found.Value.Show();
                    shownStore = found.Value;
                }
                selectedState = rb;
            }
            else
            {
                rb.BackColor = settings.colorOf("ButtonBGColor");
                rb.ForeColor = settings.colorOf("ButtonFGColor");
                if (shownStore != null) shownStore.Hide();
            }
        }

        private static void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            if (states != null)
            {
                foreach (var x in states.Controls)
                {
                    ((RadioButton)x).BackColor = settings.colorOf("ButtonBGColor");
                    ((RadioButton)x).ForeColor = settings.colorOf("ButtonFGColor");
                }
            }
            if (stores != null) {
                foreach (var x in stores)
                {
                    foreach (var y in x.Value.Controls)
                    {
                        ((CheckBox)y).BackColor = settings.colorOf("ButtonBGColor");
                        ((CheckBox)y).ForeColor = settings.colorOf("ButtonFGColor");
                    }
                }
            }
        }
    }
    class StoreForm : Form
    {
        public SearchBar searchBar = null;
        private Action<MenuEntity, string> action = null;
        private MenuEntity mb = null;

        private bool invoked = false;
        public StoreForm()
        {
            searchBar = new SearchBar();
            searchBar.closeOnLostFocus = false;
            searchBar.controller = this;

            searchBar.LostFocus += lostme;

            StoreStates.init(this);

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TransparencyKey = this.BackColor = Color.Lime;
            this.TopMost = true;

            searchBar.Height = 20;
            searchBar.Width = StoreStates.getWidth();

            this.Size = new Size(searchBar.Width, 60);

            this.Controls.Add(searchBar);
        }

        public void lostme(Object o, EventArgs e)
        {
            hide();
        }

        private void searchHandler(MenuEntity me, string a)
        {
            string stores = "";
            string state =  StoreStates.getShownState();
            string[] arr = StoreStates.getSelectedStores();

            if (arr.Length > 0)
            {
                stores += "[ ";
                if (state == "ONLINE") stores += "ONLINE: ";
                for (int i = 0; i < arr.Length - 1; i++) stores += arr[i] + ", ";
                stores += arr[arr.Length-1]+" ] - ";
            }
            else if (state != "")
                stores += "[ " + state + " ] - ";

            action.Invoke(me, stores+a.Replace("\n",""));
            invoked = true;
        }

        private void Rb_CheckedChangedSTART(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked)
            {
                rb.BackColor = settings.colorOf("ButtonBGColorHover");
                rb.ForeColor = settings.colorOf("ButtonFGColorHover");
            }
            else
            {
                rb.BackColor = settings.colorOf("ButtonBGColor");
                rb.BackColor = settings.colorOf("ButtonFGColor");
            }
        }


        private void Rb_CheckedChangedEND(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked)
            {
                rb.BackColor = settings.colorOf("ButtonBGColorHover");
                rb.ForeColor = settings.colorOf("ButtonFGColorHover");
                searchHandler(mb, searchBar.Text);
                hide(true);
            }
            else
            {
                rb.BackColor = settings.colorOf("ButtonBGColor");
                rb.ForeColor = settings.colorOf("ButtonFGColor");
            }
        }

        public void show(MenuEntity me, Action<MenuEntity, string> e)
        {
            action = e;
            mb = me;
            searchBar.setSearch(me, searchHandler, false);
            searchBar.Focus();
        }

        private void hide(bool hide = false)
        {
            if (!this.ContainsFocus || hide)
            {
                if (!invoked && (StoreStates.getShownState() != "" || searchBar.Text != ""))
                {
                    searchHandler(mb, searchBar.Text.Replace("\n", ""));
                }
                searchBar.Clear();
                StoreStates.reset();
                this.Hide();
                invoked = false;
            }
        }
    }

    static class ClaimStates
    {
        private static List<RadioButton> start = null;
        private static List<RadioButton> end = null;

        static string formatText(object field, int size, char c = ' ')
        {
            var value = field.ToString();
            if (value.Length == size) return value;
            return value.Length > size ? value.Substring(0, size) : value.PadRight(size, c);
        }
        public static RadioButton[] getStart()
        {
            if (start == null) init();
            return start.ToArray();
        }
        public static RadioButton[] getEnd()
        {
            if (end == null) init();
            return end.ToArray();
        }

        public static void init()
        {
            if (start == null)
                start = new List<RadioButton>();
            else start.Clear();
            if (end == null)
                end = new List<RadioButton>();
            else end.Clear();

            RadioButton rb = null;

            string fname = settings.valueOf("ClaimsFile");

            if (File.Exists(fname))
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(fname);


                    string currentLine = sr.ReadLine();
                    if (currentLine.Length > 0)
                    {
                        string[] split = currentLine.Split(',');

                        int length = split.Max(s => s.Length);

                        foreach (string s in split)
                        {
                            rb = new RadioButton(); rb.Text = ClaimStates.formatText(s, length);
                            start.Add(rb);
                        }
                    }
                    currentLine = sr.ReadLine();
                    if (currentLine.Length > 0)
                    {
                        string[] split = currentLine.Split(',');

                        int length = split.Max(s => s.Length);

                        foreach (string s in split)
                        {
                            rb = new RadioButton(); rb.Text = ClaimStates.formatText(s, length);
                            end.Add(rb);
                        }
                    }
                }
                catch { }
                sr.Close();
            }
        }
    }

    class ClaimForm : Form
    {
        public SearchBar searchBar = null;

        private Panel startState = null;
        private Panel endState = null;

        private Action<MenuEntity, string, string, string> action = null;
        private MenuEntity mb = null;

        private string start = "?";
        private string end = "?";

        public ClaimForm() {
            searchBar = new SearchBar();
            searchBar.closeOnLostFocus = false;
            searchBar.controller = this;
            searchBar.Height = 20;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TransparencyKey = this.BackColor = Color.Lime;
            this.TopMost = true;

            ClaimStates.init();

            startState = new Panel();
            startState.Location = new Point(0, 20);
            startState.Controls.AddRange(ClaimStates.getStart());
            startState.Height = 20;
            int startx = startState.Location.X;
            foreach (var x in startState.Controls)
            {
                if (x.GetType() == typeof(RadioButton))
                {
                    RadioButton rb = ((RadioButton)x);
                    rb.Location = new Point(startx, rb.Location.Y);
                    rb.AutoSize = true;
                    rb.BackColor = settings.colorOf("ButtonBGColor");
                    rb.ForeColor = settings.colorOf("ButtonFGColor");
                    rb.Margin = new Padding(0, 0, 0, 0);
                    rb.LostFocus += S_LostFocus;
                    rb.CheckedChanged += Rb_CheckedChangedSTART;
                    rb.Appearance = Appearance.Button;
                    startx += rb.Width;
                }
            }

            endState = new Panel();
            endState.Location = new Point(0, 40);
            endState.Controls.AddRange(ClaimStates.getEnd());
            endState.Height = 20;
            startx = endState.Location.X;
            foreach (var x in endState.Controls)
            {
                if (x.GetType() == typeof(RadioButton))
                {
                    RadioButton rb = ((RadioButton)x);
                    rb.Location = new Point(startx, rb.Location.Y);
                    rb.AutoSize = true;
                    rb.BackColor = settings.colorOf("ButtonBGColor");
                    rb.ForeColor = settings.colorOf("ButtonFGColor");
                    rb.Margin = new Padding(0, 0, 0, 0);
                    rb.LostFocus += S_LostFocus;
                    rb.CheckedChanged += Rb_CheckedChangedEND;
                    rb.Appearance = Appearance.Button;
                    startx += rb.Width;
                }
            }
            endState.Width = startx;
            startState.Width = startx;
            this.Size = new Size(startx, 60);
            searchBar.Width = startx;
            endState.BackColor = settings.colorOf("ButtonBGColor");
            startState.BackColor = settings.colorOf("ButtonBGColor");
            endState.ForeColor = settings.colorOf("ButtonFGColor");
            startState.ForeColor = settings.colorOf("ButtonFGColor");

            this.LostFocus += S_LostFocus;

            this.Controls.Add(startState);
            this.Controls.Add(endState);
            this.Controls.Add(searchBar);

            foreach (var c in this.Controls)
            {
                ((Control)c).LostFocus += S_LostFocus;
            }

            Messenger.SettingChanged += Messenger_SettingChanged;
        }

        private void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            endState.BackColor = settings.colorOf("ButtonBGColor");
            startState.BackColor = settings.colorOf("ButtonBGColor");
            endState.ForeColor = settings.colorOf("ButtonFGColor");
            startState.ForeColor = settings.colorOf("ButtonFGColor");
            foreach (RadioButton x in startState.Controls)
            {
                x.BackColor = settings.colorOf("ButtonBGColor");
                x.ForeColor = settings.colorOf("ButtonFGColor");

            }
            foreach (RadioButton x in endState.Controls)
            {
                x.BackColor = settings.colorOf("ButtonBGColor");
                x.ForeColor = settings.colorOf("ButtonFGColor");
            }
        }

        private void searchHandler(MenuEntity me, string a)
        {
            action.Invoke(me, a.Replace("\n",""), start, end);
            me.closeMenuf();
        }

        private void Rb_CheckedChangedSTART(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked)
            {
                start = rb.Text;
                rb.BackColor = settings.colorOf("ButtonBGColorHover");
                rb.ForeColor = settings.colorOf("ButtonFGColorHover");

            }
            else
            {
                rb.BackColor = settings.colorOf("ButtonBGColor");
                rb.ForeColor = settings.colorOf("ButtonFGColor");

            }
        }


        private void Rb_CheckedChangedEND(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked)
            {
                end = rb.Text;
                rb.BackColor = settings.colorOf("ButtonBGColorHover");
                rb.ForeColor = settings.colorOf("ButtonFGColorHover");
                searchHandler(mb, searchBar.Text);
                hide(true);
            }
            else
            {
                rb.BackColor = settings.colorOf("ButtonBGColor");
                rb.ForeColor = settings.colorOf("ButtonFGColor");

            }
        }

        public void show(MenuEntity me, Action<MenuEntity, string, string, string> e)
        {
            action = e;
            mb = me;
            searchBar.setSearch(me, searchHandler);
            searchBar.BringToFront();
            searchBar.Focus();
        }

        private void hide(bool hide = false)
        {
            if (!this.ContainsFocus || hide)
            {
                searchBar.Clear();
                foreach (RadioButton x in startState.Controls)
                {
                    x.Checked = false;
                }
                foreach (RadioButton x in endState.Controls)
                {
                    x.Checked = false;
                }
                start = "?";
                end = "?";
                this.Hide();
            }
        }

        private void S_LostFocus(object sender = null, EventArgs e = null)
        {
            this.hide();
        }
    }

    class SearchForm : Form
    {
        public SearchBar searchBar = null;

        public SearchForm(MenuEntity me, Action<MenuEntity, string> e, string fillText = "", string[] buttons = null)
        {
            searchBar = new SearchBar(me, e, fillText);
            this.Controls.Add(searchBar);
            this.Size = new Size(searchBar.Width, searchBar.Height);

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Show();

            if (buttons != null)
            {
                this.Height *= 2;
            }
            this.TransparencyKey = this.BackColor = Color.Lime;
            this.TopMost = true;
            this.Focus();
            searchBar.LostFocus += S_LostFocus;
        }

        public bool gotFocus()
        {
            return (this.ContainsFocus);
        }
        private void S_LostFocus(object sender, EventArgs e)
        {
            this.Dispose();
        }      
    }

    class SearchBar : RichTextBox
    {
        //Form f = null;

        public MenuEntity b { get; set; } = null;
        public Action<MenuEntity, string> a;
        public bool closeOnLostFocus = true;
        public Control controller = null;

        public bool invoked = false;

        public SearchBar()
        {
            this.setUp();
        }

        public SearchBar( MenuEntity b, Action<MenuEntity, string> a, string fillText = "", int x = 0, int y = 0)
        {
            this.b = b;
            this.a = a;
            this.Width = b.Width * 2;
            this.Height = b.Height;
            if (fillText != "")
            {
                this.Text = fillText;
                SizeF ssize = new SizeF();
                var g = this.CreateGraphics();
                ssize = g.MeasureString(this.Text, this.Font);
                if (ssize.Width > this.Width)
                    this.Width = (int)ssize.Width;
                this.SelectAll();
            }
            this.setUp();
        }

        public void setSearch(MenuEntity b, Action<MenuEntity, string> a, bool copyClipBoard = true)
        {
            this.b = b;
            this.a = a;
            if (copyClipBoard && Clipboard.ContainsText())
            {
                string text = Clipboard.GetText().Trim();
                if (text.Contains("\n")) text = text.Substring(0, text.IndexOf("\n")-1);
                this.Text = text + " - ";
                this.SelectionStart = this.Text.Length;
            }
        }

        private void setUp()
        {
            this.KeyDown += SearchBar_KeyDown;
            //this.LostFocus += SearchBar_LostFocus;
            this.ScrollBars = RichTextBoxScrollBars.None;
        }

        private void SearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (closeOnLostFocus)
                    this.Dispose();
                else
                {
                    this.Clear();
                    if (controller != null) controller.Hide();
                }
                // this.Capture = false;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (this.Text != "")
                {
                    a.Invoke(this.b, this.Text.TrimEnd('\n'));
                    invoked = true;
                }
                if (closeOnLostFocus)
                    this.Dispose();
                else
                {
                    this.Clear();
                    this.SelectionStart = 0;
                    if (controller != null) controller.Hide();
                }
                invoked = false;
            }
        }
    }
}
