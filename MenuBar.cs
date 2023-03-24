using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class MenuBar : PictureBox
    {
        protected LinkedList<MenuEntity> p_menuItems;
        protected Form p_form = null;
        protected DragBar p_dragBar = null;

        protected bool p_mouseDrag = false;
        protected Point p_mouseDragStartPoint = new Point(0, 0);

        public bool autoExpand { get; set; } = false;
        public bool autoResize { get; set; } = true;

        public int margin { get; set; } = 0;

        public bool spacer { get; set; } = true;

        public MenuBar(Form form, int w, int h, int margin, Color bg)
        {
            this.p_form = form;
            this.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.Location = new Point(0, 0);
            this.margin = margin;
            this.Width = (w > 0 ? w : margin * 2); //padding =5
            this.Height = h;
            this.BackColor = bg;

            this.p_menuItems = new LinkedList<MenuEntity>();

            this.MouseDown += new MouseEventHandler(MEH_MouseDown);
            this.MouseUp += new MouseEventHandler(MEH_MouseUp);
            this.MouseMove += new MouseEventHandler(MEH_MouseMove);

            Messenger.SettingChanged += Messenger_SettingChanged;
        }

        public void clear()
        {
            foreach(var i in p_menuItems)
            {
                i.clear();
                i.Dispose();
            }
            p_menuItems.Clear();
        }

        private void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            //  throw new NotImplementedException();
            Console.WriteLine("..........");
        }


        public void useDragBar(bool t)
        {
            if (t)
            {
                if(p_dragBar == null)
                {
                    p_dragBar = new DragBar(p_form, this.Width, settings.IntOf("ButtonHeight"), settings.colorOf("ButtonFGColor"));
                    this.Controls.Add(p_dragBar);
                }
            }
        }

        public int count()
        {
            return p_menuItems.Count;
        }

        public MenuEntity getEntity(int i)
        {
            if (i < p_menuItems.Count)
                return p_menuItems.ElementAt(i);
            else
                return null;
        }

        public MenuEntity addMenuItem(string s, int w = 0, int h = 0, int x = 5, int y = 0)
        {
            MenuEntity me = new MenuEntity(s);

            if (this.p_menuItems.ToList().Exists(z => z.Text == s)) return null;

            me.Width = (w == 0 ? settings.IntOf("ButtonWidth") : w);
            me.Height = (h == 0 ? settings.IntOf("ButtonHeight") : h);
            if (x == 5)
            {
                x = this.p_menuItems.Count > 0 ? this.p_menuItems.Last().Location.X + this.p_menuItems.Last().Width : 5;
            }

            if (!spacer)
            {
                me.Text = me.Text.Trim();
            }
            
            me.Location = new Point(x, y);

            me.BringToFront();
            me.Show();
            this.p_menuItems.AddLast(me);
            this.Controls.Add(me);

            if (autoExpand) expand();
            else if (autoResize) resize();

            if(p_dragBar != null)
            {
                p_dragBar.SendToBack();
            }

            //for movement within the bar
            me.MouseDown += ME_MouseDown;
            me.MouseUp += ME_MouseUp;
            me.MouseMove += ME_MouseMove;

            return me;
        }

        public MenuEntity getMenuItem(string s)
        {
            MenuEntity ret = null;

            foreach(var i in p_menuItems)
            {
                if(i.Name == s)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        private MenuEntity findParent(string name)
        {
            MenuEntity ret = null;
            name = name.ToLower();

            foreach(var i in this.p_menuItems)
            {
                if(i.Name.ToLower().Trim() == name)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        public MenuEntity addSubMenu(string submenu, MenuEntity parent)
        {
            MenuEntity sub = null;
            try
            {
                sub = parent.addSubMenu(submenu);
                if (sub != null)
                {
                    if (!spacer)
                    {
                        sub.Text = sub.Text.Trim();
                        sub.TextAlign = ContentAlignment.MiddleCenter;
                    }
                        
                    this.Controls.Add(sub);
                }
            }
            catch { }
            return sub;
        }

        public MenuEntity addSubMenu(string basemenu, string submenu)
        {
            MenuEntity subMenu = null;
            MenuEntity baseMenu = findParent(basemenu);

            if (baseMenu != null)
            {
                subMenu = baseMenu.addSubMenu(submenu);
                if (subMenu != null)
                {
                    this.Controls.Add(subMenu);
                }
            }

            return subMenu;
        }

        private int getMinWidth()
        {
            int w = margin * 2;
            foreach(var i in p_menuItems)
                w += i.Width;

            return w;
        }

        private void expand()
        {
            int minWidth = getMinWidth();
            if(this.Width < minWidth)
            {
                this.Width = minWidth;
               // if(p_dragBar != null) { p_dragBar.Width = minWidth; }
               // p_dragBar.SendToBack();
            }
        }

        private void resize()
        {
            this.Width = getMinWidth();
            if (p_dragBar != null) { p_dragBar.Width = getMinWidth(); p_dragBar.SendToBack(); }

        }

        private void ME_MouseMove(object o, MouseEventArgs e)
        {
            if (p_mouseDrag)
            {
                Point mouse = PointToClient(Cursor.Position);
                MenuEntity b = (MenuEntity)o;
                int dif = p_mouseDragStartPoint.X - mouse.X;
                if (dif < -b.Width)
                {
                    b._swap(p_menuItems.Find(b), p_menuItems.Find(b).Next);
                    p_mouseDragStartPoint = PointToClient(Cursor.Position);
                }
                else if (dif > b.Width)
                {
                    b._swap(p_menuItems.Find(b).Previous, p_menuItems.Find(b));
                    p_mouseDragStartPoint = PointToClient(Cursor.Position);
                }
            }
        }

        private void ME_MouseUp(object sender, MouseEventArgs e)
        {
            p_mouseDrag = false;
        }

        private void ME_MouseDown(object o, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Right))
            {
                p_mouseDragStartPoint = PointToClient(Cursor.Position);
                p_mouseDrag = true;
                //((MenuEntity)o).BackColor = settings.colorOf("Button2ColorDown");
            }
        }

        private void MEH_MouseMove(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void MEH_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void MEH_MouseDown(object sender, MouseEventArgs e)
        {
        }
    }
}
