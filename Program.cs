using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
           // Application.SetCompatibleTextRenderingDefault(false);

            if (settings.loadSettings())
            {
                List<Form> forms = new List<Form>();

                if (settings.BoolOf("TrackerWindow"))
                {
                    forms.Add(new Tracker());
                }
                //if (settings.BoolOf("PadWindow"))
                    //forms.Add(new Pad());

                if (forms.Count > 0)
                {
                    Application.Run(new MultiForm(forms));
                }
                else
                {
                    forms.Add(new Tracker());
                    Application.Run(new MultiForm(forms));
                }
            }
        }
    }
}
