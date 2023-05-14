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
        bool resize = false, mov = false;
        private Point p_startPoint = new Point(0, 0);
        int lastWidth = -1;

        public Sizer(Form f, Point location, int width, int height)
        {
            this.form = f;
            lastWidth = f.Width;

            this.Width = width;
            this.Height = height;
            this.Location = location;
            this.BackColor = settings.colorOf("ButtonFGColor");
            this.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            this.MouseUp += new MouseEventHandler(Control_MouseUp);
            this.MouseDown += new MouseEventHandler(Control_MouseDown);
            this.MouseMove += new MouseEventHandler(Control_MouseMove);
            this.MouseDoubleClick += Sizer_MouseDoubleClick;
            this.BringToFront();
        }

        private void Sizer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.form.Width == this.Width)
            {
                this.form.Width = lastWidth;
            }
            else
            {
                this.lastWidth = this.form.Width;
                this.form.Width = this.Width;
            }
        }

        void Control_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    resize = true;
                    my = MousePosition.Y;
                    mx = MousePosition.X;
                    sw = this.form.Width;
                    sh = this.form.Height;
                    break;
                case MouseButtons.Right:
                    this.p_startPoint = e.Location;
                    mov = true;
                    break;
            }

        }

        void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (resize == true)
            {
                this.form.Width = MousePosition.X - mx + sw;
                this.form.Height = MousePosition.Y - my + sh;
            }
            else if(mov == true)
            {
                Point p1 = new Point(e.X, e.Y);
                Point p2 = this.form.PointToScreen(p1);
                Point p3 = new Point(p2.X - this.p_startPoint.X, p2.Y - this.p_startPoint.Y);
                this.form.Location = p3;
            }
        }

        void Control_MouseUp(object sender, MouseEventArgs e)
        {
            resize = false;
            mov = false;
        }
    }
}
