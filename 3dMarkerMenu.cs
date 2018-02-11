using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using NativeUI;

public class MenuExample : Script
{
    private MenuPool _menuPool;
    private UIMenu mainMenu;
    ScriptSettings config;

    Blip[] active_blips;

    public void CreateMenu(UIMenu menu)
    {

        // refresh button to scan for new blips
        // destroys and recreates menu
        UIMenuItem description_button = new UIMenuItem("Description", "Select the markers that you want to be displayed in the game world. Only 15 total markers can be displayed at one time, so if a marker is not showing up in the world, try unchecking other enabled markers.");
        menu.AddItem(description_button);

        // refresh button to scan for new blips
        // destroys and recreates menu
        UIMenuItem refresh_button = new UIMenuItem("Refresh", "The Marker list doesn't refresh automatically when you move around, so refresh it to re-populate with the most recent available markers");
        menu.AddItem(refresh_button);
        menu.OnItemSelect += (sender, item, index) =>
        {
            if (item == refresh_button)
            {
                CreateNewMenu();
                mainMenu.Visible = true;
            }
        };

        // scan for all active blips
        // create a list of all unique blip names
        active_blips = World.GetActiveBlips();
        List<string> blip_names = new List<string>();
        foreach (Blip b in active_blips)
        {
            string tmp_name = b.Sprite.ToString().ToUpper();
            if (!blip_names.Contains(tmp_name))
            {
                blip_names.Add(tmp_name);
            }
        }

        // alphabetize list
        blip_names.Sort();

        // iterate through list, create menu item for each unique blip name
        // set value to ini value
        foreach (string blip_name in blip_names)
        {
            // read ini, default to disabled if not found
            string enable_disable = config.GetValue<string>("options", blip_name, "disabled");
            bool ed = false;
            if (enable_disable == "enabled") ed = true;
            var newitem = new UIMenuCheckboxItem(blip_name, ed, "show " + blip_name + " in world");
            menu.AddItem(newitem);
            menu.OnCheckboxChange += (sender, item, checked_) =>
            {
                if (item == newitem)
                {
                    string tmp_string = "enabled";
                    if (!checked_) tmp_string = "disabled";
                    config.SetValue("options", item.Text.ToUpper(), tmp_string);
                    UI.Notify(item.Text.ToUpper() + " is " + tmp_string);
                    config.Save();
                }
            };
        }
    }

    // create or re-create menu
    void CreateNewMenu()
    {
        _menuPool = new MenuPool();
        mainMenu = new UIMenu("3d Marker", "~b~Marker Selection");
        _menuPool.Add(mainMenu);
        CreateMenu(mainMenu);
        _menuPool.RefreshIndex();
    }

    void OnTick(object sender, EventArgs e)
    {
        if (!mainMenu.Visible)
        {
            if (Game.IsControlJustPressed(2, GTA.Control.PhoneRight) && Game.IsControlPressed(2, GTA.Control.VehicleHandbrake)) mainMenu.Visible = true;
        }
    }

    public MenuExample()
    {
        active_blips = World.GetActiveBlips();
        config = ScriptSettings.Load("scripts\\3dMarker.ini");
        CreateNewMenu();
        Tick += OnTick;
        Tick += (o, e) => _menuPool.ProcessMenus();
        KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.F5) // Our menu on/off switch
                mainMenu.Visible = !mainMenu.Visible;
        };

    }
}
