using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class TitleBar : MenuBar
    {
        public TitleBar(Form form, int w, int h, int margin, Color bg)
            :base(form, w, h, margin, bg)
        {
            this.useDragBar(true);
            this.p_dragBar.BackColor = bg;
            this.p_dragBar.colorMatchFG = false;
            this.autoExpand = false;
            this.autoResize = false;
            this.spacer = false;
        }
        public void addFormControl()
        {
            var mb = new MenuEntity("-", this.Width - (50 / 2) - margin, this.Location.Y);
            mb.addMouseEvent(new MouseEvent(this.min, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP));
            mb.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            mb.Width = 25;
            this.p_menuItems.AddLast(mb);
            this.p_form.Controls.Add(mb);
        }

        public void addManualButton(string name, int w, int h, int x, int y)
        {
            var mb = new MenuEntity(name, w - margin, h, x, y);
            mb.addMouseEvent(new MouseEvent(this.min, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP));
            mb.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            mb.Width = 25;
            this.p_menuItems.AddLast(mb);
            this.p_form.Controls.Add(mb);
        }

        public void min(Object o, EventArgs e)
        {
            if (this.p_form != null)
                this.p_form.Hide();
        }
    }
}
