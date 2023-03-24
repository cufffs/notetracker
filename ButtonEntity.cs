using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class ButtonEntity : Label
    {
        static private int uid = 0;
        public Color bgColorOff { get; set; }
        public Color bgColorOn { get; set; }
        public Color fgColorOff { get; set; }
        public Color fgColorOn { get; set; }

        protected Stopwatch p_timer;

        public int _offset = 0;

        public bool hoverEffect { get; set; } = true;
        public bool minMenuAfterClick { get; set; } = false;
        public bool minMenuAfterLostFocus { get; set; } = false;
        public bool mouseInside { get; set; } = false;
        protected bool leftUp { get; set; } = true; 
        protected bool longPress { get; set; } = false;
        protected bool enteredWithMouseDown { get; set; } = false;

        public int p_id { get; private set; }

        //mouse clicks
        protected List<MouseEvent> p_mouseEntityEvents = null;

        public ButtonEntity(string name, int x = 0, int y = 0)
        {
            setUp(name, x, y);
            p_id = uid++;
        }

        ~ButtonEntity()
        {
            Console.WriteLine("~ButtonEntity()");
        }

        private void setUp(string name, int x, int y)
        {
            this.Text = name;
            this.p_mouseEntityEvents = new List<MouseEvent>();
            this.p_timer = new Stopwatch();

            this.Location = new Point(x, y);

            this.fgColorOff = this.ForeColor = settings.colorOf("ButtonFGColor");
            this.bgColorOff = this.BackColor = settings.colorOf("ButtonBGColor");
            this.bgColorOn = settings.colorOf("ButtonBGColorHover");
            this.fgColorOn = settings.colorOf("ButtonFGColorHover");
            this.Width = settings.IntOf("ButtonWidth");
            this.Height = settings.IntOf("ButtonHeight");
            this.Font = settings.fontOf("ButtonFont");
            this.TextAlign = ContentAlignment.TopCenter;

            this.MouseEnter += MenuEntity_MouseEnter;
            this.MouseLeave += MenuEntity_MouseLeave;
            this.MouseUp += MenuEntity_MouseUp;
            this.MouseDown += MenuEntity_MouseDown;
            this.MouseMove += MenuEntity_MouseMove;


            this.BringToFront();
        }

        public void addOnHover(Action<Object, EventArgs> a)
        {
            this.MouseEnter += a.Invoke;
        }

        public void addMouseEvent(MouseEvent me)
        {
            this.p_mouseEntityEvents.Add(me);
        }

        public void clearMouseEvents()
        {
            p_mouseEntityEvents.Clear();
            this.MouseEnter -= MenuEntity_MouseEnter;
            this.MouseLeave -= MenuEntity_MouseLeave;
            this.MouseUp -= MenuEntity_MouseUp;
            this.MouseDown -= MenuEntity_MouseDown;
            this.MouseMove -= MenuEntity_MouseMove;
        }

        public void removeMouseEvent(MouseEvent me)
        {
            this.p_mouseEntityEvents.Remove(me);
        }

        public void remove(int id)
        {
            var remove = this.p_mouseEntityEvents.SingleOrDefault(r => r.mId == id);
            if(remove != null)
                this.p_mouseEntityEvents.Remove(remove);
        }

        protected virtual void MenuEntity_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button != MouseButtons.None && longPress)
            {
                Control control = (Control)sender;

                if (control.Capture)
                    control.Capture = false;
            }
        }

        protected virtual void MenuEntity_MouseDown(object sender, MouseEventArgs e)
        {
            startMouseDown(sender, e);        
        }

        protected bool isMouseInside()
        {
            return this.ClientRectangle.Contains(this.PointToClient(Cursor.Position));
        }

        protected virtual void MenuEntity_MouseUp(object sender, MouseEventArgs e)
        {
            leftUp = true;
            p_timer.Stop();
            UpdateColours(true, true);

            if (!longPress && this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                if (e.Button.HasFlag(flag: MouseButtons.Left) && !enteredWithMouseDown)
                {
                    triggerMouseEvent(sender, e, MouseEvents.LEFT_CLICK);
                }
                else if (e.Button.HasFlag(flag: MouseButtons.Middle))
                {
                    triggerMouseEvent(sender, e, MouseEvents.MIDDLE_CLICK);

                }
                else if (e.Button.HasFlag(MouseButtons.Right))
                {
                    triggerMouseEvent(sender, e, MouseEvents.RIGHT_CLICK);
                }
            }
            else if (longPress&& this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                triggerMouseEvent(sender, e, MouseEvents.LEFT_LONG_UP);
            }
            enteredWithMouseDown = false;
        }

        protected void UpdateColours(bool backColour = false, bool foreColour = false)
        {
            if(backColour)
                this.BackColor = this.bgColorOn;
            else this.BackColor = this.bgColorOff;
            if (foreColour)
                this.ForeColor = this.fgColorOn;
            else this.ForeColor = this.fgColorOff;
        }

        protected virtual void MenuEntity_MouseLeave(object sender, EventArgs e)
        {
            mouseInside = false;
            if (hoverEffect)
            {
                UpdateColours(false, false);
            }
            enteredWithMouseDown = false;
        }

        protected virtual void MenuEntity_MouseEnter(object sender, EventArgs e)
        {
            handleMouseEnter(sender, e);
        }

        protected virtual void handleMouseEnter(object sender = null, EventArgs e = null)
        {
            mouseInside = true;

            if (hoverEffect)
            {
                UpdateColours(true, true);
            }
            if (Control.MouseButtons == MouseButtons.Left)
            {
                enteredWithMouseDown = true;
                if(sender != null)
                    startMouseDown(sender, e);
            }
        }

        private void startMouseDown(object sender, EventArgs e)
        {
            this.BackColor = settings.colorOf("Button1ColorDown");
            leftUp = false;
            longPress = false;
            p_timer.Restart();
            while ((Control.MouseButtons == MouseButtons.Left || Control.MouseButtons == MouseButtons.Right) && (!leftUp && p_timer.ElapsedMilliseconds < settings.IntOf("LongPressActivate")))
                Application.DoEvents();
            if (p_timer.ElapsedMilliseconds >= settings.IntOf("LongPressActivate"))
            {
                longPress = true;
                if(Control.MouseButtons == MouseButtons.Left)
                    triggerMouseEvent(sender, e, MouseEvents.LEFT_LONG_PRESS);
                else if(Control.MouseButtons == MouseButtons.Right)
                    triggerMouseEvent(sender, e, MouseEvents.RIGHT_LONG_PRESS);
            }
        }

        protected void triggerMouseEvent(object sender, EventArgs e, MouseEvents type)
        {
            foreach(var i in p_mouseEntityEvents)
            {
                if(i.mType.HasFlag(type))
                {
                    i.mEvent.Invoke(sender, e);
                }
            }
        }
    }
}
