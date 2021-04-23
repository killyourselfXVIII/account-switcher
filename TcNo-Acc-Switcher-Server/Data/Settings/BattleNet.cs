﻿// TcNo Account Switcher - A Super fast account switcher
// Copyright (C) 2019-2021 TechNobo (Wesley Pyburn)
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

// Special thanks to iR3turnZ for contributing to this platform's account switcher
// iR3turnZ: https://github.com/HoeblingerDaniel

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Svg.ExCSS.Model.Extensions;
using TcNo_Acc_Switcher_Globals;
using TcNo_Acc_Switcher_Server.Pages.BattleNet;
using TcNo_Acc_Switcher_Server.Pages.General;
using TcNo_Acc_Switcher_Server.Pages.General.Classes;

namespace TcNo_Acc_Switcher_Server.Data.Settings
{
    public class BattleNet
    {
        private static BattleNet _instance = new BattleNet();

        public BattleNet(){}
        private static readonly object LockObj = new();
        public static BattleNet Instance
        {
            get
            {
                lock (LockObj)
                {
                    return _instance ??= new BattleNet();
                }
            }
            set => _instance = value;
        }

        #region VARIABLES

        private string _folderPath = "C:\\Program Files (x86)\\Battle.net";
        [JsonProperty("FolderPath", Order = 1)] public string FolderPath { get => _instance._folderPath; set => _instance._folderPath = value; }
        
        private Point _windowSize = new() { X = 800, Y = 450 };
        [JsonProperty("WindowSize", Order = 2)] public Point WindowSize { get => _instance._windowSize; set => _instance._windowSize = value; }
        
        private bool _admin;
        [JsonProperty("BattleNet_Admin", Order = 3)] public bool Admin { get => _instance._admin; set => _instance._admin = value; }
        
        private int _trayAccNumber = 3;
        [JsonProperty("BattleNet_TrayAccNumber", Order = 4)] public int TrayAccNumber { get => _instance._trayAccNumber; set => _instance._trayAccNumber = value; }
        
        private bool _forgetAccountEnabled;
        [JsonProperty("ForgetAccountEnabled", Order = 5)] public bool ForgetAccountEnabled { get => _instance._forgetAccountEnabled; set => _instance._forgetAccountEnabled = value; }
        
        private bool _overwatchMode = true;
        [JsonProperty("OverwatchMode", Order = 6)] public bool OverwatchMode { get => _instance._overwatchMode; set => _instance._overwatchMode = value; }
        
        private bool _desktopShortcut;
        [JsonIgnore] public bool DesktopShortcut { get => _instance._desktopShortcut; set => _instance._desktopShortcut = value; }
        
        private List<BattleNetSwitcherBase.BattleNetUser> _accounts = new();
        [JsonProperty("Accounts", Order = 6)] public List<BattleNetSwitcherBase.BattleNetUser> Accounts { get => _instance._accounts; set => _instance._accounts = value; }
        
        private List<BattleNetSwitcherBase.BattleNetUser> _ignoredAccounts = new();
        [JsonIgnore] public List<BattleNetSwitcherBase.BattleNetUser> IgnoredAccounts { get => _instance._ignoredAccounts; set => _instance._ignoredAccounts = value; }
        
        // Constants
        [JsonIgnore] public string SettingsFile = "BattleNetSettings.json";
        [JsonIgnore] public string BattleNetImagePath = "wwwroot/img/profiles/battlenet/";
        [JsonIgnore] public string BattleNetImagePathHtml = "img/profiles/battlenet/";
        [JsonIgnore] public string StoredAccPath = "LoginCache\\BattleNet\\StoredAccounts.json";
        [JsonIgnore] public string IgnoredAccPath = $"LoginCache\\BattleNet\\IgnoredAccounts.json";
        [JsonIgnore] public string ContextMenuJson = @"[
              {""Swap to account"": ""SwapTo(-1, event)""},
              {""Set BattleTag"": ""ShowModal('changeUsername')""},
              {""Delete BattleTag"": ""ForgetBattleTag()""},
              {""Refetch Rank"": ""RefetchRank()""},
              {""Forget"": ""forget(event)""}
            ]";


        #endregion

        #region FORGETTING_ACCOUNTS
    
        /// <summary>
        /// Updates the ForgetAccountEnabled bool in settings file
        /// </summary>
        /// <param name="enabled">Whether will NOT prompt user if they're sure or not</param>
        public void SetForgetAcc(bool enabled)
        {
            Globals.DebugWriteLine($@"[Func:Data\Settings\BattleNet.SetForgetAcc]");
            if (_forgetAccountEnabled == enabled) return; // Ignore if already set
            _forgetAccountEnabled = enabled;
            SaveSettings();
        }
        
        /// <summary>
        /// Deletes the account from the settings file and from the roaming config
        /// </summary>
        /// <param name="accountEmail">Account email to forget</param>
        public void ForgetAccount(string accountEmail)
        {
            var acc = Accounts.Find(x => x.Email == accountEmail);
            Accounts.Remove(acc);
            IgnoredAccounts.Add(acc);
            SaveAccounts();
        }

        #endregion
        

        /// <summary>
        /// Get Battle.net.exe path from OriginSettings.json 
        /// </summary>
        /// <returns>Battle.net.exe's path string</returns>
        public string Exe() => FolderPath + "\\Battle.net.exe";
        

        #region SETTINGS
        /// <summary>
        /// Default settings for BattleNetSettings.json
        /// </summary>
        public void ResetSettings()
        {
            Globals.DebugWriteLine($@"[Func:Data\Settings\BattleNet.ResetSettings]");
            _instance.FolderPath = "C:\\Program Files (x86)\\Battle.net";
            _instance.WindowSize = new Point() { X = 800, Y = 450 };
            _instance.Admin = false;
            _instance.TrayAccNumber = 3;
            _instance._overwatchMode = true;
            // Should this also clear ignored accounts?

            CheckShortcuts();
            SaveSettings();
        }
        public void SetFromJObject(JObject j)
        {
            Globals.DebugWriteLine($@"[Func:Data\Settings\BattleNet.SetFromJObject]");
            var curSettings = j.ToObject<BattleNet>();
            if (curSettings == null) return;
            _instance.FolderPath = curSettings.FolderPath;
            _instance.WindowSize = curSettings.WindowSize;
            _instance.Admin = curSettings.Admin;
            _instance.TrayAccNumber = curSettings.TrayAccNumber;
            _instance._overwatchMode = curSettings._overwatchMode;
            CheckShortcuts();
        }
        public void LoadFromFile() => SetFromJObject(GeneralFuncs.LoadSettings(SettingsFile, GetJObject()));
        public JObject GetJObject() => JObject.FromObject(this);

        [JSInvokable]
        public void SaveSettings(bool mergeNewIntoOld = false) => GeneralFuncs.SaveSettings(SettingsFile, GetJObject(), mergeNewIntoOld);

        /// <summary>
        /// Load the Stored Accounts and Ignored Accounts
        /// </summary>
        public void LoadAccounts()
        {
            if (!Directory.Exists("LoginCache"))
            {
                Directory.CreateDirectory("LoginCache");
            }
            if (!Directory.Exists("LoginCache\\BattleNet"))
            {
                Directory.CreateDirectory("LoginCache\\BattleNet");
            }
            if (File.Exists(StoredAccPath) )
            {
                Accounts = JsonConvert.DeserializeObject<List<BattleNetSwitcherBase.BattleNetUser>>(File.ReadAllText(StoredAccPath)) ?? new();
            }

            if (File.Exists(IgnoredAccPath))
            {
                IgnoredAccounts = JsonConvert.DeserializeObject<List<BattleNetSwitcherBase.BattleNetUser>>(File.ReadAllText(IgnoredAccPath)) ?? new();
            }
        }

        /// <summary>
        /// Load the Stored Accounts and Ignored Accounts
        /// </summary>
        public void SaveAccounts()
        {
            File.WriteAllText(StoredAccPath, JsonConvert.SerializeObject(Accounts));
            File.WriteAllText(IgnoredAccPath, JsonConvert.SerializeObject(IgnoredAccounts));
        }
        
        
        #endregion

        #region SHORTCUTS
        public void CheckShortcuts()
        {
            Globals.DebugWriteLine($@"[Func:Data\Settings\BattleNet.CheckShortcuts]");
            _instance._desktopShortcut = File.Exists(Path.Join(Shortcut.Desktop, "BattleNet - TcNo Account Switcher.lnk"));
            AppSettings.Instance.CheckShortcuts();
        }

        public void DesktopShortcut_Toggle()
        {
            Globals.DebugWriteLine($@"[Func:Data\Settings\BattleNet.DesktopShortcut_Toggle]");
            var s = new Shortcut();
            s.Shortcut_Platform(Shortcut.Desktop, "BattleNet", "battlenet");
            s.ToggleShortcut(!DesktopShortcut, true);
        }
        #endregion
        
        
    }
}
