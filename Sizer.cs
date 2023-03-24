using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class Sizer : PictureBox
    {
        Form form;
        int mx, my, sw, sh;
        bool mov;

        public Sizer(Form f, Point location)
        {
            this.form = f;

            this.Width = 10;
            this.Height = 10;
            this.Location = location;
            this.BackColor = settings.colorOf("ButtonFGColor");
            this.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            this.MouseUp += new MouseEventHandler(Control_MouseUp);
            this.MouseDown += new MouseEventHandler(Control_MouseDown);
            this.MouseMove += new MouseEventHandler(Control_MouseMove);
            this.BringToFront();
        }
        void Control_MouseDown(object sender, MouseEventArgs e)
        {
            mov = true;
            my = MousePosition.Y;
            mx = MousePosition.X;
            sw = this.form.Width;
            sh = this.form.Height;
        }

        void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == true)
            {
                this.form.Width = MousePosition.X - mx + sw;
                this.form.Height = MousePosition.Y - my + sh;
            }
        }

        void Control_MouseUp(object sender, MouseEventArgs e)
        {
            mov = false;
        }
    }
}
