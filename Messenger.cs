using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTrackerV3
{
    public static class Messenger
    {
        public static event Action<string> CreateForm;
        public static void createForm(string s) => CreateForm?.Invoke(s);

        public static event Action<string> SaveForm;
        public static void saveForm(string s) => SaveForm?.Invoke(s);

        public static event Action<string> CloseForms;
        public static void closeForms(string s) => CloseForms?.Invoke(s);

        public static event Action<string, string, string> SettingChanged;
        public static void settingChanged(string s, string val, string old) => SettingChanged?.Invoke(s, val, old);

        public static event Action<string, object> ShowData;
        public static void showData(string s, object o = null) => ShowData?.Invoke(s, o);
    }
}
