using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    public partial class SettingsWindow : Form
    {
        Form parent = null;

        //public event delEventHandler ColorChanged;
        private TitleBar _tb = null;


        public SettingsWindow(Form parent)
        {
            InitializeComponent();
            //this.Icon = Resources.favicon;
            this.parent = parent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.LightGray;

            settings.colorOf("ButtonFGColor");
            this.ButtonFG.BackColor = settings.colorOf("ButtonFGColor");
            this.BGColor.BackColor = settings.colorOf("ButtonBGColor");
            this.FGHoverColor.BackColor = settings.colorOf("ButtonFGColorHover");
            this.BGHoverColor.BackColor = settings.colorOf("ButtonBGColorHover");
            this.notepadBG.BackColor = settings.colorOf("NotepadBGColor");
            this.Text_Color.BackColor = settings.colorOf("NotepadTextColor");
            this.Copy_Color.BackColor = settings.colorOf("CopyColor");
            this.C_Color.BackColor = settings.colorOf("CColor");
            this.X_Color.BackColor = settings.colorOf("XColor");
            this.Z_Color.BackColor = settings.colorOf("ZColor");
            this.TimeC.BackColor = settings.colorOf("NotepadTimeColor");
            this.DateColor.BackColor = settings.colorOf("NotepadDateColor");
            this.button1down.BackColor = settings.colorOf("Button1ColorDown");
            this.Button2down.BackColor = settings.colorOf("Button2ColorDown");


            _tb = new TitleBar(this, this.Width, 20, 5, settings.colorOf("ButtonBGColor"));
            var b = this._tb.addMenuItem("x", 25, 0, this.Width - 30);
            b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            b.addMouseEvent(new MouseEvent(min, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP));

            this.CenterToScreen();
            this.Location = new Point(Location.X, Location.Y - 300);
            this.Controls.Add(this._tb);
        }

        void min(Object o, EventArgs e)
        {
            this.Hide();
        }

        private bool validColor(Color c)
        {
            if (c == settings.colorOf("NotepadTextColor") || c == settings.colorOf("NotepadDateColor")
                || c == settings.colorOf("NotepadTimeColor") || c == settings.colorOf("CopyColor")
                || c == settings.colorOf("CColor") || c == settings.colorOf("XColor")
                || c == settings.colorOf("ZColor"))
            {
                return false;
            }
            return true;
        }

        private void ButtonFG_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("ButtonFGColor");
            cd.ShowDialog();
            settings.setColor("ButtonFGColor", cd.Color);
            this.ButtonFG.BackColor = cd.Color;

            cd.Dispose();
        }

        private void BGColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("ButtonBGColor");
            cd.ShowDialog();
            settings.setColor("ButtonBGColor", cd.Color);
            this.BGColor.BackColor = cd.Color;
            //ColorChanged?.Invoke();
            cd.Dispose();
        }

        private void FGHoverColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("ButtonFGColorHover");
            cd.ShowDialog();
            settings.setColor("ButtonFGColorHover", cd.Color);
            this.FGHoverColor.BackColor = cd.Color;
            //ColorChanged?.Invoke();
            cd.Dispose();
        }

        private void BGHoverColor_Click(object sender, EventArgs e)
        {

            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("ButtonBGColorHover");
            cd.ShowDialog();
            settings.setColor("ButtonBGColorHover", cd.Color);
            this.BGHoverColor.BackColor = cd.Color;
            //ColorChanged?.Invoke();
            cd.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("NotepadBGColor");
            cd.ShowDialog();
            settings.setColor("NotepadBGColor", cd.Color);
            this.notepadBG.BackColor = cd.Color;
            //ColorChanged?.Invoke();
            cd.Dispose();
        }

        private void AlwaysFront_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Windows Always on top?", "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                settings.set("AlwaysOnTop", "true", true);
            }
            else if (dialogResult == DialogResult.No)
            {
                settings.set("AlwaysOnTop", "false", true);
            }
            //ColorChanged?.Invoke();
        }

        private void DateColorClick(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("NotepadDateColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("NotepadDateColor", cd.Color);
                this.DateColor.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void TimeColor(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("NotepadTimeColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("NotepadTimeColor", cd.Color);
                this.TimeC.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void TextColor(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("NotepadTextColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("NotepadTextColor", cd.Color);
                this.Text_Color.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void Z_Color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("ZColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("ZColor", cd.Color);
                this.Z_Color.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void X_Color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("XColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("XColor", cd.Color);
                this.X_Color.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void C_Color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("CColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("CColor", cd.Color);
                this.C_Color.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void Copy_Color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("CopyColor");
            cd.ShowDialog();
            if (validColor(cd.Color))
            {
                settings.setColor("CopyColor", cd.Color);
                this.Copy_Color.BackColor = cd.Color;
            }
            cd.Dispose();
        }

        private void Text_Font_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = settings.fontOf("NotepadTextFont");
            fd.ShowDialog();
            settings.setFont("NotepadTextFont", fd.Font);
            this.Text_Font.Font = fd.Font;
            fd.Dispose();
        }

        private void button1down_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("Button1ColorDown");
            cd.ShowDialog();

            settings.setColor("Button1ColorDown", cd.Color);
            this.button1down.BackColor = cd.Color;
            cd.Dispose();
        }

        private void Button2down_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = settings.colorOf("Button2ColorDown");
            cd.ShowDialog();
            settings.setColor("Button2ColorDown", cd.Color);
            this.Button2down.BackColor = cd.Color;
            cd.Dispose();
        }

        private void newLine_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("New Lines for temp KPI INFO?", "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                settings.set("TrackerTempNewLine", "true");
            }
            else if (dialogResult == DialogResult.No)
            {
                settings.set("TrackerTempNewLine", "false");
            }
        }

        private void AutoSaveBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("AutoSave? *may cause crashing", "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                settings.set("AutoSave", "true");
            }
            else if (dialogResult == DialogResult.No)
            {
                settings.set("AutoSave", "false");
            }
        }
    }
}
