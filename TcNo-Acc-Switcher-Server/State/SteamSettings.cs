﻿// TcNo Account Switcher - A Super fast account switcher
// Copyright (C) 2019-2022 TechNobo (Wesley Pyburn)
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using TcNo_Acc_Switcher_Globals;

namespace TcNo_Acc_Switcher_Server.State;

public class SteamSettings
{
    public bool ForgetAccountEnabled { get; set; }
    public string FolderPath { get; set; } = "C:\\Program Files (x86)\\Steam\\";
    public bool Admin { get; set; }
    public bool ShowSteamId { get; set; }
    public bool ShowVac { get; set; } = true;
    public bool ShowLimited { get; set; } = true;
    public bool ShowLastLogin { get; set; } = true;
    public bool ShowAccUsername { get; set; } = true;
    public bool TrayAccountName { get; set; }
    public int ImageExpiryTime { get; set; } = 7;
    public int TrayAccNumber { get; set; } = 3;
    public int OverrideState { get; set; } = -1;
    public string SteamWebApiKey { get; set; } = "";
    public Dictionary<int, string> Shortcuts { get; set; } = new();
    public string ClosingMethod { get; set; } = "TaskKill";
    public string StartingMethod { get; set; } = "Default";
    public bool AutoStart { get; set; } = true;
    public bool ShowShortNotes { get; set; } = true;
    public bool StartSilent { get; set; }
    public Dictionary<string, string> CustomAccountNames { get; set; } = new();

    [JsonIgnore] public string LoginUsersVdf;
    public const string SteamImagePath = "wwwroot/img/profiles/steam/";
    public const string SteamImagePathHtml = "img/profiles/steam/";
    public readonly List<string> Processes = new (){ "steam.exe", "SERVICE:steamservice.exe", "steamwebhelper.exe", "GameOverlayUI.exe" };
    public readonly string VacCacheFile = Path.Join(Globals.UserDataFolder, "LoginCache\\Steam\\VACCache\\SteamVACCache.json");


    public static readonly string Filename = "SteamSettings.json";
    public SteamSettings()
    {
        Globals.LoadSettings(Filename, this, false);
        LoginUsersVdf = Path.Join(FolderPath, "config\\loginusers.vdf");
    }
    public void Save() => Globals.SaveJsonFile(Filename, this, false);

    public void Reset()
    {
        var type = GetType();
        var properties = type.GetProperties();
        foreach (var t in properties)
            t.SetValue(this, null);
        Save();
    }


    /// <summary>
    /// Get Steam.exe path from SteamSettings.json
    /// </summary>
    /// <returns>Steam.exe's path string</returns>
    public string Exe => Path.Join(FolderPath, "Steam.exe");

    [JSInvokable]
    public void SaveShortcutOrderSteam(Dictionary<int, string> o)
    {
        Shortcuts = o;
        Save();
    }

    public void SetClosingMethod(string method)
    {
        ClosingMethod = method;
        Save();
    }
    public void SetStartingMethod(string method)
    {
        StartingMethod = method;
        Save();
    }

    /// <summary>
    /// Updates the ForgetAccountEnabled bool in Steam settings file
    /// </summary>
    /// <param name="enabled">Whether will NOT prompt user if they're sure or not</param>
    public void SetForgetAcc(bool enabled)
    {
        Globals.DebugWriteLine(@"[Func:Data\Settings\Steam.SetForgetAcc]");
        if (ForgetAccountEnabled == enabled) return; // Ignore if already set
        ForgetAccountEnabled = enabled;
        Save();
    }
}