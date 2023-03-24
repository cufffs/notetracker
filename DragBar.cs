using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class DragBar : PictureBox
    {
        private Form p_f;

        private bool p_drag = false;
        private Point p_startPoint = new Point(0, 0);

        public bool colorMatchFG { get; set; } = true;

        public DragBar(Form f, int w, int h, Color bg)
        {
            this.p_f = f;
            this.Width = w;
            this.Height = h;
            this.BackColor = bg;

            this.MouseDown += DragBar_MouseDown;
            this.MouseUp += DragBar_MouseUp;
            this.MouseMove += DragBar_MouseMove;

            Messenger.SettingChanged += Messenger_SettingChanged;
        }

        private void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            if(colorMatchFG)
                this.BackColor = settings.colorOf("ButtonFGColor");
            else
                this.BackColor = settings.colorOf("ButtonBGColor");
        }

        private void DragBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.p_drag)
            {
                Point p1 = new Point(e.X, e.Y);
                Point p2 = this.p_f.PointToScreen(p1);
                Point p3 = new Point(p2.X - this.p_startPoint.X,
                                     p2.Y - this.p_startPoint.Y);
                this.p_f.Location = p3;
            }
        }

        private void DragBar_MouseDown(object sender, MouseEventArgs e)
        {
            this.p_startPoint = e.Location;
            this.p_drag = true;
        }

        private void DragBar_MouseUp(object sender, MouseEventArgs e)
        {
            this.p_drag = false;
        }
    }
}
