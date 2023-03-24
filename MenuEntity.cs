using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class MenuEntity : ButtonEntity
    {
        protected LinkedList<MenuEntity> p_children = null;
        public bool destroyOnLostFocus = false;

        private MenuEntity p_parent = null;
        public int subMenuWidth { get; set; } = 100;
        public int subMenuHeight { get; set; } = 20;

        private bool p_subMenuShown = false;
        public bool autoSubMenuWidth { get; set; } = true;
        public bool menuMatchSubMenuWidth { get; set; } = false;

        public bool isSideMenu { get; set; } = false;

        public bool alwaysShowMenu { get; set; } = false;

        public bool p_autoShow { get; private set; } = false;

        protected Form p_form = null;

        public Object data = null;

        private MouseEvent longPressEvent = null;

        //drag
        private Point p_dragStart;
        private bool p_dragging = false;
        private bool p_dragged = false;
        private MouseEventArgs p_storedArgs = null;


        public MenuEntity(string name, int x = 0, int y = 0, int w = 0, int h = 0, bool autoShow = true)
            :base(name, x, y)
        {
            this.Name = name;
            this.Location = new Point(x, y);
            this.Width = w == 0 ? subMenuWidth : w;
            this.Height = subMenuHeight = h ==0? settings.IntOf("ButtonHeight"):h;

            setAutoOpenSub(autoShow);
            p_autoShow = autoShow;

            Messenger.SettingChanged += Messenger_SettingChanged;

            this.LostFocus += MenuEntity_LostFocus;
        }

        ~MenuEntity()
        {
            Console.WriteLine("~MenuEntity()");
        }
        public void clear()
        {
            clearMouseEvents();
            Messenger.SettingChanged -= Messenger_SettingChanged;
            this.LostFocus -= MenuEntity_LostFocus;

            if (p_children != null)
            {
                foreach (var i in this.p_children)
                {
                    i.clear();
                }
            }
        }

        private void MenuEntity_LostFocus(object sender, EventArgs e)
        {
            if (destroyOnLostFocus)
            {
                this.clear();
                this.Dispose();
            }
        }

        protected virtual void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            this.fgColorOff = this.ForeColor = settings.colorOf("ButtonFGColor");
            this.bgColorOff = this.BackColor = settings.colorOf("ButtonBGColor");
            this.bgColorOn = settings.colorOf("ButtonBGColorHover");
            this.fgColorOn = settings.colorOf("ButtonFGColorHover");
        }

        public void setAutoOpenSub(bool s)
        {
            p_autoShow = s;
            if (s) { 
                this.MouseEnter += showSubMenu;
                if (longPressEvent != null)
                    this.removeMouseEvent(longPressEvent);
            }
            else
            {
                this.MouseEnter -= showSubMenu;
                if (longPressEvent == null)
                    longPressEvent = new MouseEvent(showSubMenu, MouseEvents.LEFT_LONG_PRESS);
                this.addMouseEvent(longPressEvent);
            }
        }

        public void setForm(Form f)
        {
            p_form = f;
        }

        private int calcFontWidth(string s, Font f)
        {
            if(s != null && f != null)
            {
                return (int)this.CreateGraphics().MeasureString(s, f).Width;
            }
            return 0;
        }

        public void setParent(MenuEntity p)
        {
            this.p_parent = p;
        }
        
        public MenuEntity getSubMenu(string s)
        {
            if (this.p_children != null)
            {
                foreach (var i in this.p_children)
                {
                    if (i.Text == s)
                        return i;
                }
            }
            return null;
        }

        public virtual MenuEntity addSubMenu(string s)
        {
            bool alreadyExist = false;

            int buttonWidth = this.subMenuWidth;

            if (s.Length > 0 && s[0] != '-')
                s = "    " + s;

            if (this.p_children == null)
                this.p_children = new LinkedList<MenuEntity>();

           
            int fontlen = calcFontWidth(s, this.Font);
            if ((fontlen+5) > this.subMenuWidth &&  this.autoSubMenuWidth)
                buttonWidth = fontlen + 5;

            foreach (var i in this.p_children)
            {
                if (i.Text == s)
                    alreadyExist = true;
            }

            if (!alreadyExist)
            {
                if(buttonWidth>this.subMenuWidth && this.autoSubMenuWidth)
                {
                    this.subMenuWidth = buttonWidth;
                    this.resetChildren();
                    foreach(var i in p_children)
                    {
                        i.Width = this.subMenuWidth;
                        i.resetChildren();
                    }
                }

                int newX = this.Location.X;
                int newY = this.Location.Y;
                if (this.isSideMenu)
                {
                    newX += this.Width;
                    if (this.p_children.Count >= 1) newY = this.p_children.Last.Value.Location.Y + this.p_children.Last.Value.Height;
                    else newY = this.Location.Y;
                }
                else
                {
                    if (this.p_children.Count >= 1) newY = this.p_children.Last.Value.Location.Y + this.p_children.Last.Value.Height;
                    else newY = this.Location.Y + this.Height;
                }

                MenuEntity t = new MenuEntity(s, newX, newY, buttonWidth, this.subMenuHeight, false);
                t.setParent(this);
                t.Hide();
                t.TextAlign = ContentAlignment.MiddleLeft;

                this.p_children.AddLast(t);

                bool autoShow = true;
                t.setAutoOpenSub(autoShow);
                if (autoShow)
                    t.MouseEnter += showSubMenu;

                if(p_form != null)
                {
                    p_form.Controls.Add(t);
                }

                if (this.p_autoShow && this.mouseInside)
                {
                    this.handleMouseEnter();
                    this.Refresh();
                    this.showSubMenu();
                }

                return t;
            }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if(p_children!=null && p_parent != null && p_children.Count>0)
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.Red, 5, ButtonBorderStyle.None,
                                         Color.Red, 5, ButtonBorderStyle.None,
                                         settings.colorOf("ButtonFGColor"), 4, ButtonBorderStyle.Inset,
                                         Color.Red, 5, ButtonBorderStyle.None);
        }

        protected override void MenuEntity_MouseDown(object o, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button.HasFlag(MouseButtons.Right))
            {
                p_dragStart = PointToClient(Cursor.Position);
                p_dragging = true;
                ((MenuEntity)o).BackColor = settings.colorOf("Button2ColorDown");
                p_storedArgs = e;
                p_timer.Restart();
                p_dragged = false;
            }
            else
            {
                //rare chance that click doesn't register with color change
                this.BackColor = settings.colorOf("Button1ColorDown");
            }
            if (!p_dragging) 
                base.MenuEntity_MouseDown(o, e);
        }

        protected override void MenuEntity_MouseLeave(object o, EventArgs e)
        {
            closeMenu(o, e);
            //if (!p_dragging)
            {
                base.MenuEntity_MouseLeave(o, e);
            }
            if (destroyOnLostFocus)
            {
                this.clear();
                this.Dispose();
            }
        }

        protected override void MenuEntity_MouseUp(object o, MouseEventArgs e)
        {
            if (p_dragging)
            {
                p_dragging = false;
                p_dragged = false;
                ((MenuEntity)o).BackColor = settings.colorOf("ButtonBGColor");
                p_timer.Stop();
                if (p_timer.ElapsedMilliseconds >= settings.IntOf("LongPressActivate"))
                {
                    triggerMouseEvent(o, e, MouseEvents.RIGHT_LONG_PRESS);
                }
                else
                {
                    triggerMouseEvent(o, e, MouseEvents.RIGHT_CLICK);
                }
            }
            else base.MenuEntity_MouseUp(o, e);
        }

        protected override void MenuEntity_MouseEnter(object o, EventArgs e)
        {
            //if (p_autoShow)
               // this.MouseEnter += showSubMenu;

               base.MenuEntity_MouseEnter(o, e);
        }

        protected override void MenuEntity_MouseMove(object o, MouseEventArgs e)
        {
            Point mouse = PointToClient(Cursor.Position);
            MenuEntity b = (MenuEntity)o;
            int dify = p_dragStart.Y - mouse.Y;
            if (p_dragging && p_parent != null)
            {
                if (dify < -(b.Height))
                {
                    p_dragged = _swap(b.p_parent.p_children.Find(b), b.p_parent.p_children.Find(b).Next);
                    p_dragStart = PointToClient(Cursor.Position);
                }
                else if (dify > (b.Height))
                {
                    p_dragged = _swap(b.p_parent.p_children.Find(b).Previous, b.p_parent.p_children.Find(b));
                    p_dragStart = PointToClient(Cursor.Position);
                }
            }
            base.MenuEntity_MouseMove(o, e);
        }

        public void moveChildren(int xOffset, int yOffset)
        {
            if (p_children != null)
            {
                foreach (var i in p_children)
                    i.moveChildren(xOffset, yOffset);
            }
            this.Location = new Point(this.Location.X + xOffset, this.Location.Y + yOffset);
            this.Refresh();
        }

        public void resetChildren()
        {
            if (p_parent != null)
            {
                int ind = p_parent.getChildIndex(this);
                int newx = p_parent.Location.X;
                int newy = p_parent.Location.Y + (ind * this.Height);
                if (p_parent.isSideMenu)
                {
                    newx += p_parent.Width;
                }
                else
                {
                    newy += this.Height;
                }
                this.Location = new Point(newx, newy);
            }
            if (p_children != null)
            {
                foreach (var i in p_children)
                    i.resetChildren();
            }
        }

        public int getChildIndex(MenuEntity me)
        {
            if(p_children != null)
            {
                return p_children.ToList().IndexOf(me);
            }
            return -1;
        }


        public void remove(MenuEntity me)
        {
            if (p_children != null)
            {
                p_children.Remove(me);
                me.deleteChildren();
                squashWidth();
                if (p_parent != null) p_parent.resetChildren();
                else resetChildren();
            }
        }

        void squashWidth()
        {
            MenuEntity me = this;
            if (p_parent != null) me = p_parent;

            int biggest = 0;

            SizeF ssize = new SizeF();
            var g = this.CreateGraphics();
            foreach (var i in me.p_children)
            {
                ssize = g.MeasureString(i.Text, i.Font);
                if (ssize.Width > biggest)
                    biggest = (int)ssize.Width + 1;
            }
            if (biggest < settings.IntOf("ButtonWidth"))
                biggest = settings.IntOf("ButtonWidth");
            
            foreach(var i in me.p_children)
            {
                i.Width = biggest;
            }
        
        }

        protected void deleteChildren()
        {
            if(p_children != null)
            {
                foreach (var i in p_children)
                    i.deleteChildren();
            }
            this.Dispose();
        }

        public void deleteMenuItem()
        {
            p_parent.remove(this);
            deleteChildren();
            this.Dispose();
        }


        public bool _swap(LinkedListNode<MenuEntity> first, LinkedListNode<MenuEntity> second)
        {
            if (first == null || second == null) 
                return false;
            var tmp = first.Value.Location;
            first.Value.Location = new Point(second.Value.Location.X, second.Value.Location.Y);
            second.Value.Location = new Point(tmp.X, tmp.Y);
            first.List.Remove(first);
            second.List.AddAfter(second, first);

            //handle children:
            Point dif = new Point(first.Value.Location.X - second.Value.Location.X, first.Value.Location.Y - second.Value.Location.Y);
            if(first.Value.p_children != null)
            {
                foreach(var i in first.Value.p_children)
                    i.moveChildren(dif.X, dif.Y);
            }
            if (second.Value.p_children != null)
            {
                foreach (var i in second.Value.p_children)
                    i.moveChildren(-dif.X, -dif.Y);
            }

            return true;
        }

        public MenuEntity getNext(MenuEntity me)
        {
            return p_children != null ? p_children.Find(me).Next.Value : null;
        }


        public MenuEntity getChild(string s)
        {
            MenuEntity ret = null;

            foreach(var i in p_children)
            {
                if (i.Name == s)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        public MenuEntity getChild(int i)
        {
            MenuEntity ret = null;
            if (p_children != null && i < p_children.Count)
                ret = p_children.ElementAt(i);
            return ret;
        }

        public MenuEntity getRoot()
        {
            MenuEntity mi = null;
            MenuEntity p = this.p_parent;

            while(p != null)
            {
                p = p.p_parent;
            }

            return mi;
        }

        public void showSubMenu(object sender=null, EventArgs e = null)
        {
            if (p_children != null)
            {
                foreach (var i in p_children)
                {
                    i.Show();
                }
                p_subMenuShown = true;
            }
        }

        public bool isSubShown()
        {
            return p_subMenuShown;
        }

        private bool safeToClose()
        {
            bool ret = false;

            if (this.p_children != null)
            {
                foreach (var i in p_children)
                {
                    if (i.safeToClose())
                        ret = true;
                }
            }

            return ret;
        }

        /*
         * goto to root, foreach child, goto child's child a
         */

        public MenuEntity getParent()
        {
            return p_parent;
        }

        public bool hasChildren()
        {
            if (p_children != null)
                return p_children.Count > 0;
            else return false; ;
        }

        public void printMenu()
        {
            Console.WriteLine("START: " + this.Name);
            if(p_children != null)
                foreach(var i in p_children)
                {
                    i.printMenu();
                }
            Console.WriteLine("END: " + this.Name);
        }

        public void closeSubMenu()
        {
            if (p_children != null)
            {
                foreach (var i in p_children)
                {
                    i.closeSubMenu();
                    i.Hide();
                    p_subMenuShown = false;
                }
            }     
        }

        public List<MenuEntity> getChildren()
        {
            if (p_children != null)
                return p_children.ToList();
            else return null;
        }

        public bool testSubMenu(bool inside = false)
        {
            if (isSubShown() && p_children != null)
            {
                foreach (var i in p_children)
                    inside = i.testSubMenu(inside);
            }
            if (inside) return true;
            else if (ClientRectangle.Contains(PointToClient(Cursor.Position)))  return true;
            else return false;
        }

        public virtual void closeMenuf()
        {
            if (p_parent != null)
            {
                p_parent.closeMenuf();
            }
            if (p_children != null)
            {
                foreach (var i in p_children)
                {
                    i.closeSubMenu();
                    i.Hide();
                }
                p_subMenuShown = false;
            }
            this.Refresh();
        }

        public virtual void closeMenu(object sender = null, EventArgs e = null)
        {
            if (alwaysShowMenu) return;

            if (p_parent != null)
            {
                p_parent.closeMenu();
            }
            //else
            //{
                if (p_children != null)
                {
                    foreach (var i in p_children)
                    {
                        if (i.testSubMenu()) //if is inside any sub menu
                        {
                            return; // probably bad but ohwell
                        }
                        else
                        {
                            i.closeSubMenu();
                        }
                    }

                    foreach (var i in p_children)
                    {
                        i.Hide();
                        //i.Visible = false;
                    }
                    p_subMenuShown = false;
                }
            //}
            this.Refresh();
        }


    }
}
