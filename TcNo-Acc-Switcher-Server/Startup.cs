// TcNo Account Switcher - A Super fast account switcher
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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TcNo_Acc_Switcher_Globals;
using TcNo_Acc_Switcher_Server.Data;
using TcNo_Acc_Switcher_Server.Pages.General;

namespace TcNo_Acc_Switcher_Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Console.Title = @"TcNo Account Switcher - Server";
            IconChanger.SetConsoleIcon();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Crash handler
            AppDomain.CurrentDomain.UnhandledException += Globals.CurrentDomain_UnhandledException;
            _ = services.AddControllers();

            _ = services.AddRazorPages();
            _ = services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Persistent settings:
            _ = services.AddSingleton<AppSettings>();
            _ = services.AddSingleton<AppData>();
            _ = services.AddSingleton<Data.Settings.BattleNet>();
            _ = services.AddSingleton<Data.Settings.Discord>();
            _ = services.AddSingleton<Data.Settings.Epic>();
            _ = services.AddSingleton<Data.Settings.Origin>();
            _ = services.AddSingleton<Data.Settings.Riot>();
            _ = services.AddSingleton<Data.Settings.Steam>();
            _ = services.AddSingleton<Data.Settings.Ubisoft>();
            _ = services.AddSingleton<Lang>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Lang.Instance.LoadLocalised();

            _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");

            // Moves any old files from previous installs.
            foreach (var p in Globals.PlatformList) // Copy across all platform files
            {
                MoveIfFileExists(p + "Settings.json");
            }
            MoveIfFileExists("SteamForgotten.json");
            MoveIfFileExists("Tray_Users.json");
            MoveIfFileExists("WindowSettings.json");

            // Copy LoginCache
            if (Directory.Exists(Path.Join(Globals.AppDataFolder, "LoginCache\\")))
            {
                if (Directory.Exists(Path.Join(Globals.UserDataFolder, "LoginCache"))) GeneralFuncs.RecursiveDelete(new DirectoryInfo(Path.Join(Globals.UserDataFolder, "LoginCache")), true);
                Globals.CopyFilesRecursive(Path.Join(Globals.AppDataFolder, "LoginCache"), Path.Join(Globals.UserDataFolder, "LoginCache"));
            }

            try
            {
                _ = app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Join(Globals.UserDataFolder, @"wwwroot")),
                    RequestPath = new PathString("")
                });
            }
            catch (DirectoryNotFoundException)
            {
                Globals.CopyFilesRecursive(Globals.OriginalWwwroot, "wwwroot");
            }

            _ = app.UseStaticFiles(); // Second call due to: https://github.com/dotnet/aspnetcore/issues/19578

            _ = app.UseRouting();

            _ = app.UseEndpoints(endpoints =>
              {
                  _ = endpoints.MapDefaultControllerRoute();
                  _ = endpoints.MapControllers();

                  _ = endpoints.MapBlazorHub();
                  _ = endpoints.MapFallbackToPage("/_Host");
              });
        }

        private static void MoveIfFileExists(string f)
        {
            if (File.Exists(Path.Join(Globals.AppDataFolder, f)))
                File.Copy(Path.Join(Globals.AppDataFolder, f), Path.Join(Globals.UserDataFolder, f), true);
            File.Delete(Path.Join(Globals.AppDataFolder, f));
        }
    }

    internal class IconChanger
    {
        // Based on https://stackoverflow.com/a/59897483/5165437
        public static void SetConsoleIcon()
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return;
                var path = Path.Join(Globals.AppDataFolder, "originalwwwroot\\prog_icons\\program.ico");
                if (!File.Exists(path)) path = Path.Join(Globals.AppDataFolder, "wwwroot\\prog_icons\\program.ico");
                if (!File.Exists(path)) return;
                var icon = new System.Drawing.Icon(path);
                SetWindowIcon(icon);
            }
            catch (Exception e)
            {
                //
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        private static void SetWindowIcon(System.Drawing.Icon icon)
        {
            // 0x0080 is SETICON
            var mwHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var result01 = SendMessage(mwHandle, (int)0x0080, 0, icon.Handle);
            var result02 = SendMessage(mwHandle, (int)0x0080, 1, icon.Handle);
        }
    }
}
