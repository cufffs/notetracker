using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class MultiForm : ApplicationContext
    {
        private int _openForms;
        List<Form> _forms = null;

        public MultiForm(List<Form> forms)
        {
            _openForms = forms.Count;

            _forms = forms;

            Messenger.CreateForm += Messenger_CreateForm;
            Messenger.CloseForms += Messenger_CloseForms;

            foreach (var form in forms)
            {
                form.FormClosed += (s, args) =>
                {
                    _forms.Remove((Form)s);
                    if (Interlocked.Decrement(ref _openForms) == 0)
                    {
                        settings.save();
                        ExitThread();
                    }
                };
                form.Show();
            }
        }

        private void Messenger_CloseForms(string obj)
        {
            for (int i = _forms.Count - 1; i >= 0; i--)
            {
                _forms[i].Close();
            }
        }

        private void Messenger_CreateForm(string s)
        {
            bool isOpen = false;
            foreach (Form f in Application.OpenForms)
            {
                if (s.Equals(f.Name))
                {
                    isOpen = true;
                    f.Visible = !f.Visible; //show/hide
                }
            }
            if (!isOpen)
            {
                if (s == "TrackerWindow")
                {
                    //this.add(new PadForm());
                    this.add(new Tracker());
                }
                else if (s == "PadWindow")
                {
                    //this.add(new Pad());
                }
                settings.set(s, "true");
            }
        }

        private void add(Form form)
        {
            _openForms++;
            _forms.Add(form);
            form.FormClosed += (s, args) =>
            {
                _forms.Remove((Form)s);
                if (Interlocked.Decrement(ref _openForms) == 0)
                    ExitThread();
            };
            form.Show();
        }

        private void remove(string form)
        {
            foreach (Form f in _forms)
            {
                if (f.Name == form)
                {
                    _forms.Remove(f);
                }
            }
        }
    }
}
