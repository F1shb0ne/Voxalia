//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK.Graphics;
using System.Linq;
using FreneticGameCore;
using FreneticGameGraphics;
using FreneticGameGraphics.GraphicsHelpers;

namespace Voxalia.ClientGame.UISystem
{
    /// <summary>
    /// Handles the interactive user text console.
    /// TODO: Make non-static.
    /// TODO: Support channels.
    /// TODO: Maybe rewrite entirely!?
    /// </summary>
    public class UIConsole
    {
        /// <summary>
        /// Holds the Graphics text object, for rendering.
        /// </summary>
        public static string ConsoleText;
        public static Location ConsoleTextLoc;

        /// <summary>
        /// Holds the text currently being typed.
        /// </summary>
        public static string Typing;
        public static Location TypingLoc;

        /// <summary>
        /// Holds the "scrolled-up" text.
        /// </summary>
        public static string ScrollText;
        public static Location ScrollTextLoc;

        /// <summary>
        /// The background texture to be used.
        /// </summary>
        public static Texture ConsoleTexture;

        /// <summary>
        /// How many lines the console should have.
        /// </summary>
        public static int Lines = 100;

        /// <summary>
        /// How long across (in pixels) console text may be.
        /// </summary>
        public static int MaxWidth = 1142;

        /// <summary>
        /// Whether the console is open.
        /// </summary>
        public static bool Open = false;

        /// <summary>
        /// The text currently being typed by the user.
        /// </summary>
        public static string TypingText = "";

        /// <summary>
        /// Where in the typing text the cursor is at.
        /// </summary>
        public static int TypingCursor = 0;

        /// <summary>
        /// What line has been scrolled to:
        /// 0 = farthest down, -LINES = highest up.
        /// The selected line will be rendered at the bottom of the screen.
        /// </summary>
        public static int ScrolledLine = 0;

        /// <summary>
        /// How many recent commands to store.
        /// </summary>
        public static int MaxRecentCommands = 50;

        /// <summary>
        /// A list of all recently execute command-lines.
        /// </summary>
        public static List<string> RecentCommands = new List<string>() { "" };

        /// <summary>
        /// What spot in the RecentCommands the user is currently at.
        /// </summary>
        public static int RecentSpot = 0;

        static bool ready = false;

        static string pre_waiting = "";

        static int extralines = 0;
        static double LineBack = 0;

        /// <summary>
        /// Prepares the console.
        /// </summary>
        public static void InitConsole()
        {
            ready = true;
            ConsoleText = Utilities.CopyText("\n", Lines);
            ConsoleTextLoc = new Location(5, 0, 0);
            Typing = "";
            TypingLoc = new Location(5, 0, 0);
            ScrollText = "^1" + Utilities.CopyText("/\\ ", 150);
            ScrollTextLoc = new Location(5, 0, 0);
            MaxWidth = Client.Central.Window.Width - 10;
            ConsoleTexture = Client.Central.Textures.GetTexture("ui/hud/console");
            WriteLine("Console loaded!");
            Write(pre_waiting);
        }

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public static void WriteLine(string text)
        {
            Write(FreneticScript.TextStyle.Default + text + "\n");
            SysConsole.Output(OutputType.CLIENTINFO, text);
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        public static void Write(string text)
        {
            if (!ready)
            {
                pre_waiting += text;
                return;
            }
            Client.Central.FontSets.Standard.MeasureFancyText(text.Replace("\n", "").Replace("\r", ""), pushStr: true);
            if (!ConsoleText.EndsWith("\n"))
            {
                for (int x = ConsoleText.Length - 1; x > 0; x--)
                {
                    if (ConsoleText[x] == '\n')
                    {
                        string snippet = ConsoleText.Substring(x + 1, ConsoleText.Length - (x + 1));
                        text = snippet + text;
                        ConsoleText = ConsoleText.Substring(0, x + 1);
                        break;
                    }
                }
            }
            text = text.Replace('\r', ' ');
            if (text.EndsWith("\n") && text.Length > 1)
            {
                //    SysConsole.Output(OutputType.CLIENTINFO, text.Substring(0, text.Length - 1));
            }
            else
            {
                //    SysConsole.Output(OutputType.CLIENTINFO, text);
            }
            int linestart = 0;
            int i = 0;
            for (i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    linestart = i + 1;
                    i++;
                    continue;
                }
                if (Client.Central.FontSets.Standard.MeasureFancyText(text.Substring(linestart, i - linestart)) > MaxWidth)
                {
                    i -= 1;
                    for (int x = i; x > 0 && x > linestart + 5; x--)
                    {
                        if (text[x] == ' ')
                        {
                            i = x + 1;
                            break;
                        }
                    }
                    text = text.Substring(0, i) + "\n" + (i < text.Length ? text.Substring(i, text.Length - i) : "");
                    linestart = i + 1;
                    i++;
                }
            }
            int lines = Utilities.CountCharacter(text, '\n');
            if (lines > 0)
            {
                int linecount = 0;
                for (i = 0; i < ConsoleText.Length; i++)
                {
                    if (ConsoleText[i] == '\n')
                    {
                        linecount++;
                        if (linecount >= lines)
                        {
                            break;
                        }
                    }
                }
                ConsoleText = ConsoleText.Substring(i + 1, ConsoleText.Length - (i + 1));
            }
            extralines += lines;
            if (extralines > 3)
            {
                extralines = 3;
            }
            LineBack = 3f;
            ConsoleText += text;
        }

        static bool keymark_add = false;
        static double keymark_delta = 0f;

        /// <summary>
        /// Whether the mouse was captured before the console was opened.
        /// </summary>
        public static bool MouseWasCaptured = false;

        /// <summary>
        /// Updates the console, called every tick.
        /// </summary>
        public static void Tick()
        {
            // Update open/close state
            if (KeyHandler._TogglerPressed)
            {
                KeyHandler.GetKBState();
                Open = !Open;
                if (Open)
                {
                    MouseWasCaptured = MouseHandler.MouseCaptured;
                    MouseHandler.ReleaseMouse();
                    RecentSpot = RecentCommands.Count;
                }
                else
                {
                    if (MouseWasCaptured)
                    {
                        MouseHandler.CaptureMouse();
                    }
                    Typing = "";
                    TypingText = "";
                    TypingCursor = 0;
                }
            }
            if (Open)
            {
                KeyHandlerState KeyState = KeyHandler.GetKBState();
                extralines = 0;
                LineBack = 0;
                // flicker the cursor
                keymark_delta += Client.Central.Delta;
                if (keymark_delta > 0.5f)
                {
                    keymark_add = !keymark_add;
                    keymark_delta = 0f;
                }
                // handle backspaces
                if (KeyState.InitBS > 0)
                {
                    string partone = TypingCursor > 0 ? TypingText.Substring(0, TypingCursor) : "";
                    string parttwo = TypingCursor < TypingText.Length ? TypingText.Substring(TypingCursor) : "";
                    if (partone.Length > KeyState.InitBS)
                    {
                        partone = partone.Substring(0, partone.Length - KeyState.InitBS);
                        TypingCursor -= KeyState.InitBS;
                    }
                    else
                    {
                        TypingCursor -= partone.Length;
                        partone = "";
                    }
                    TypingText = partone + parttwo;
                }
                // handle input text
                KeyState.KeyboardString = KeyState.KeyboardString.Replace("\t", "    ");
                if (KeyState.KeyboardString.Length > 0)
                {
                    if (TypingText.Length == TypingCursor)
                    {
                        TypingText += Utilities.CleanStringInput(KeyState.KeyboardString);
                    }
                    else
                    {
                        if (KeyState.KeyboardString.Contains('\n'))
                        {
                            string[] lines = KeyState.KeyboardString.SplitFast('\n', 1);
                            TypingText = TypingText.Insert(TypingCursor, Utilities.CleanStringInput(lines[0])) + "\n" + Utilities.CleanStringInput(lines[1]);
                        }
                        else
                        {
                            TypingText = TypingText.Insert(TypingCursor, Utilities.CleanStringInput(KeyState.KeyboardString));
                        }
                    }
                    TypingCursor += KeyState.KeyboardString.Length;
                    while (TypingText.Contains('\n'))
                    {
                        int index = TypingText.IndexOf('\n');
                        string input = TypingText.Substring(0, index);
                        if (index + 1 < TypingText.Length)
                        {
                            TypingText = TypingText.Substring(index + 1);
                            TypingCursor = TypingText.Length;
                        }
                        else
                        {
                            TypingText = "";
                            TypingCursor = 0;
                        }
                        WriteLine("] " + input);
                        RecentCommands.Add(input);
                        if (RecentCommands.Count > MaxRecentCommands)
                        {
                            RecentCommands.RemoveAt(0);
                        }
                        RecentSpot = RecentCommands.Count;
                        Client.Central.Commands.ExecuteCommands(input);
                    }
                }
                // handle copying
                if (KeyState.CopyPressed)
                {
                    if (TypingText.Length > 0)
                    {
                        System.Windows.Forms.Clipboard.SetText(TypingText);
                    }
                }
                // handle cursor left/right movement
                if (KeyState.LeftRights != 0)
                {
                    TypingCursor += KeyState.LeftRights;
                    if (TypingCursor < 0)
                    {
                        TypingCursor = 0;
                    }
                    if (TypingCursor > TypingText.Length)
                    {
                        TypingCursor = TypingText.Length;
                    }
                    keymark_add = true;
                    keymark_delta = 0f;
                }
                // handle scrolling up/down in the console
                if (KeyState.Pages != 0)
                {
                    ScrolledLine -= (int)(KeyState.Pages * ((float)Client.Central.Window.Height / 2 / Client.Central.FontSets.Standard.font_default.Height - 3));
                }
                ScrolledLine -= MouseHandler.MouseScroll;
                if (ScrolledLine > 0)
                {
                    ScrolledLine = 0;
                }
                if (ScrolledLine < -Lines + 5)
                {
                    ScrolledLine = -Lines + 5;
                }
                // handle scrolling through commands
                if (KeyState.Scrolls != 0)
                {
                    RecentSpot -= KeyState.Scrolls;
                    if (RecentSpot < 0)
                    {
                        RecentSpot = 0;
                        TypingText = RecentCommands[0];
                    }
                    else if (RecentSpot >= RecentCommands.Count)
                    {
                        RecentSpot = RecentCommands.Count;
                        TypingText = "";
                    }
                    else
                    {
                        TypingText = RecentCommands[RecentSpot];
                    }
                    TypingCursor = TypingText.Length;
                }
                // update the rendered text
                Typing = ">" + TypingText;
            }
            else // !Open
            {
                if (extralines > 0)
                {
                    LineBack -= Client.Central.Delta;
                    if (LineBack <= 0)
                    {
                        extralines--;
                        LineBack = 3f;
                    }
                }
            }
        }

        public static bool NoLinks = false;

        /// <summary>
        /// Renders the console, called every tick.
        /// </summary>
        public static void Draw()
        {
            // Render the console texture
            TypingLoc.Y = ((Client.Central.Window.Height / 2) - Client.Central.FontSets.Standard.font_default.Height) - 5;
            ConsoleTextLoc.Y = (-(Lines + 2) * Client.Central.FontSets.Standard.font_default.Height) - 5 - ScrolledLine * (int)Client.Central.FontSets.Standard.font_default.Height;
            ScrollTextLoc.Y = ((Client.Central.Window.Height / 2) - Client.Central.FontSets.Standard.font_default.Height * 2) - 5;
            if (Open)
            {
                ConsoleTextLoc.Y += Client.Central.Window.Height / 2;
                // Standard console box
                //  GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                ConsoleTexture.Bind();
                Client.Central.Rendering.SetColor(Color4.White);
                Client.Central.Rendering.RenderRectangle(0, 0, Client.Central.Window.Width, Client.Central.Window.Height / 2);
                // Scrollbar
                Client.Central.Textures.White.Bind();
                Client.Central.Rendering.RenderRectangle(0, 0, 2, Client.Central.Window.Height / 2);
                Client.Central.Rendering.SetColor(Color4.Red);
                float Y = Client.Central.Window.Height / 2;
                float percentone = -(float)ScrolledLine / (float)Lines;
                float percenttwo = -((float)ScrolledLine - (float)Client.Central.Window.Height / Client.Central.FontSets.Standard.font_default.Height) / (float)Lines;
                Client.Central.Rendering.RenderRectangle(0, (int)(Y - Y * percenttwo), 2, (int)(Y - Y * percentone));
                // Bottom line
                Client.Central.Textures.White.Bind();
                Client.Central.Rendering.SetColor(Color4.Cyan);
                Client.Central.Rendering.RenderRectangle(0, (Client.Central.Window.Height / 2) - 1, Client.Central.Window.Width, Client.Central.Window.Height / 2);
                // Typing text
                string typed = Typing;
                int c = 0;
                if (!Client.Central.CVars.u_colortyping.ValueB)
                {
                    for (int i = 0; i < typed.Length && i < TypingCursor; i++)
                    {
                        if (typed[i] == '^')
                        {
                            c++;
                        }
                    }
                    typed = typed.Replace("^", "^^n");
                }
                Client.Central.Rendering.SetColor(Color4.White);
                Client.Central.FontSets.Standard.DrawColoredText(typed, TypingLoc);
                // Cursor
                if (keymark_add)
                {
                    double XAdd = Client.Central.FontSets.Standard.MeasureFancyText(typed.Substring(0, TypingCursor + 1 + c * 2), pushStr: true) - 1;
                    if (typed.Length > TypingCursor + 1 && typed[TypingCursor + c] == '^'
                        && FontSet.IsColorSymbol(typed[TypingCursor + 1 + c]))
                    {
                        XAdd -= Client.Central.FontSets.Standard.font_default.MeasureString(typed[TypingCursor + c].ToString());
                    }
                    Client.Central.FontSets.Standard.DrawColoredText("|", new Location(TypingLoc.X + XAdd, TypingLoc.Y, 0));
                }
                // Render the console 
                float maxy = Client.Central.Window.Height / 2 - Client.Central.FontSets.Standard.font_default.Height * 3;
                Client.Central.FontSets.Standard.DrawColoredText(ConsoleText, ConsoleTextLoc, (int)maxy);
                if (ScrolledLine != 0)
                {
                    Client.Central.FontSets.Standard.DrawColoredText(ScrollText, ScrollTextLoc);
                }
                double sy = ConsoleTextLoc.Y;
                float sh = Client.Central.Fonts.Standard.Height;
                foreach (string str in ConsoleText.Split('\n'))
                {
                    if (sy + sh > 0)
                    {
                        if (str.Contains("^["))
                        {
                            List<KeyValuePair<string, Rectangle2F>> rects = new List<KeyValuePair<string, Rectangle2F>>();
                            Client.Central.FontSets.Standard.MeasureFancyText(str, out rects);
                            foreach (KeyValuePair<string, Rectangle2F> rectent in rects)
                            {
                                rectent.Value.Y += (float)sy;
                                if (MouseHandler.MouseX() >= rectent.Value.X && MouseHandler.MouseX() <= rectent.Value.X + rectent.Value.Width
                                    && MouseHandler.MouseY() >= rectent.Value.Y && MouseHandler.MouseY() <= rectent.Value.Y + rectent.Value.Height)
                                {
                                    bool isUrl = (rectent.Key.StartsWith("url=http://") || rectent.Key.StartsWith("url=https://"));
                                    bool isHover = (rectent.Key.StartsWith("hover="));
                                    bool clicked = Client.Central.Window.Focused && OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed;
                                    if (isUrl && clicked && !NoLinks)
                                    {
                                        System.Diagnostics.Process.Start(rectent.Key.Substring("url=".Length));
                                    }
                                    NoLinks = clicked;
                                    string renderme;
                                    if (isUrl)
                                    {
                                        string url = rectent.Key.StartsWith("url=http://") ? rectent.Key.After("url=http://") : rectent.Key.After("url=https://");
                                        renderme = "Click:URL<" + FontSet.EscapeFancyText(url) + ">";
                                    }
                                    else if (isHover)
                                    {
                                        renderme = "<" + FontSet.EscapeFancyText(rectent.Key.Substring("hover=".Length).Replace("\\n", "\n")) + ">";
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    Location lens = Client.Central.FontSets.Standard.MeasureFancyLinesOfText(renderme);
                                    float len = (float)lens.X;
                                    float hei = (float)lens.Y;
                                    float x = (rectent.Value.X + len > Client.Central.Window.Width) ? 0 : rectent.Value.X;
                                    float y = rectent.Value.Y + sh;
                                    Client.Central.Rendering.SetColor(Color4.Blue);
                                    Client.Central.Rendering.RenderRectangle(x, y, x + len + 5, y + 5 + hei);
                                    Client.Central.Rendering.SetColor(Color4.LightGray);
                                    Client.Central.Rendering.RenderRectangle(x + 1, y + 1, x + len + 4, y + 4 + hei);
                                    Client.Central.Rendering.SetColor(Color4.White);
                                    Client.Central.FontSets.Standard.DrawColoredText("^)" + renderme, new Location(x, y, 0));
                                }
                            }
                        }
                    }
                    sy += sh;
                    if (sy > maxy)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (Client.Central.CVars.u_showhud.ValueB)
                {
                    ConsoleTextLoc.Y += (int)(Client.Central.FontSets.Standard.font_default.Height * (2 + extralines)) + 4;
                    Client.Central.FontSets.Standard.DrawColoredText(ConsoleText, ConsoleTextLoc, (int)(Client.Central.Window.Height / 2 - Client.Central.FontSets.Standard.font_default.Height * 3), 1, true);
                    ConsoleTextLoc.Y -= (int)(Client.Central.FontSets.Standard.font_default.Height * (2 + extralines)) + 4;
                }
            }
        }
    }
}
