using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class NotePd : RichTextBox
    {
        Form _form;
        TextBox content;

        private int _lastKey = -1;

        public NotePd(Form f)
        {
            _form = f;
            this.BackColor = settings.colorOf("NotepadBGColor");
            this.Font = settings.fontOf("NotepadTextFont");
            this.Width = this._form.Width - 4;
            this.Height = this._form.Height - settings.IntOf("ButtonHeight") - 2;
            this.Location = new Point(this.Location.X + 2, this.Location.Y + settings.IntOf("ButtonHeight"));
            this.BorderStyle = BorderStyle.None;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            //cursor
            //Settings.Default.SettingChanging += Default_SettingChanging;

            //keys
            this.KeyDown += Control_KeyDown;
            this.KeyUp += Control_KeyUp;
            this.PreviewKeyDown += new PreviewKeyDownEventHandler(Control_PreviewKeyDown);
            //mouse
            this.MouseClick += new MouseEventHandler(Control_MouseClick);
            //colors
            Messenger.SettingChanged += Messenger_SettingChanged;

            content = new TextBox();


            ///set up pad
            insertDate();
        }

        private Color string2Color(string c)
        {
            return Color.FromArgb(Convert.ToInt32(c));
        }

        private Font string2Font(string s)
        {
            var converter = new FontConverter();
            return converter.ConvertFromString(s) as Font;
        }

        private void Messenger_SettingChanged(string name, string val, string old)
        {
            this.BackColor = settings.colorOf("NotepadBGColor");
            if (name != "AlwaysOnTop" && name != "NotepadTextFont")
            {
                this.updateTextColour(string2Color(old), string2Color(val));
            }
            if (name == "NotepadTextFont")
            {
                this.updateTextFont(string2Font(old), string2Font(val));
            }
        }

        public void updateColours()
        {
            this.BackColor = settings.colorOf("NotepadBGColor");
            //this.Font = Settings.Default.NotepadTextFont;
        }

        /********************************
         * Utility
        *********************************/
        public void newPad(Object o, EventArgs e)
        {
            this.Clear();
            this.insertDate();
        }
        private void setStyle(Color c, Font f)
        {
            this.SelectionColor = c;
            this.SelectionFont = f;
        }
        private int selectLine(char[] delimters, int start)
        {
            if (start < 1) start = 1;

            int left = -1, right = this.Text.Length, pos = 0;

            foreach (char c in delimters)
            {
                pos = this.Text.LastIndexOf(c, start - 1);
                if (pos > left) left = pos;

                pos = this.Text.IndexOf(c, start);
                if (pos < right && pos != -1) right = pos;
            }
            if (left >= 0)
            {
                this.SelectionStart = left;
                this.SelectionLength = right - left;
            }
            else
            {
                this.SelectionStart = 0;
                this.SelectionLength = right;
            }

            return SelectionStart;
        }

        /*********************************
         * Date stuff
        *********************************/

        string getDate()
        {
            return DateTime.Now.ToString().Substring(0, DateTime.Now.ToString().IndexOf(" "));
        }

        bool DefaultText(Color c)
        {
            return (c != settings.colorOf("NotepadDateColor")
                     && c != settings.colorOf("NotepadTimeColor")
                     && c != Color.DarkRed
                     && c != Color.Blue
                     && c != Color.DarkOrchid);
        }
        public void updateTextFont(Font oldf, Font newf)
        {
            int curSelLen = 0;

            int i = 0;

            while (i < Text.Length)
            {
                this.SelectionStart = i;
                this.SelectionLength = 1;
                if (SelectionFont.Name == oldf.Name && SelectionFont.Size == oldf.Size && SelectionFont.Style == oldf.Style)
                    curSelLen++;
                else if (curSelLen > 0)
                {
                    this.SelectionStart = i - curSelLen;
                    this.SelectionLength = curSelLen;
                    this.SelectionFont = newf;
                    curSelLen = 0;
                }
                i++;
            }
            if (curSelLen > 0)
            {
                this.SelectionStart = i - curSelLen;
                this.SelectionLength = curSelLen;
                this.SelectionFont = newf;
            }
            this.SelectionLength = 0;
            this.Refresh();
            // this.Update();
        }

        public void updateTextColour(Color oldc, Color newc)
        {
            int curSelLen = 0;
            int oldStart = this.SelectionStart;
            int oldLen = this.SelectionLength;


            int i = 0;

            while (i < Text.Length)
            {
                this.SelectionStart = i;
                this.SelectionLength = 1;
                if (SelectionColor.ToArgb() == oldc.ToArgb())
                    curSelLen++;
                else if (curSelLen > 0)
                {
                    this.SelectionStart = i - curSelLen;
                    this.SelectionLength = curSelLen;
                    this.SelectionColor = newc;
                    curSelLen = 0;
                }
                i++;
            }
            if (curSelLen > 0)
            {
                this.SelectionStart = i - curSelLen;
                this.SelectionLength = curSelLen;
                this.SelectionColor = newc;
            }
            this.SelectionStart = oldStart;
            this.SelectionLength = oldLen;
        }

        public void updateTextColour(string reg, Color c)
        {
            int j = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                Match match = Regex.Match(this.Lines[i], reg);
                if (match.Success)
                {
                    Select(j, match.Length);
                    if (this.SelectionColor != Color.LightGreen)
                    {
                        this.SelectionColor = c;
                        this.SelectionLength = 0;
                    }
                }
                j += Lines[i].Length + 1;
            }
            this.SelectionStart = this.Text.Length;
        }

        public string getDate(RegexOptions way)
        {
            string ret = "";
            Match match = Regex.Match(this.Text, @"\[\s\d{1,2}\/\d{1,2}\/\d{4}\s\]", way);
            if (match.Success)
            {
                ret = match.Value;
            }
            return ret;
        }

        void insertDate()
        {
            this.setStyle(settings.colorOf("NotepadDateColor"), settings.fontOf("NotepadDateFont"));
            this.AppendText("[ " + getDate() + " ]\n");
            this.setStyle(settings.colorOf("NotepadTextColor"), settings.fontOf("NotepadTextFont"));
        }

        void updateDate()
        {
            Match match = Regex.Match(this.Text, getDate(), RegexOptions.RightToLeft);

            if (!match.Success)
            {
                insertDate();
            }
        }

        /*********************************
        * Time stuff
        *********************************/

        string getTime()
        {
            String s = DateTime.Now.ToString();
            return s.Substring(s.IndexOf(' '), s.Length - s.LastIndexOf(':') - 1) + s[s.Length - 2] + s[s.Length - 1];
        }

        void insertTime()
        {
            updateDate();
            this.setStyle(settings.colorOf("NotepadTimeColor"), settings.fontOf("NotepadTimeFont"));
            this.AppendText("[" + this.getTime() + "]");
            this.setStyle(settings.colorOf("NotepadTextColor"), settings.fontOf("NotepadTextFont"));
            this.AppendText(" ");
        }

        /*********************************
         * Completed stuff
        *********************************/

        void markAsComplete()
        {
            int initialStart = this.SelectionStart;

            if (this.Text.Length > 0)
            {
                selectLine(new char[] { '\n', '[' }, this.SelectionStart);
                this.SelectionColor = Color.LightGreen;
            }
            this.SelectionStart = initialStart;
            this.SelectionLength = 0;
        }

        public void clearComplete(Object o, EventArgs e)
        {
            int j = 0;
            int initalStart = this.SelectionStart;

            foreach (var i in Lines)
            {
                if (i.Length > 0)
                {
                    Select(j, i.Length);
                    if (SelectionColor == Color.LightGreen)
                    {
                        SelectionLength += 1;
                        SelectedText = "";
                        continue;
                    }
                }
                j += i.Length + 1;
            }
            this.SelectionStart = initalStart;
            this.SelectionLength = 0;

        }

        /*********************************
         * Input Related
        *********************************/

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void Control_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {  ///auto copy word if ctrl+click

                selectLine(new char[] { '\n', ' ' }, this.SelectionStart);
                SelectionStart += 1;

                try
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Clipboard.SetText(this.SelectedText);
                }
                catch { }
            }
        }

        private void Control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (this.Text.Length > 0 && this.SelectionStart > 1 &&
                    ((ModifierKeys & Keys.Control) != Keys.Control
                    && (ModifierKeys & Keys.Alt) != Keys.Alt
                    && (ModifierKeys & Keys.Escape) != Keys.Escape
                    && (ModifierKeys & Keys.Shift) != Keys.Shift
                    ) || _lastKey == -1)
                {
                    if (this.Text[this.SelectionStart - 1] == '\n' && _lastKey != (int)Keys.Back && e.KeyCode != Keys.Back)
                        this.insertTime();
                }
            }
            catch { }
        }

        private bool isCommand(KeyEventArgs e)
        {
            bool ret = false;
            if ((ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                switch ((int)e.KeyCode)
                {
                    //swap colours
                    case (int)Keys.Z: this.setStyle(settings.colorOf("ZColor"), settings.fontOf("NotepadTextFont")); ret = true; break;
                    case (int)Keys.X: this.setStyle(settings.colorOf("XColor"), settings.fontOf("NotepadTextFont")); ret = true; break;
                    case (int)Keys.C: this.setStyle(settings.colorOf("CColor"), settings.fontOf("NotepadTextFont")); ret = true; break;

                    //done
                    case (int)Keys.D: markAsComplete(); ret = true; break;
                }

                try { e.SuppressKeyPress = true; }
                catch { }
                ret = true;
            }
            else if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                switch ((int)e.KeyCode)
                {
                    //swap colours
                    case (int)Keys.V:
                        if (_lastKey == -1) insertTime();
                        this.setStyle(settings.colorOf("CopyColor"), settings.fontOf("NotepadTextFont"));
                        try
                        {
                            this.SelectedText = (string)Clipboard.GetData("Text");
                        }
                        catch { }
                        this.setStyle(settings.colorOf("NotepadTextColor"), settings.fontOf("NotepadTextFont"));
                        e.Handled = true;
                        break;
                    case (int)Keys.S:
                        try
                        {
                            String fname = this.Text.Substring(0, this.Text.IndexOf('\n'));
                            fname = fname.Replace('/', '-');
                            String fpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                            fpath = fpath.Substring(0, fpath.LastIndexOf("\\"));
                            if (fname.Length >= 0)
                            {
                                this.SaveFile(fpath + "\\" + fname + ".rtf");
                            }
                        }
                        catch
                        {
                            //MessageBox.Show("Failed to save" + ex);
                        }
                        e.Handled = true;
                        break;
                }
                ret = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.setStyle(settings.colorOf("NotepadTextColor"), settings.fontOf("NotepadTextFont"));
                ret = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                //insertTime();
            }
            else if (e.KeyCode == Keys.Back)
            {
                ret = true;
            }
            return ret;
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isCommand(e))
            {
                if (_lastKey == (int)e.KeyCode && e.KeyCode == Keys.Enter)  //don't handle multiple enters in a row.
                {
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    this.AppendText("\n");
                    this.insertTime();
                    e.Handled = true;
                    this.setStyle(settings.colorOf("NotepadTextColor"), settings.fontOf("NotepadTextFont"));
                }
            }
            _lastKey = (int)e.KeyCode;
        }
    }
}
