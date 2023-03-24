using NoteTrackerV3.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class PadForm : Form
    {
        public RichTextBox _pad = null;

        private Stopwatch _timer;

        private TitleBar _tb = null;

        public bool closeOnLostFocus = false;
        public bool inEditKpi = false;

        Action<object, EventArgs> callback = null;

        Font f = null;

        //formatting stuff

        private Font headfont = null;
        Color desCol = Color.FromArgb(0xb23cc9);
        Color altRowCol = Color.FromArgb(0xedf5ef);
        Color mainRowCol = Color.White;
        Color altTextCol = Color.Brown;
        Color mainTextCol = Color.FromArgb(0x581845);

        private const int MAX_WIDTH = 124;
        private const int SECTION_HEADER = 0;
        private const int SECTION_0 = 11;
        private const int SECTION_1 = 0;
        private const int SECTION_2 = 0;
        private const int MAX_HEAD_NAME_LENGTH = 15;
        private const int MAX_TOTAL_LENGTH = 3;
        private const int TOTAL_CHANGE_COLOUR_1 = 10;
        private const int TOTAL_CHANGE_COLOUR_2 = 20;


        public PadForm()
        {
            this.Icon = Resources.favicon;
            this.setup();
        }

        private void setup()
        {
            _pad = new RichTextBox();

            this.TopMost = settings.BoolOf("AlwaysOnTop");
            this.BackColor = settings.colorOf("ButtonBGColor");
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TransparencyKey = this.BackColor = Color.Lime;

            this.Location = new Point(settings.IntOf("TrackerPadX"), settings.IntOf("TrackerPadY"));
            _pad.Location = new Point(0, 20);
            _pad.AcceptsTab = true;

            _pad.Size = new Size(1000, 500);
            this.Size = new Size(1000, 520);

            headfont = new Font(FontFamily.GenericMonospace, 14, FontStyle.Underline | FontStyle.Bold);

            f = (new FontConverter()).ConvertFromString("Consolas, 9pt") as Font;


            _pad.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            _pad.SelectionTabs = new int[] { 50, 140, 160, 250, 300 };
            _pad.Font = f;
            this.Controls.Add(_pad);

            _timer = new Stopwatch();

            _tb = new TitleBar(this, _pad.Width, 20, 5, settings.colorOf("ButtonBGColor"));

            var tohtml = _tb.addMenuItem("copy html");
            tohtml.addMouseEvent(new MouseEvent(copyHtml, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP));

            var b = this._tb.addMenuItem("x", 25, 0, this.Width-30);
            b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            b.addMouseEvent(new MouseEvent(min, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP));

            this.VisibleChanged += PadForm_VisibleChanged;

            _pad.KeyDown += PadForm_KeyDown;
            Controls.Add(_tb);
        }

        private void copyHtml(Object o, EventArgs e)
        {
            var html = RtfPipe.Rtf.ToHtml(_pad.Rtf);
            ClipboardHelper.CopyToClipboard(html, _pad.Text, _pad.Rtf);
        }

        public void setCallBack(Action<object, EventArgs> a)
        {
            callback = a;
        }

        private void PadForm_VisibleChanged(object sender, EventArgs e)
        {
            inEditKpi = false;
        }

        private void PadForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (inEditKpi && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.KeyCode == Keys.S)
                {
                    _pad.SaveFile("kpi.txt", RichTextBoxStreamType.PlainText);
                    if (callback != null)
                        callback.Invoke(sender, e);
                }
            }
        }

        void min(Object o, EventArgs e)
        {
            this.Visible = false;
        }

        void setPadStyle(Font f, Color c, Color bc)
        {
            _pad.SelectionFont = f;
            _pad.SelectionColor = c;
            _pad.SelectionBackColor = bc;
        }
        private string formatText(object field, int size, char c = ' ')
        {
            var value = field.ToString();
            if (value.Length == size) return value;
            return value.Length > size ? value.Substring(0, size) : value.PadRight(size, c);
        }

        private void appendPad(string text, Font f, Color textColour, Color backColour)
        {
            setPadStyle(f, textColour, backColour);
            _pad.AppendText(text);
        }

        public void updateCounts(MenuEntity me)
        {
            if (me.data != null)
                ((TrackerInfo)me.data).total = ((TrackerInfo)me.data).count;
            if (me.hasChildren())
            {
                MenuEntity n = null; int i = 0;
                while((n = me.getChild(i++)) != null)
                    updateCounts(n);
            }
            if (me.getParent() != null)
                if (me.getParent().data != null && me.data != null)
                    ((TrackerInfo)me.getParent().data).total += ((TrackerInfo)me.data).total;
        }

        private void insertHeader(TrackerInfo maind)
        {
            appendPad("    " + formatText(maind.name, MAX_HEAD_NAME_LENGTH) + "   [ total recorded ", headfont, Color.Black, Color.White);
            appendPad(formatText(maind.total.ToString(), MAX_TOTAL_LENGTH), headfont, maind.total >= TOTAL_CHANGE_COLOUR_1 ? Color.Green : Color.BlueViolet, Color.White);
            appendPad(formatText(" ]", 46, '_') + "\n", headfont, Color.Black, Color.White); 
            //46 is from total to end of line
        }

        //todo: make recursive
        public void showPad(MenuBar mb, bool keepshow = false)
        {
            string finlin = " ]";

            _pad.Clear();

            for(int i = 0; i < mb.count(); i++)
            {
                MenuEntity main = mb.getEntity(i);
                if (main != null && main.data != null)
                {
                    updateCounts(main);
                    TrackerInfo maind = ((TrackerInfo)main.data);

                    if (maind.total > 0)
                    {
                        var list = main.hasChildren() ? main.getChildren().OrderByDescending(p => ((TrackerInfo)p.data).total) : null;

                        insertHeader(maind);

                        if (list!=null)
                        {
                            int row = 0;
                            foreach (var j in list)
                            {
                                TrackerInfo ti = ((TrackerInfo)j.data);
                                if (ti.total > 0)
                                {
                                    //set colours
                                    Color rowCol = row % 2 == 0 ? altRowCol : mainRowCol;
                                    Color textCol = row % 2 == 0 ? altTextCol : mainTextCol;
                                    if (ti.total >= TOTAL_CHANGE_COLOUR_2) textCol = Color.Red; else if (ti.total >= TOTAL_CHANGE_COLOUR_1) textCol = Color.Green;

                                    //
                                    appendPad(formatText("|----", SECTION_0) + formatText(ti.total.ToString(), MAX_TOTAL_LENGTH) + formatText(ti.name.Trim('-', ' '), 25), f, textCol, rowCol);
                                    appendPad(ti.description.Length > 0 ? formatText("[ " + ti.description + " ]", 95) : formatText("", 95), f, desCol, rowCol);
                                    appendPad("----|\n", f, textCol, row % 2 == 0 ? Color.FromArgb(0xedf5ef) : Color.White);

                                    if (ti.claimInfo != null)
                                        foreach (var zulu in ti.claimInfo)
                                            appendPad(formatText("|", 14) + formatText("~", 4) + formatText(zulu, 120) + "|\n", f, textCol, rowCol);
                                    else if (ti.storeInfo != null)
                                        foreach (var zulu in ti.storeInfo)
                                            appendPad(formatText("|", 14) + formatText(zulu.Value + "x", 4) + formatText(zulu.Key, 120) + "|\n", f,textCol, rowCol);

                                    var list2 = j.hasChildren() ? j.getChildren().OrderByDescending(p => ((TrackerInfo)p.data).total) : null;
                                    if(list2 != null)
                                    {
                                        foreach(var z in list2)
                                        {
                                            ti = ((TrackerInfo)z.data);
                                            if (ti.total > 0)
                                            {
                                                textCol = row % 2 == 0 ? altTextCol : mainTextCol;
                                                if (ti.total >= 20) textCol = Color.Red; else if (ti.total >= 10) textCol = Color.Green;
                                                appendPad(formatText("|", 14) + formatText("~", 4) + formatText(((ti.total > 1) ? ti.total + "x -" : ""), 6) + formatText(ti.name.Trim(' ', '>'), 50), f, textCol, rowCol);
                                                appendPad(ti.description.Length > 0 ? formatText("[ " + ti.description + " ]", 64) : formatText("", 64), f, desCol, rowCol);
                                                appendPad("|\n", f, textCol, rowCol);

                                                if (ti.storeInfo != null)
                                                    foreach (var zulu in ti.storeInfo)
                                                        appendPad(formatText("|", 25)  + formatText(zulu.Value + "x", 3) + formatText(zulu.Key, 110) + "|\n", f, textCol, rowCol);
                                                else if (ti.claimInfo != null)
                                                    foreach (var zulu in ti.claimInfo)
                                                        appendPad(formatText("|", 29) + formatText(zulu, 109) + "|\n", f, textCol, rowCol);

                                                var list3 = z.hasChildren() ? z.getChildren().OrderByDescending(p => ((TrackerInfo)p.data).total) : null;
                                                if (list3 != null)
                                                {
                                                    foreach (var k in list3)
                                                    {
                                                        ti = ((TrackerInfo)k.data);
                                                        if (ti.total > 0)
                                                        {
                                                            textCol = Color.FromArgb(0x2a0a94);
                                                            if (ti.total >= 20) textCol = Color.DarkRed; else if (ti.total >= 10) textCol = Color.DarkGreen;

                                                            appendPad(formatText("|", 27) + formatText(((ti.total > 1) ? ti.total + "x -" : ""), 6) + formatText(ti.name.Trim(' ', '>'), 105) + "|\n", f, textCol, rowCol);

                                                            if (ti.storeInfo != null)
                                                                foreach (var zulu in ti.storeInfo)
                                                                    appendPad(formatText("|", 38) + formatText(zulu.Value + "x", 4) + formatText(zulu.Key, 96) + "|\n", f, textCol, rowCol);
                                                            else if (ti.claimInfo != null)
                                                                foreach (var zulu in ti.claimInfo)
                                                                    appendPad(formatText("|", 41) + formatText(zulu, 96) + "|\n", f, textCol, rowCol);
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                row++;
                            }
                        }
                    }
                }
            }
            if (!keepshow)
            {
                _pad.Select(0, 0);
                _pad.ScrollToCaret();
                if (inEditKpi)
                    this.Visible = true;
                else
                    this.Visible = !this.Visible;
                inEditKpi = false;
            }
        }

 
        public void editKPI()
        {
            if (!inEditKpi)
            {
                _pad.Clear();
                try
                {
                    String fpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                    fpath = fpath.Substring(0, fpath.LastIndexOf("\\")) + "\\";
                    _pad.LoadFile(fpath + "kpi.txt", RichTextBoxStreamType.PlainText);
                    this.colourBox();
                    this.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("editKPI : " + ex.Message);
                }
            }
            else
            {
                this.Hide();
            }
            inEditKpi = !inEditKpi;
        }

        private void colourBox()
        {
            _pad.BackColor = Color.AliceBlue;
            string[] lines = _pad.Lines;
            _pad.Text = "";

            Color c = Color.Black;

            foreach(string line in lines)
            {
                if (line.Length > 0)
                {
                    switch (line.Trim('\t')[0])
                    {
                        case ';':
                            c = Color.Purple;
                            break;
                        case '[':
                            c = Color.DarkRed;
                            break;
                        case '~':
                            c = Color.Blue;
                            break;
                        case '^':
                            c = Color.Green;
                            break;
                        default:
                            c = Color.Black;
                            break;
                    }
                }
                _pad.SelectionColor = c;
                _pad.AppendText(line + "\n");
            }

            _pad.SelectionStart = 0;
            _pad.SelectionLength = 0;
        }
    }
}
