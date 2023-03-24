using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Runtime.InteropServices;

    static class FontHelper
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        static bool IsMonospaced(Graphics g, Font f)
        {
            float w1, w2;

            w1 = g.MeasureString("i", f).Width;
            w2 = g.MeasureString("W", f).Width;
            return w1 == w2;
        }

        static bool IsSymbolFont(Font font)
        {
            const byte SYMBOL_FONT = 2;

            LOGFONT logicalFont = new LOGFONT();
            font.ToLogFont(logicalFont);
            return logicalFont.lfCharSet == SYMBOL_FONT;
        }

        /// <summary>
        /// Tells us, if a font is suitable for displaying document.
        /// </summary>
        /// <remarks>Some symbol fonts do not identify themselves as such.</remarks>
        /// <param name="fontName"></param>
        /// <returns></returns>
        static bool IsSuitableFont(string fontName)
        {
            return !fontName.StartsWith("ESRI") && !fontName.StartsWith("Oc_");
        }

        public static List<string> GetMonospacedFontNames()
        {
            List<string> fontList = new List<string>();
            InstalledFontCollection ifc;

            ifc = new InstalledFontCollection();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    foreach (FontFamily ff in ifc.Families)
                    {
                        if (ff.IsStyleAvailable(FontStyle.Regular) && ff.IsStyleAvailable(FontStyle.Bold)
                            && ff.IsStyleAvailable(FontStyle.Italic) && IsSuitableFont(ff.Name))
                        {
                            using (Font f = new Font(ff, 10))
                            {
                                if (IsMonospaced(g, f) && !IsSymbolFont(f))
                                {
                                    fontList.Add(ff.Name);
                                }
                            }
                        }
                    }
                }
            }
            return fontList;
        }
    }
    static class settings
    {
        static Dictionary<string, string> _stored;

        static settings()
        {
            _stored = new Dictionary<string, string>();
        }

        private static void loadDefault()
        {
            _stored["Name"] = "Name";
            _stored["TrackerWindow"] = "true";
            _stored["TrackerTempNewLine"] = "true";
            _stored["TrackerX"] = "300";
            _stored["TrackerY"] = "100";
            _stored["TrackerPadX"] = "100";
            _stored["TrackerPadY"] = "150";
            _stored["AutoSave"] = "true";
            _stored["AutoSaveTime"] = "900000";
            _stored["AutoSwap"] = "true";
            _stored["AltTab"] = "false";

            _stored["StoresFile"] = "stores.txt";
            _stored["ClaimsFile"] = "claim-states.txt";

            _stored["PadWindow"] = "true";

            _stored["ButtonFont"] = "Tahoma, 9.75pt";
            _stored["ButtonFGColor"] = "-12582784";
            _stored["ButtonBGColor"] = "-2298387";
            _stored["ButtonLeftDown"] = "-356783";
            _stored["ButtonWidth"] = "100";
            _stored["ButtonHeight"] = "20";
            _stored["MenuBarColor"] = "-444444";
            _stored["ButtonBGColorHover"] = "-128";
            _stored["ButtonFGColorHover"] = "-8355585";
            _stored["Button1ColorDown"] = "-155512";
            _stored["Button2ColorDown"] = "-32640";
            _stored["LongPressActivate"] = "700";

            //notepad
            _stored["NotepadBGColor"] = "-1774355";
            _stored["NotepadWidth"] = "400";
            _stored["NotepadHeight"] = "600";
            _stored["NotepadX"] = "0";
            _stored["NotepadY"] = "0";

            _stored["ZColor"] = "-65536";
            _stored["XColor"] = "-8388353";
            _stored["CColor"] = "-12550016";
            _stored["CopyColor"] = "-32576";
            _stored["NotepadTextColor"] = "-16777216";
            _stored["NotepadDateColor"] = "-16777056";
            _stored["NotepadTimeColor"] = "-8388608";
            _stored["NotepadTextFont"] = "Tahoma, 9.75pt";
            _stored["NotepadDateFont"] = "Tahoma, 11.25pt, style=Bold, Underline";
            _stored["NotepadTimeFont"] = "Tahoma, 9.5pt";

            _stored["AlwaysOnTop"] = "true";
        }

        internal static void save()
        {
            string file = "";
            foreach (var s in _stored)
            {
                file += s.Key + "=" + s.Value + System.Environment.NewLine;
            }
            try
            {
                File.WriteAllText(@"settings.txt", file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failure to save settings" + ex.ToString());
            }
        }

        public static bool loadSettings()
        {
            loadDefault();
            try
            {
                String fpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string line = "", name = "", value = "";
                if (fpath.Length > 0) { fpath = fpath.Substring(0, fpath.LastIndexOf("\\")) + "\\settings.txt"; }
                if (File.Exists(fpath))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(fpath);
                    while ((line = file.ReadLine()) != null)
                    {
                        int len = line.IndexOf("=");
                        name = line.Substring(0, len);
                        value = line.Substring(len + 1, line.Length - len - 1);
                        _stored[name] = value;
                        ///Debug.WriteLine("Loading Setting | name : " + name + " | value : " + value);
                    }
                    file.Close();
                    return true;
                }
                else
                {

                }
                return true;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); return false; }
        }

        public static void set(string name, string val, bool notify = false)
        {
            try
            {
                string old = _stored[name];
                _stored[name] = val;
                if (notify)
                    Messenger.settingChanged(name, val, old);
            }
            catch (Exception ex) { MessageBox.Show("setting val" + ex); }
        }

        public static string valueOf(string name)
        {
            string ret = "";
            try
            {
                if (_stored.TryGetValue(name, out ret))
                {
                    return ret;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            return "";
        }

        public static int IntOf(string name)
        {
            string temp = "";
            int ret = -1;
            try
            {
                if (_stored.TryGetValue(name, out temp))
                {

                    ret = int.Parse(temp);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            return ret;
        }

        public static bool BoolOf(string name)
        {
            string ret = "";
            try
            {
                if (_stored.TryGetValue(name, out ret))
                {
                    return bool.Parse(ret);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            return false;
        }

        public static Color colorOf(string name)
        {
            Color ret = Color.Black;
            String storedColor = "";
            try
            {
                if (_stored.TryGetValue(name, out storedColor))
                {
                    return Color.FromArgb(Convert.ToInt32(storedColor));
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            return ret;
        }

        public static void setColor(string name, Color c)
        {
            String col = c.ToArgb().ToString();
            try
            {
                string old = _stored[name];
                _stored[name] = col;
                Messenger.settingChanged(name, col, old);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static Font fontOf(string name)
        {
            var converter = new FontConverter();
            string storedFont = "";
            try
            {
                if (_stored.TryGetValue(name, out storedFont))
                {
                    return converter.ConvertFromString(storedFont) as Font;

                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            return SystemFonts.DefaultFont;
        }

        public static void setFont(string name, Font f)
        {
            var converter = new FontConverter();
            string ret = "";
            try
            {
                ret = converter.ConvertToString(f);
                string old = _stored[name];
                _stored[name] = ret;
                Messenger.settingChanged(name, ret, old);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

        }
    }
}
