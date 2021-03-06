//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ClientGame.ClientMainSystem;
using System.IO;
using System.Threading;
using System.Globalization;
using FreneticGameCore.Files;
using System.Runtime.InteropServices;
using FreneticGameCore;

namespace Voxalia
{
    /// <summary>
    /// Central program entry point.
    /// </summary>
    class MainProgram
    {
        /// <summary>
        /// A handle for the console window.
        /// </summary>
        public static IntPtr ConsoleHandle;

#if !LINUX
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
#endif

        /// <summary>
        /// Central program entry point.
        /// Decides whether to lauch the server or the client.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        static void Main(string[] args)
        {
            ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            if (args.Length == 1 && args[0] == "{{Launcher}}")
            {
                args = new string[0];
#if !LINUX
                ShowWindow(ConsoleHandle, SW_HIDE);
#endif
            }
            VoxProgram.PreInitVox();
            SysConsole.AllowCursor = false;
            SysConsole.Init();
            StringBuilder arger = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                arger.Append(args[i]).Append(' ');
            }
            try
            {
                VoxProgram.Init();
                if (args.Length > 0 && args[0] == "server")
                {
                    string[] targs = new string[args.Length - 1];
                    Array.Copy(args, 1, targs, 0, targs.Length);
                    args = targs;
                    string game = "default";
                    if (args.Length > 0)
                    {
                        game = args[0];
                        string[] t = new string[args.Length - 1];
                        Array.Copy(args, 1, t, 0, t.Length);
                        args = t;
                    }
                    Server.Init(game, args);
                }
                else
                {
                    Client.Init(arger.ToString());
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    Console.WriteLine("Forced shutdown - terminating process.");
                    Environment.Exit(0);
                    return;
                }
                SysConsole.Output(ex);
                File.WriteAllText("GLOBALERR_" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt", ex.ToString() + "\n\n" + Environment.StackTrace);
            }
            SysConsole.ShutDown();
            Console.WriteLine("Final shutdown - terminating process.");
            Environment.Exit(0);
        }
    }
}
