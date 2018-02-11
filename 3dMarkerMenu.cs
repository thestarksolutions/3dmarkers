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

        UIMenuItem refresh_button = new UIMenuItem("Refresh");
        menu.AddItem(refresh_button);
        menu.OnItemSelect += (sender, item, index) =>
        {
            if (item == refresh_button)
            {
                CreateNewMenu();
            }
        };

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

        blip_names.Sort();

        foreach (string blip_name in blip_names)
        {
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

    void CreateNewMenu()
    {
        _menuPool = new MenuPool();
        mainMenu = new UIMenu("3d Marker", "~b~Marker Menu");
        _menuPool.Add(mainMenu);
        CreateMenu(mainMenu);
        _menuPool.RefreshIndex();
    }

    public MenuExample()
    {
        active_blips = World.GetActiveBlips();
        config = ScriptSettings.Load("scripts\\3dMarker.ini");
        CreateNewMenu();

        Tick += (o, e) => _menuPool.ProcessMenus();
        KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.F5 && !_menuPool.IsAnyMenuOpen()) // Our menu on/off switch
                mainMenu.Visible = !mainMenu.Visible;
        };

    }
}
