﻿using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UILabel : UIMenuItem
    {
        public string Text;
        
        public Func<float> XGet;

        public Func<float> YGet;

        public FontSet TextFont;

        public Func<int> MaxX = null;

        public string BColor = "^r^7";

        public UILabel(string btext, Func<float> xer, Func<float> yer, FontSet font, Func<int> maxx = null)
        {
            Text = btext;
            TextFont = font;
            XGet = xer;
            YGet = yer;
            MaxX = maxx;
        }

        public override void MouseEnter()
        {
        }

        public override void MouseLeave()
        {
        }

        public override void MouseLeftDown()
        {
        }

        public override void MouseLeftUp()
        {
        }

        public override void Init()
        {
        }

        public override void Render(double delta, int xoff, int yoff)
        {
            TextFont.DrawColoredText(MaxX != null ? TextFont.SplitAppropriately(Text,MaxX()): Text, new Location(GetX() + xoff, GetY() + yoff, 0), int.MaxValue, 1, false, BColor);
        }

        public float GetWidth()
        {
            Location size = TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor);
            return (float)size.X;
        }

        public float GetHeight()
        {
            Location size = TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor);
            return (float)size.Y;
        }

        public float GetX()
        {
            return XGet.Invoke();
        }

        public float GetY()
        {
            return YGet.Invoke();
        }

        public override bool Contains(int x, int y)
        {
            float tx = GetX();
            float ty = GetY();
            Location size = TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor);
            return x > tx && x < tx + size.X
                && y > ty && y < ty + size.Y;
        }
    }
}
