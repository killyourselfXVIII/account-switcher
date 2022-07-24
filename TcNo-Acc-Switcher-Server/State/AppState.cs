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

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using TcNo_Acc_Switcher_Server.State.Classes;
using TcNo_Acc_Switcher_Server.State.DataTypes;
using TcNo_Acc_Switcher_Server.State.Interfaces;

namespace TcNo_Acc_Switcher_Server.State;

public class AppState : IAppState, INotifyPropertyChanged
{
    public string PasswordCurrent { get; set; }

    public ShortcutsState Shortcuts { get; set; }

    public Toasts Toasts { get; set; }

    public Discord Discord { get; set; }

    public Updates Updates { get; set; }

    public Stylesheet Stylesheet { get; set; }

    public Navigation Navigation { get; set; }

    public Switcher Switcher { get; set; }

    public WindowState WindowState { get; set; }

    // Property change notifications
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public AppState(IWindowSettings www)
    {
        PasswordCurrent = "";
        Shortcuts = new ShortcutsState();
        Toasts = new Toasts();
        Discord = new Discord();
        Updates = new Updates();
        Stylesheet = new Stylesheet();
        Navigation = new Navigation();
        Switcher = new Switcher();
        WindowState = new WindowState();

        // Discord integration
        Discord.RefreshDiscordPresenceAsync(true);

        // Forward state changes.
        Stylesheet.PropertyChanged += (s, e) => PropertyChanged?.Invoke(s, e);
        WindowState.PropertyChanged += (s, e) => PropertyChanged?.Invoke(s, e);
    }


    public void OpenFolder(string folder)
    {
        Directory.CreateDirectory(folder); // Create if doesn't exist
        Process.Start("explorer.exe", folder);
        Toasts.ShowToastLang(ToastType.Info, "Toast_PlaceShortcutFiles");
    }
}