using NoteTrackerV3.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NoteTrackerV3
{
    class TrackerInfo
    {
        public string name = "";
        public int parent = -1;
        public int count = 0;
        public int total = 0;
        public RichTextBox rtb = null;
        public string copyInfo = "";
        public string description = "";
        public string options = "";
        public bool isTemp = false;
        public List<KeyValuePair<string, string>> searchList = null;
        public List<string> comments = null;
        public string fname = "";

        public string prefix = "";

        public List<string> claimInfo = null;
        public Dictionary<string, int> storeInfo = null;

        public TrackerInfo(string name = "", int count = 0, int parent = -1, string options = "")
        {
            this.name = name;
            this.parent = parent;
            this.count = count;
            this.options = options;
        }

        public TrackerInfo(TrackerInfo info)
        {
            name = info.name;
            count = info.count;
            parent = info.parent;
            if (info.rtb != null)
            {
                rtb = new RichTextBox();
                rtb.Rtf = info.rtb.Rtf;
            }
            copyInfo = info.copyInfo;
            description = info.description;
            options = info.options;
            isTemp = info.isTemp;
            if(info.searchList != null)
                searchList = new List<KeyValuePair<string, string>>(info.searchList);
            if (info.comments != null)
                comments = new List<string>(info.comments);

        }

        public void addRTF(string rtf)
        {
            if (this.rtb == null) rtb = new RichTextBox();
            this.rtb.Rtf = rtf;
        }

        public void addComment(string comment)
        {
            if (this.comments == null) this.comments = new List<String>();
            this.comments.Add(comment);
        }
        public void addStore(string store, int count = 1)
        {
            if (this.storeInfo == null) this.storeInfo = new Dictionary<string, int>();
            {
                if (this.storeInfo.ContainsKey(store))
                    this.storeInfo[store]++;
                else
                    this.storeInfo[store] = count;
            }
        }

        public void removeComment(string comment)
        {
            if (this.comments == null) this.comments = new List<String>();
            this.comments.Remove(comment);
        }

        public void addClaim(string text)
        {
            if (this.claimInfo == null) this.claimInfo = new List<String>();

            this.claimInfo.Add(text);
        }
    }

    class Tracker : Form
    {
        private MenuBar p_menuBar;
        private PadForm p_scratchPad = null;

        private Pad p_pad = null;

        //mouse events
        MouseEvent LeftClickEvent = null;
        MouseEvent ShowDataEvent = null;
        MouseEvent EditKpiEvent = null;
        MouseEvent CopyRTBEvent = null;
        MouseEvent ExitEvent = null;
        MouseEvent SpawnCopyBarEvent = null;
        MouseEvent SpawnAddBarEvent = null;

        SettingsWindow p_settingsWindow = null;

        //claim form part
        ClaimForm p_claimForm = null;
        StoreForm p_storeForm = null;


        public Tracker()
        {
            this.Icon = Resources.favicon;
            this.Name = "TrackerWindow";

            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Width = Screen.PrimaryScreen.Bounds.Width;

            this.TransparencyKey = this.BackColor = Color.Lime;
            this.DoubleBuffered = true;

            this.TopMost = settings.BoolOf("AlwaysOnTop");

            startSaveTimer();

            p_settingsWindow = new SettingsWindow(this);
            p_settingsWindow.Visible = false;
            p_settingsWindow.TopMost = settings.BoolOf("AlwaysOnTop");

            p_pad = new Pad();

            this.Location = new Point(settings.IntOf("TrackerX"), settings.IntOf("TrackerY"));

            Messenger.SettingChanged += Messenger_SettingChanged;
            this.FormClosing += Tracker_FormClosing;

            LeftClickEvent = new MouseEvent(LeftClickHandler, MouseEvents.LEFT_CLICK);
            ShowDataEvent = new MouseEvent(this.showData, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP);
            EditKpiEvent = new MouseEvent(this.editKPI, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP);
            CopyRTBEvent = new MouseEvent(copyRTB, MouseEvents.LEFT_CLICK);
            ExitEvent = new MouseEvent(exit, MouseEvents.LEFT_CLICK | MouseEvents.LEFT_LONG_UP);
            SpawnCopyBarEvent = new MouseEvent(spawnCopyBar, MouseEvents.MIDDLE_CLICK);
            SpawnAddBarEvent = new MouseEvent(spawnAddBar, MouseEvents.RIGHT_CLICK);

            p_scratchPad = new PadForm();
            p_scratchPad.setCallBack(this.reload);

            p_menuBar = new MenuBar(this, 0, this.Height, 5, Color.Lime);
            p_menuBar.useDragBar(true);
            loadMenu();

            p_claimForm = new ClaimForm();
            //this.Controls.Add(p_claimForm);
            p_claimForm.Hide();

            p_storeForm = new StoreForm();
            //this.Controls.Add(p_storeForm);
            p_storeForm.Hide();

            this.Controls.Add(p_menuBar);
        }

        private void Tracker_FormClosing(object sender, FormClosingEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private System.Timers.Timer p_timer = null;
        private void startSaveTimer()
        {
            p_timer = new System.Timers.Timer(settings.IntOf("AutoSaveTime"));
            p_timer.Elapsed += OnTimedEvent;
            p_timer.AutoReset = true;
            p_timer.Enabled = settings.BoolOf("AutoSave");
        }

        private void reload(Object o, EventArgs e)
        {
            //p_menuBar = new MenuBar(this, 0, this.Height, 5, Color.Lime);
            //p_menuBar.useDragBar(true);
            //loadMenu();           // this.loadMenu();

            p_menuBar.clear();
            loadMenu();
            GC.Collect();
        }
        private void save(Object o = null, EventArgs e = null)
        {
            settings.set("PadWindow", this.p_pad.Visible.ToString());
            settings.set("TrackerX", this.Location.X.ToString());
            settings.set("TrackerY", this.Location.Y.ToString());
            settings.set("TrackerPadX", this.p_scratchPad.Location.X.ToString());
            settings.set("TrackerPadY", this.p_scratchPad.Location.Y.ToString());
            settings.save();
            p_pad.save();
            saveFile();
        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            p_pad.save();
            saveFile();
        }

        private void showSettings(object arg1, EventArgs arg2)
        {
            this.p_settingsWindow.Visible = !this.p_settingsWindow.Visible;
            this.p_settingsWindow.BringToFront();
        }

        private void Messenger_SettingChanged(string arg1, string arg2, string arg3)
        {
            this.TopMost = settings.BoolOf("AlwaysOnTop");
            if (settings.BoolOf("AutoSave"))
                p_timer.Enabled = true;
            else
                p_timer.Enabled = false;
        }

        private void LeftClickHandler(Object o, EventArgs e)
        {
            MenuEntity me = ((MenuEntity)o);
            if(me.data != null)
            {
                if (((TrackerInfo)me.data).options.Contains("i"))
                {
                    ((TrackerInfo)me.data).count++;
                    if (p_scratchPad.Visible == true) p_scratchPad.showPad(p_menuBar, true);
                }
                if (((TrackerInfo)me.data).copyInfo != "")
                {
                    if (((ModifierKeys & Keys.Control) == Keys.Control || (ModifierKeys & Keys.Shift) == Keys.Shift) && Clipboard.GetText().Length > 0)
                    {
                        string old = Clipboard.GetText();
                        if (((TrackerInfo)me.data).copyInfo.Contains("()"))
                        {
                            Clipboard.SetText(((TrackerInfo)me.data).copyInfo.Replace("()", old).Replace("\\n", "\n"));
                        }
                        else if (old.Contains("()"))
                        {
                            Clipboard.SetText(old.Replace("()", ((TrackerInfo)me.data).copyInfo.Replace("\\n", "\n")));
                        }
                        else
                            Clipboard.SetText(old + ((TrackerInfo)me.data).copyInfo.Replace("\\n", "\n"));
                    }
                    else
                        Clipboard.SetText(((TrackerInfo)me.data).copyInfo.Replace("\\n", "\n"));
                }
            }
        }

        private void applyMenuOptions(MenuEntity me, string options, bool isMenu = false)
        {
            if (isMenu)
            {
                me.addMouseEvent(LeftClickEvent);

                if (options.Contains("d"))
                {
                    me.addMouseEvent(ShowDataEvent);
                }
                if (options.Contains("m"))
                {
                    try
                    {
                        this.p_menuBar.addSubMenu("save", me).addMouseEvent         (new MouseEvent(save,           MouseEvents.LEFT_CLICK));
                        this.p_menuBar.addSubMenu("edit", me).addMouseEvent         (EditKpiEvent);
                        this.p_menuBar.addSubMenu("reset", me).addMouseEvent        (new MouseEvent(resetKPI,       MouseEvents.LEFT_CLICK));
                        this.p_menuBar.addSubMenu("pad", me).addMouseEvent          (new MouseEvent(showPad,        MouseEvents.LEFT_CLICK));
                        this.p_menuBar.addSubMenu("settings", me).addMouseEvent     (new MouseEvent(showSettings,   MouseEvents.LEFT_CLICK));
                        this.p_menuBar.addSubMenu("exit", me).addMouseEvent         (ExitEvent);
                        me.addMouseEvent                                            (new MouseEvent(formatConnote,  MouseEvents.LEFT_CLICK));
                    }
                    catch { }
                }
            }
            else
            {
                me.addMouseEvent(LeftClickEvent);
                me.addMouseEvent(SpawnCopyBarEvent);
                me.addMouseEvent(SpawnAddBarEvent);
            }
        }

        private void showPad(object arg1, EventArgs arg2)
        {
            this.p_pad.Visible = !this.p_pad.Visible;
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        
        private void spawnCopyBar(object o, EventArgs arg2)
        {
            MenuEntity me = (MenuEntity)o;
            TrackerInfo ti = ((TrackerInfo)me.data);
            if (me.data == null) return;

            SearchForm sf = new SearchForm(me, copyBarHandler, ti.copyInfo);
            sf.Location = PointToScreen(new Point(me.Location.X + me.Width, me.Location.Y));
            SetForegroundWindow(sf.Handle);
        }
        private void spawnAddBar(object o, EventArgs arg2)
        {
            MenuEntity me = (MenuEntity)o;
            TrackerInfo ti = ((TrackerInfo)me.data);
            if (me.data == null) return;
            if (ti.options.Contains("c"))
            {
                p_claimForm.Location = PointToScreen(new Point(me.Location.X + me.Width, me.Location.Y));
                p_claimForm.show(me, claimBarHandler);
                p_claimForm.BringToFront();
                p_claimForm.Show();
                p_claimForm.searchBar.Focus();
                SetForegroundWindow(p_claimForm.Handle);

            }
            else if (ti.options.Contains("t"))
            {
                p_storeForm.show(me, storeBarHandler);
                p_storeForm.Location = PointToScreen(new Point(me.Location.X + me.Width, me.Location.Y));
                p_storeForm.BringToFront();
                p_storeForm.Show();
                p_storeForm.searchBar.Focus();
                SetForegroundWindow(p_storeForm.Handle);
            }
            else
            {
                SearchForm sf = new SearchForm(me, addBarHandler);
                sf.Location = PointToScreen(new Point(me.Location.X + me.Width, me.Location.Y));
                SetForegroundWindow(sf.Handle);
            }
        }
        
        private void storeBarHandler(MenuEntity me, string text)
        {
            me.closeMenuf();
            if (text.Length > 0 && text[0] == '!') searchBarOptionsHandler(me, text);
            else if(text.Length > 0)
            {
                TrackerInfo ti = (TrackerInfo)me.data;
                ti.addStore(text);
                ti.count++;
                if (p_scratchPad.Visible == true) p_scratchPad.showPad(p_menuBar, true);
            }
        }

        private void claimBarHandler(MenuEntity me, string text, string start, string end)
        {
            me.closeMenuf();
            if (text.Length > 0 && text[0] == '!') searchBarOptionsHandler(me, text);
            else
            {
                TrackerInfo ti = (TrackerInfo)me.data;
                ti.addClaim(formatText("[" + formatText(start, 15) + "->" + formatText(end, 15), 34) + "] - " + text);
                ti.count++;
                if (p_scratchPad.Visible == true) p_scratchPad.showPad(p_menuBar, true);
            }
        }

        private void copyBarHandler(MenuEntity me, string a)
        {
            me.closeMenuf();
            if (a.Length > 0)
            {
                if(a[0] == '!')
                {
                    searchBarOptionsHandler(me, a);
                }
                else if (me.data != null)
                {
                    ((TrackerInfo)me.data).copyInfo = a;
                }
            }
        }

        private void addBarHandler(MenuEntity me, string a)
        {
            me.closeMenuf();
            if (a.Length > 0)
            {
                if (a[0] == '!')
                {
                    searchBarOptionsHandler(me, a);
                }
                else if (me.data != null)
                {
                    searchBarOptionsHandler(me, "!a " + a);
                }
            }
        }

        private void searchBarOptionsHandler(MenuEntity me, string a)
        {
            me.closeMenuf();
            string b = a.ToLower();
            if (b.Length > 1)
            {
                switch (a[1])
                {
                    case 'a':
                        {
                            string prefix = "\t";
                            if(((TrackerInfo)me.data).prefix.Contains('\t'))
                                prefix += ((TrackerInfo)me.data).prefix.Substring(0, ((TrackerInfo)me.data).prefix.LastIndexOf('\t') + 1);
                            var x = addSubMenu(me, a.Replace("!a", "").Trim(), ((TrackerInfo)me.data).options, prefix, true);
                            if (x != null)
                            {
                                if (((TrackerInfo)x.data).options.Contains("i"))
                                    ((TrackerInfo)x.data).count++;
                                if (((TrackerInfo)x.data).copyInfo.Length == 0)
                                {
                                    ((TrackerInfo)x.data).copyInfo = ((TrackerInfo)me.data).copyInfo;
                                }
                                if (((TrackerInfo)x.data).copyInfo.Length > 0)
                                {
                                    Clipboard.SetText(((TrackerInfo)x.data).copyInfo);
                                }
                                if (a.Length > 0 && a[2] != '>') ((TrackerInfo)x.data).isTemp = true;//{ prefix += '<'; }

                                if (p_scratchPad.Visible == true) p_scratchPad.showPad(p_menuBar, true);
                                me.Refresh();
                            }
                            break;
                        }
                    case 'i':
                        {
                            if (a.Length > 4)
                                ((TrackerInfo)me.data).description = a.Substring(3);
                            break;
                        }
                    case 's':
                        {////////SET VALUE OF COUNT
                            int intout = 0;
                            if (me.data != null && int.TryParse(a.Substring(3), out intout))
                            {
                                if (intout < 0) ((TrackerInfo)me.data).count += intout;
                                else ((TrackerInfo)me.data).count = intout;
                                if (((TrackerInfo)me.data).count<0) ((TrackerInfo)me.data).count = 0;
                            }
                            break;
                        }
                    case 'd':
                        {////////DELETE BUTTON
                            me.deleteMenuItem();
                            break;
                        }
                    case '#':
                        {
                            me.setAutoOpenSub(false);
                            ((TrackerInfo)me.data).addComment(";#");
                            me.closeSubMenu();
                            break;
                        }
                    case '$':
                        {
                            me.setAutoOpenSub(true);
                            ((TrackerInfo)me.data).removeComment(";#");
                            me.showSubMenu();
                            break;
                        }
                }
            }
        }

        private void editKPI(object arg1, EventArgs arg2)
        {
            p_scratchPad.editKPI();
        }

        private void searchList(object o, EventArgs e)
        {
            try
            {
                MenuEntity me = ((MenuEntity)o);
                string clipboard = Clipboard.GetText().Trim().Replace("-0-", "-O-");
                string[] chars = new string[] {";", ",", "\'", "\"", "\n" };
                if (me.data != null && ((TrackerInfo)me.data).searchList != null)
                {
                    foreach (var i in ((TrackerInfo)me.data).searchList)
                    {
                        var match = Regex.Match(clipboard, "^" + i.Key + "$");
                        if (match.Success)
                        {
                            string url = i.Value.Replace("{}", clipboard);                                      //try prevent any abuse                                                                                                            
                            string snew = chars.Aggregate(url, (c1, c2) => c1.Replace(c2, ""));                 //make sure starts with URL
                            snew = snew.Replace(" ", "%20");    //fix spaces in url
                            if (snew.ToLower().StartsWith("http://") || snew.ToLower().StartsWith("https://"))
                                Process.Start("Chrome", snew);
                        }
                        else if (i.Key == ".+")
                        {
                            string snew = chars.Aggregate(i.Value, (c1, c2) => c1.Replace(c2, ""));                 //make sure starts with URL
                            if (snew.ToLower().StartsWith("http://") || snew.ToLower().StartsWith("https://"))
                                Process.Start("Chrome", snew);
                        }
                    }
                }
            }
            catch { }
        }

        private void exit(object o, EventArgs arg2)
        {
            if (((MenuEntity)o).getParent() != null)
            {
                ((MenuEntity)o).getParent().closeSubMenu();
            }
            DialogResult dialogResult = MessageBox.Show("Save?", "", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
                this.save();

            Messenger.closeForms("close");
        }

        private void clearCounts(MenuEntity me)
        {
            if (me.hasChildren())
            {
                foreach (MenuEntity n in me.getChildren())
                    clearCounts(n);
            }
            if (me.data != null)
            {
                ((TrackerInfo)me.data).count = 0;
                if (((TrackerInfo)me.data).isTemp)
                {
                    me.deleteMenuItem();
                }
                if(((TrackerInfo)me.data).claimInfo != null)
                {
                    ((TrackerInfo)me.data).claimInfo.Clear();
                }
                if (((TrackerInfo)me.data).storeInfo != null)
                {
                    ((TrackerInfo)me.data).storeInfo.Clear();
                }
            }
        }
        private void resetKPI(object o, EventArgs e)
        {
            if (((MenuEntity)o).getParent() != null)
            {
                ((MenuEntity)o).getParent().closeSubMenu();
            }
            DialogResult dialogResult = MessageBox.Show("Reset Stats?", "", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                for (int i = 0; i < p_menuBar.count(); i++)
                {
                    clearCounts(p_menuBar.getEntity(i));
                }
                if (p_scratchPad.Visible == true) p_scratchPad.showPad(p_menuBar, true);
            }
        }

        private MenuEntity addSubMenu(MenuEntity parent, string currentLine, string options, string prefix, bool side = false)
        {
            string lhs = Regex.Match(currentLine.Trim(' ', '>','<', '\t'), @"^[^\;\<\[\]\r\n]+(?=$|\<|\;)").Value;
            if (parent.getSubMenu(lhs) == null)
            {
                //new sub button, new tracker info
                var newButton = this.p_menuBar.addSubMenu(lhs, parent);
                if (newButton != null)
                {
                    if (side) newButton.isSideMenu = true;
                    TrackerInfo ti = new TrackerInfo(lhs, 0, parent.p_id, options);
                    ti.copyInfo = Regex.Match(currentLine, @"((?<=\;)[^\[\]\<\r\n]+(?=<|$))").Value;
                    var matches = Regex.Match(currentLine.TrimStart('<', '\t'), @"(?<=<)([^>\[\]\r\n]+)(?=>)");
                    int intout = 0;
                    if (int.TryParse(matches.Value, out intout))
                    {//no description

                        ti.count = intout;
                    }
                    else
                    {
                        ti.description = matches.Value;
                        ti.count = (matches.NextMatch().Success ? int.Parse(matches.NextMatch().Value) : 0);
                    }
                    ti.prefix = prefix;
                    applyMenuOptions(newButton, options);

                    if (currentLine.Trim('\t')[0] == '<' || prefix.Contains('<'))
                    {
                        ti.isTemp = true;
                        if (ti.count == 0 && ti.options.Contains("i")) ti.count = 1;
                    }

                    newButton.data = ti;

                    return newButton;
                }
            }
            return null;
        }

        //there's no hope for this one
        private bool loadMenu()
        {
            bool success = true;
            try
            {
                if (File.Exists("kpi.txt"))
                {
                    string currentLine = "", menuName = "", options = "";
                    MenuEntity currentMenuButton = null, currentSubMenuButton = null, lastEntity = null;
                    string lastEntityOptions = "";

                    int lastLevel = 0;

                    List<string> comments = new List<string>();

                    StreamReader sr = new StreamReader("kpi.txt");

                    while (!sr.EndOfStream)
                    {
                        currentLine = sr.ReadLine();
                        if (currentLine.Length > 0)
                        {
                            string trimmedline = currentLine.Trim('\t');
                            //MAIN MENU ADD
                            if (currentLine[0] == '[')
                            {
                                menuName = Regex.Match(currentLine, @"(?<=^\[)[a-zA-Z -\@_{-~\^\r\n]+(?=\])").Value;
                                if (menuName != "")
                                {
                                    currentMenuButton = this.p_menuBar.addMenuItem(menuName);
                                    options = Regex.Match(currentLine, @"\/.+$").Value;
                                    var match = Regex.Match(currentLine, @"(?<=<)([^>\[\]\r\n]+)(?=>)");
                                    currentMenuButton.data = new TrackerInfo(menuName, Int32.Parse(match.Success ? match.Value : "0"), -1, options);
                                    applyMenuOptions(currentMenuButton, options, true);
                                    lastEntity = currentMenuButton;
                                    lastEntityOptions = options;
                                }
                            }
                            else if (currentLine[0] == '/')
                            {
                                ((TrackerInfo)lastEntity.data).addClaim(currentLine.Trim('/'));
                            }
                            else if (trimmedline[0] == '\\')
                            {
                                MatchCollection mc = Regex.Matches(currentLine, @"(?<=~)([^~\r\n]+)(?=~)");
                                if(mc.Count > 1)
                                {
                                    ((TrackerInfo)lastEntity.data).addStore(mc[1].Value, Int32.Parse(mc[0].Value));
                                }
                            }
                            else if (currentLine[0] == ';')
                            {   //comments
                                comments.Add(currentLine);
                            }
                            else if (trimmedline[0] == '^')
                            {   //RTF MENU
                                loadRTFMenu(currentLine, lastEntity);    //RTF can be sub menu maybe
                            }
                            else if (trimmedline[0] == '~')
                            {   //SEARCH LIST
                                if (lastEntity != null && lastEntity.data != null)
                                {
                                    if (((TrackerInfo)lastEntity.data).searchList == null)
                                    {
                                        ((TrackerInfo)lastEntity.data).searchList = new List<KeyValuePair<string, string>>();
                                        lastEntity.addMouseEvent(new MouseEvent(this.searchList, MouseEvents.LEFT_CLICK));
                                        //((TrackerInfo)lastEntity.data).prefix += "~";
                                    }
                                    ((TrackerInfo)lastEntity.data).searchList.Add(
                                        new KeyValuePair<string, string>(Regex.Match(currentLine, @"(?<=~).*?(?!.*@)").Value.TrimEnd('@'), Regex.Match(currentLine, @"(@)(?!.*@).+$").Value.TrimStart('@'))
                                        );
                                }
                            }
                            else
                            {   //STANDARD LINE (SUB MENU)
                                if (currentMenuButton != null)
                                {
                                    if (currentLine[0] == '\t')
                                    {
                                        int level = 0; string prefix = "";
                                        for (int c = 0; c < currentLine.Length; c++)
                                        {
                                            if (currentLine[c] == '\t') { level++; prefix += "\t"; }
                                            else break;
                                        }
                                        if (level == 1)
                                        {   //last was sub menu
                                            lastEntity = addSubMenu(currentSubMenuButton, currentLine, lastEntityOptions, prefix, true);
                                        }
                                        else if (level > lastLevel)
                                        {   //new level from previous level
                                            lastEntity = addSubMenu(lastEntity, currentLine, lastEntityOptions, prefix, true);
                                        }
                                        else if (level < lastLevel)
                                        {
                                            lastEntity = addSubMenu(lastEntity.getParent().getParent(), currentLine, lastEntityOptions, prefix, true);
                                        }
                                        else
                                        {
                                            lastEntity = addSubMenu(lastEntity.getParent(), currentLine, lastEntityOptions, prefix, true);
                                        }
                                        lastLevel = level;
                                    }
                                    else
                                    {   //first level menu
                                        currentSubMenuButton = addSubMenu(currentMenuButton, currentLine, options, "", true);
                                        lastEntity = currentSubMenuButton;
                                        lastEntityOptions = options;
                                    }
                                }
                            }
                            if (currentLine[0] != ';' && lastEntity != null && comments.Count > 0)
                            {
                                if (((TrackerInfo)lastEntity.data).comments == null)
                                    ((TrackerInfo)lastEntity.data).comments = new List<string>();
                                foreach (var x in comments)
                                {
                                    string t = x.ToLower();
                                    if (t.Length > 1 && t[1] == '#')
                                    {
                                        if(lastEntity != null)
                                        {
                                            lastEntity.setAutoOpenSub(false);
                                        }
                                    }
                                    else if(t == ";addstore")
                                    {
                                        if(lastEntity != null)
                                        {
                                            ((TrackerInfo)lastEntity.data).options += "t";
                                            
                                            lastEntityOptions += "t";
                                        }
                                    }
                                    else if (t == ";addclaim")
                                    {
                                        if (lastEntity != null)
                                        {
                                            ((TrackerInfo)lastEntity.data).options += "c";
                                            lastEntityOptions += "c";
                                        }
                                    }
                                    ((TrackerInfo)lastEntity.data).comments.Add(x);
                                }
                                comments.Clear();
                            }
                        }
                    }
                    sr.Close();
                }       
            }
            catch { success = false; }

            return success;
        }

        private string _FILE = "";
        private void recursiveSave(MenuEntity me)
        {
            if (me.getChildren() != null)
            {
                foreach (var j in me.getChildren())
                {
                    if (j.data != null)
                    {
                        TrackerInfo ti = ((TrackerInfo)j.data);
                        if (!ti.options.Contains("x"))
                        {
                            if (ti.comments != null) foreach (var z in ti.comments) _FILE += z + "\n";
                            _FILE += ti.prefix + (ti.isTemp ? "<" : "") + ti.name.Trim() + (ti.copyInfo == "" ? "" : (";" + ti.copyInfo)) + (ti.description == "" ? "" : ("<" + ti.description + ">")) + "<" + ti.count + ">\n";
                            if (ti.fname != "") _FILE += ti.fname + "\n";
                            if (ti.searchList != null) { string pre = ""; pre = ti.prefix.Substring(0, ti.prefix.LastIndexOf('\t') + 1) + "~"; foreach (var z in ti.searchList) _FILE += pre + z.Key + "@" + z.Value + "\n"; }
                            if (ti.claimInfo != null) { string pre = "/"; foreach (var z in ti.claimInfo) _FILE += pre + z + "\n"; }
                            if (ti.storeInfo != null) { string pre = ""; pre = ti.prefix.Substring(0, ti.prefix.LastIndexOf('\t') + 1) + "\\"; foreach (var z in ti.storeInfo) _FILE += pre + "~" + z.Value + "~" + z.Key + "~" + "\n"; }
                        }
                    }
                    recursiveSave(j);
                }
            }
        }

        private void saveFile(Object o = null, EventArgs e = null)
        {
            _FILE = "";
            for(int i = 0; i< p_menuBar.count(); i++)
            {
                MenuEntity me = p_menuBar.getEntity(i);

                if (me.data!= null)
                {
                    TrackerInfo ti = ((TrackerInfo)me.data);
                    if(ti.comments != null)foreach(var z in ti.comments)_FILE += z+"\n";
                    _FILE += "[" + ti.name.Trim() + "]" + "<" + ti.count +">" + ti.options + "\n";
                    if (!ti.options.Contains("x"))
                    {
                        if (ti.fname != "") _FILE += ti.fname + "\n";
                        if (ti.searchList != null) foreach (var z in ti.searchList) _FILE += "~" + z.Key + "@" + z.Value + "\n";
                    }
                    recursiveSave(me);
                    _FILE += "\n\n";
                }
            }
            File.WriteAllText(@"kpi.txt", _FILE);
            _FILE = "";
        }

        private void loadRTFMenu(string filename, MenuEntity rootButton)
        {
            try
            {
                MenuEntity lastEntity = null;
                string fname = filename;
                filename = filename.Trim('\t', '^');
                if (File.Exists(filename))
                {
                    RichTextBox rtb = new RichTextBox();
                    rtb.LoadFile(filename);

                    ((TrackerInfo)rootButton.data).fname = fname;

                    rtb.SelectionStart = 0;
                    rtb.SelectionLength = 1;
                    bool heading = false;

                    MenuEntity newButton = null;
                    TrackerInfo ti = null;

                    int start = rtb.Find("{");
                    int end = rtb.Find(new char[] { '}' }, start);

                    char[] starD = new char[] { '{' };
                    char[] endD = new char[] { '}' };

                    while (start != -1 || end != -1)
                    {
                        rtb.SelectionStart = start + 1;
                        rtb.SelectionLength = end - start - 1;
                        start += (end - start);
                        if (heading)
                        {
                            ti.addRTF(rtb.SelectedRtf);
                            newButton.data = ti;
                            heading = false;
                        }
                        else
                        {
                            string header = rtb.SelectedText.TrimStart('\n', '{', '}');
                            if (header[0] == '\t' && lastEntity != null)
                            {
                                lastEntity.isSideMenu = true;
                                newButton = p_menuBar.addSubMenu(header.TrimStart('\t'), lastEntity);
                            }
                            else
                            {
                                newButton = p_menuBar.addSubMenu(header.TrimStart('\t'), rootButton);
                                lastEntity = newButton;
                            }
                            newButton.addMouseEvent(CopyRTBEvent);
                            ti = new TrackerInfo(newButton.Text, 0, rootButton.p_id, ((TrackerInfo)rootButton.data).options);
                            ti.options += "sx";
                            //ti.isTemp = true;
                            heading = true;
                        }
                        start = rtb.Find(starD, start);
                        if (start == -1) break;
                        end = rtb.Find(endD, start);
                    }
                }
            }
            catch { Debug.WriteLine("ISSUE LOADING RTFMENU"); }
        }

        private void copyRTB(object o, EventArgs e)
        {
            try
            {
                MenuEntity me = ((MenuEntity)o);
                RichTextBox temp = new RichTextBox();
                RichTextBox temp2 = new RichTextBox();

                if (me.data != null && ((TrackerInfo)me.data).rtb != null)
                {
                    TrackerInfo ti = ((TrackerInfo)me.data);
                    if (((ModifierKeys & Keys.Control) == Keys.Control || (ModifierKeys & Keys.Shift) == Keys.Shift) && Clipboard.GetText().Length > 0)
                    {

                        var dataobject = Clipboard.GetDataObject();
                        if (dataobject != null)
                        {
                            temp.Rtf = dataobject.GetData(DataFormats.Rtf).ToString();

                        }
                        else
                        {
                            temp.Rtf = Clipboard.GetText(TextDataFormat.Rtf);
                        }
                        if (temp.Text.Contains("$$$"))
                        {
                            //copy second half of rtb
                            temp.Select(temp.Text.IndexOf("$$$") + 3, temp.TextLength);
                            temp2.SelectionStart = 0;
                            temp2.SelectedRtf = temp.SelectedRtf;

                            temp.SelectedRtf = "";
                            temp.SelectionStart = temp.TextLength-3;
                            temp.SelectionLength = 3;
                            ti.rtb.Select(0, ti.rtb.TextLength);    //-1 for unavoidable \n at start
                            temp.SelectedRtf = ti.rtb.SelectedRtf;

                            temp.SelectionStart = temp.TextLength;
                            temp.SelectionLength = 0;
                            temp2.SelectAll();
                            temp.SelectedRtf = temp2.SelectedRtf;

                            //temp.SelectAll();
                            //temp.Copy();

                            var html = RtfPipe.Rtf.ToHtml(temp.Rtf);
                            ClipboardHelper.CopyToClipboard(html, temp.Text, temp.Rtf);
                        }
                    }
                    else
                    {
                        ((TrackerInfo)me.data).rtb.SelectAll();
                        temp.Rtf = ((TrackerInfo)me.data).rtb.SelectedRtf;
                        string code = (Clipboard.GetText().Trim());
                        if (code.Length <= 20)
                        {
                            temp.Rtf = temp.Rtf.Replace("()", code);
                        }
                        temp.Rtf = temp.Rtf.Replace("<greeting>", getGreeting());
                        //temp.SelectAll();
                        //temp.Copy();
                        var html = RtfPipe.Rtf.ToHtml(temp.Rtf);
                        ClipboardHelper.CopyToClipboard(html, temp.Text, temp.Rtf);
                    }
                }
            }
            catch { Debug.WriteLine("ISSUE COPYING RTB"); }
        }

        private string getGreeting()
        {
            DateTime currentTime = DateTime.Now;
            string                                                      ret = "Hi There";
            if (currentTime.Hour < 12 && currentTime.Hour >= 5)         ret = "Good Morning";
            else if (currentTime.Hour > 11 && currentTime.Hour < 17)    ret = "Good Afternoon";
            else                                                        ret = "Good Evening";
            return                                                      ret;
        }

        private void showData(Object o, EventArgs e)
        {
            //p_scratchPad.showPad(p_trackerInfos);
            p_scratchPad.showPad(p_menuBar);
        }
        ///################################################## HELPERS
        ///        
        //dont grab focus
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
                return baseParams;
            }
        }
        private string formatText(object field, int size, char c = ' ')
        {
            var value = field.ToString();
            if (value.Length == size) return value;
            return value.Length > size ? value.Substring(0, size) : value.PadRight(size, c);
        }
        private void formatConnote(Object o, EventArgs e)
        {
            string s = Clipboard.GetText();
            s = s.Replace(System.Environment.NewLine, "\t");

            RichTextBox rt = new RichTextBox();
            FontConverter fc = new FontConverter();
            rt.Font = fc.ConvertFromString("Consolas, 9pt") as Font;

            if (s.Length > 0)
            {
                try
                {
                    var arr = s.Split('\t');

                    for (int j = 0; j < arr.Length; j += 10)
                    {
                        if (arr[1 + j].ToLower().Contains("satchel"))
                            rt.Text += formatText("#" + arr[0 + j] + " Satchel; likely white: ", 30);
                        else if (arr[1 + j].ToLower().Contains("carton"))
                            rt.Text += formatText("#" + arr[0 + j] + " Carton; likely white/brown: ", 30);
                        else
                            arr[1 + j] += ":";

                        rt.Text += string.Format("{0} x {1} x {2} at {3}Labelled:{4}{5}{6}",
                            formatText(arr[3 + j] + "H", 6),
                            formatText(arr[4 + j] + "L", 6),
                            formatText(arr[5 + j] + "W", 6),
                            formatText(arr[7 + j] + "kg", 7),
                            formatText(arr[8 + j], 25),
                            arr[9 + j],
                            Environment.NewLine
                            );
                    }
                    rt.SelectAll();
                    rt.Copy();
                }
                catch
                {
                }
            }
        }
    }
}
