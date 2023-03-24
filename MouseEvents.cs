using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTrackerV3
{
    [Flags] public enum MouseEvents
    {
        LEFT_CLICK = 1,
        RIGHT_CLICK = 2,
        MIDDLE_CLICK = 4,
        LEFT_LONG_PRESS = 8,
        RIGHT_LONG_PRESS = 16,
        LEFT_LONG_UP = 32,
        RIGHT_LONG_UP = 64
    }

    public class MouseEvent{
        public Action<Object, EventArgs> mEvent = null;
        public MouseEvents mType;
        public int mId { get; set; }

        private static int p_Counter = 0;

        public MouseEvent(Action<Object, EventArgs> callback, MouseEvents type) 
        {
            mEvent = callback;
            mType = type;
            this.mId = System.Threading.Interlocked.Increment(ref p_Counter);
        }
    }
}
