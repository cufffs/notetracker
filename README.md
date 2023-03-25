**NoteTracker – read me / how to :** quite long to read, and probably unnecessary unless you want to optimize it for your own work flow / make use of all features

_Important files:_

**Settings.txt** – holds the settings, such as position, colours etc. This can be deleted to restore default settings

**kpi.txt –** required to run app, holds all the menu structure + any current stats

**notetracker.exe** – required, this is the actual app

**[DATE].rtf –** these are the save files from the pad system, they save by the date on line zero. _Don't delete the date stamps!_

**stores.txt/claim-states.txt** – used to fill in the right click menu – use default as reference and add extra buttons as required.

Can be renamed in settings.txt

Limited to one tab per default file STORES FILE

Limited to 2 lines, comma separated for CLAIMS FILE!

**\*.rtf** – you can load your own template files by adding ^FILENAME.rtf where you want the templates to load in. Below screenshot shows how the kpi.txt layout looks along with a short description in red, followed by how it looks.

The layout of these files is simple, use sets of {}, ie {BUTTON NAM} {TEMPLATE}

Only stuff in the {} are read, so you can leave headings and such outside for easy reading of template files

Make sure you match all curly brackets, if you forget to open/close it will not load correctly!



**GENERAL WORK FLOW TIPS**

![nt3](https://user-images.githubusercontent.com/14177650/227681739-ed31b68f-7daa-4da5-bf99-d417ecde7669.gif)

![nt1](https://user-images.githubusercontent.com/14177650/227681828-0e67240b-86a4-446d-9b5c-95bd520e4d40.png)

- It is designed largely for my workflow ie lots of calls, Zendesk and Claims, but should be versatile enough to help most work flows!
- Right click + drag buttons to put them in positions that work for you.
- Modify settings.txt to change auto save timer, and long press activate.
  - If you hold mouse in too long it won't count the click, you can extend 'click time' by making long press activate setting longer but this will also make long press activate longer so long pressing to open submenu will take longer
- Best combined with my chrome extension to make tracking consignments and emailing claims/enquiry a bit faster.
- If you prefer using mouse only then it still works well, but using ctrl+C/V/TAB is much faster
- Emails:
  - Copy claim/order/tracking number/Zendesk ticket to clipboard and hit search / or submenu of search depending on what you want to do. This will load up a new tab in claim and get you to the tracking/order/Zendesk/claim/etc.
  - Copy claim/order/tracking to clipboard, and hit a template that will auto insert the clipboard into a preset template and add it all to clipboard – paste it into an email for a formatted email.
    - You can also shift+click on a second template and if the first template has '$$$' inside it, then the second template gets inserted there (good for adding specific batch photos/locations into generic template)
- Sales:
  - I always have issues with the browser saving address' for prefilling forms so I keep all stores and contactless phones in menu and click when needed.
    - Since the app doesn't take focus you can -\> click -\> ctrl+V -\> tab -\> click another button -\> ctrl+V so you can add phone+address in a second with a few clicks of keyboard and mouse
  - I tend to keep a few common but annoying to get to SKUs so I can simply click to copy it - such as 2x swag pole combo, awning pole combo
- Calls:
  - You can middle click on buttons to see their copy info – so it is super quick to check store address or phone numbers!
  - Click on button to increment it's count, and it will copy preset message to clipboard, copy that into comment to describe the call!
    - Lots of calls involve multiple things, so you can shift+click to append notes and increment multiple counts
      - Ie if you click 'invoice' and then shift click 'pre order' – your clipboard may be "SENT CUSTOMER INVOICE, ADVISED OF PRE ORDER DATE"
  - I tend to go straight to WMS order, check order details etc, then connotes page and use chrome extension to jump straight to courier tracking page
  - If about existing claim, right clicking existing claim button will auto copy clipboard (ie claim code) to the text bar, and allow you to set START and END states of claim ie OPEN-\>REVIEW
  - If there is an issue – I right click the issues button and record a short note and order number – if required elaborating inside EOD.
- End of Day
  - Count my calls and then right click the calls button and type '!s NUMBER' i.e. '!s 130' to set my call count
  - Count my claims and then do same for claims
  - Zendesk/freight emails I click after every solve so it is accurate (mostly)
  - Then the actions button is neatly calculated on a breakdown of actions

_kpi.txt_

If you edit it, make a copy in case something goes wrong!

You're best off using the default file as a template for editing.

Rules:

SPECIAL CHARACTORS – do not use / use with caution! \<

\< \> # / \ [; [_TAB_]

Each line has specific meaning

_[BUTTON NAME]\<current count\>/options_

Example:

_[sales]\<0\>/it_

These lines form the main buttons that all other buttons will be sub menus of:



When adding you don't need to add the \<current count\> it is automatically added.

Options include:

i : increment (records counts of the clicks)

t : when sub menu is right clicked it shows the stores menu

d : display stats, when clicked the stats window is shown, required at least once.

m : creates the menu, this is required to have if you want to be able to properly use the app.

Anything lines under this line will be a sub menu.

_SUB MENU NAME;STUFF TO COPY PASTE\<DESCRIPTION\>\<CURRENT COUNT\>_

Example:

_[faq]\<0\>/I_  main menu

_Is Order Ready?;Customer Checking Order Status\<Checking order, updates etc\>\<0\>_  Sub menu

Sub menus inherit the options of the main button unless you overwrite it by adding a comment above the button you want to overwrite.

When clicked, depending on the options multiple thinks may occur. If there option 'i' is set then the count will increment, if the a preset message is set (_;STUFF TO COPY_) then it will be copied to clipboard. Links may open, templates may open, etc.

If the line is tabbed, then it will become a new submenu. For instance, if kpi.txt looks like:


Then, the menu looks like:


_~REGEX@HTML LINK_

If whatever is in the clipboard (excluding trailing spaces) match the regex when clicked, then the curly braces {} in html link will be replaced whatever is in clipboard and a new tab will be loaded in CHROME.

If the regex = ".+" then the regex won't search, and whatever is in clipboard will be pasted in the {}, if there are no {} then the link will simply load.

Regex can include @, so emails can be used. Ie '.+@.+' could match an email

Note there is some cleaning, only http/https links are loaded into chrome, several characters are removed – let me know if this prevents use, so far it has been fine for all intended purposes.

You can load multiple tabs with one click, just make sure the regex matches multiple URLS -

_^FILE NAME.rtf_

Loads the rtf template file here. It loads it as a submenu of the previous button -use default kpi.txt as reference

_;COMMENT_

Example;

**not here**

You may add a comment above a button as per above.

Keep comments at start of the line – it will always affect the button directly below it.

Some comments have special meaning!

;# : this one will disable the submenu auto opening – you must long press to open menu.

;ADDSTORE : this will make the button and any submenus use the store style right click menu

;ADDCLAIM : this will make the button and any submenus use the claim style right click menu

_\<BUTTON NAME;STUFF TO COPY\<DESCRIPTION\>\<CURRENT COUNT\>_

Standard button, however it is deleted after 'reset' is used

All buttons added by right click are default to be a temporary button, use \>BUTTON NAME, when adding button to make it permanent.

- Dragging the ends will move the tool bar
- Hover over a menu to open it _(no clicking required)_
- Left Click to add a count to that button, copy pre-set message to clipboard
- Long Left Click If you've used a comment ";#" or added ";#" into text bar, then you need to long press to open sub menu, used for menus you do not want to auto open. Long pressing does not count as a click!
- ~~CTRL + Left Click~~~~ to append to clipboard ~~~~_(doesn't add to count)_~~_useless feature, removed_
- SHIFT + Left Click OR CTRL + LEFT CLICK to append to clipboard _(adds to count)_
  - Works same as before, however removed the newline between the copy pastes.
  - IF it is standard copy/paste, and either clipboard or the preset message contain "()" it will them insert the message into the "()".
    - Ie if copy button contains preset message of "ZENDESK #()" then if you shift click with "12345" in clipboard, your clipboard should contain "ZENDESK #12345"
  - ELSE IF it is a templated button (ie loaded from the RTF document) then if your current clipboard contains "$$$" then it will insert the new message into the clipboard. This works same as above point, however allows you to insert batch photos into generic claim email! Much faster.
  - ~~CTRL/SHIFT + Left Click~~~~ will remove duplicate --name tags and append it to the end, ~~no longer needed as names are now added to comments
- ~~ALT + Left Click~~~~ to open link if /w is selected. ~~Annoying feature, if it is a link then it will open, you can auto copy to clipboard too.
Right Click
  - ~~**Main Menu**~~~~ to open floating menu that allows quick add to submenu, removing the Main Menu button that was right clicked. ~~Removed useless. Use text bar and type "!d" to delete submenu, use settings\>edit to make additions to menu (ctrl+s to reload)
  - **Sub Menu**
    - Normal button:
      - Opens text bar and allows you to use commands, or add temp button (that is removed with 'reset' is used).
    - Claim button:
      - Opens text bar + some buttons, commands can be used, clicking end state will save the claim info, or use enter to save it without states.
      - Clipboard will auto copy into bar, make sure if have a claim copied.
    - Store button:
      - Same as claim button, however if you modify it and click away, it will save the change! Although you can select multiple stores in multiple states, only the current state will store, nobody askes for stores in every state…
  - **Hold and Drag Sub Menu Items** to rearrange them quickly—now works for sub menus! Still will not work for templated menus as they are within the rtf document.
- Middle Click to view and edit the data to be copied to clip board when Left Clicked _(sub menu only) – should be able to use commands too._
- Menu\>Save to save the menu information, and create backup.rtf– _(won't save settings until exit is used) - may now save settings?_
- Menu\>Edit to quickly edit kpi.txt, CTRL+S to save any changes and update the tool bar _(temp info kept)_
- Menu\>Reset to clear recorded stats – added warning due to mevans.
- Menu\>Pad to open/close the pad window _(pad information kept)_
- Menu\>Close to close tracker window
- Menu\>Exit to exit application, ~~will save menu and settings~~ , added warning to see if you want to save.

COMMANDS:

Used in text bars.

NORMAL ONLY:

- any text that is not command will create a temp button ie copy order number to save it.
- If line starts with '\>' then it will be a perm button ie "\>New claim" will save when reset used.
- Note: Middle clicking a button will not add a button, it is designed to EDIT the button

ALL BUTTONS:

- '!' at start of line means command, if command doesn't match then line is ignored.
- "!a" == add sub menu, used for adding buttons to the claim and store menus
- "!a\>" == add perm button, see above note.
- "!i" == info, add a description to the button (shows in purple text in stats window)
- "!s" == set count. Ie "!s 100" will make the buttons count 100 (useful for adding calls at end of day)
- "!d" == delete, deletes button and any children.
- "!#" == don't auto show menu, will require long left click to open sub menu
- "!$" == show menu, opposite for above.

**Pad**

**General Usage:**

![nt2](https://user-images.githubusercontent.com/14177650/227681811-11ad3643-bbaf-404e-82dd-99ad9afc6885.png)

- CTRL + S to save to file _(file name generated by the first date in the pad)_
- ALT + Z / X / C to change colour of selected text / text to be written until new line
- ALT + D to mark a line as completed
- Clear\> remove completed lines
- Menu\>New to get an empty pad
- Menu\>Save to save (same as CTRL + S)
- ~~Menu\>Settings~~~~ to edit settings for both Pad/Tracker windows~~
- ~~Menu\>Close~~~~ to close Pad window without saving ~~Use tracker menu
- ~~Menu\>Exit~~~~ to exit application, will save menu and settings~~
- Load\> hover over to view files that can be loaded (file name must be in correct format)
  - **Middle Click on Sub Menu** to quickly delete a file – need to do every now and then as it tends to create files most days
- Small Square in bottom RHS \> resize window

# Settings Window

## General Usage:

- Mainly self-explained – settings will update on the fly, even font/colours!
- **Always Front \>** if windows are always on top of other windows or not
- **AutoSave \>** will automatically save both windows every 15mins (time can be changed in settings.txt)
- **New Lines \>** when viewing stats in Tracker, temporary info can be all on new lines, or squished in as few lines as possible


