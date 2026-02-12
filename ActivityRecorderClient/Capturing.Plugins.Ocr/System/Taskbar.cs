using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
    public class Taskbar
    {
        public Rectangle Area { get; private set; }
        public TaskbarPosition Position { get; private set; }

        public static List<Taskbar> FindDockedTaskBars()
        {
            List<Taskbar> res = new List<Taskbar>();
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.Equals(screen.WorkingArea)) continue;
                Rectangle rect = new Rectangle();
                TaskbarPosition pos = TaskbarPosition.NONE;

                var leftDockedWidth = Math.Abs((Math.Abs(screen.Bounds.Left) - Math.Abs(screen.WorkingArea.Left)));
                var topDockedHeight = Math.Abs((Math.Abs(screen.Bounds.Top) - Math.Abs(screen.WorkingArea.Top)));
                var rightDockedWidth = ((screen.Bounds.Width - leftDockedWidth) - screen.WorkingArea.Width);
                var bottomDockedHeight = ((screen.Bounds.Height - topDockedHeight) - screen.WorkingArea.Height);
                if (leftDockedWidth > 0)
                {
                    rect.X = screen.Bounds.Left;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = leftDockedWidth;
                    rect.Height = screen.Bounds.Height;
                    pos = TaskbarPosition.Left;
                }
                else if (rightDockedWidth > 0)
                {
                    rect.X = screen.WorkingArea.Right;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = rightDockedWidth;
                    rect.Height = screen.Bounds.Height;
                    pos = TaskbarPosition.Right;
                }
                else if (topDockedHeight > 0)
                {
                    rect.X = screen.WorkingArea.Left;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = screen.WorkingArea.Width;
                    rect.Height = topDockedHeight;
                    pos = TaskbarPosition.Top;
                }
                else if (bottomDockedHeight > 0)
                {
                    rect.X = screen.WorkingArea.Left;
                    rect.Y = screen.WorkingArea.Bottom;
                    rect.Width = screen.WorkingArea.Width;
                    rect.Height = bottomDockedHeight;
                    pos = TaskbarPosition.Bottom;
                }
                res.Add(new Taskbar { Area = rect, Position = pos });
            }
            if (res.Count == 0)
            {
                // Taskbar is set to "Auto-Hide".
            }
            return res;
        }
    }
    public enum TaskbarPosition
    {
        NONE, Left, Top, Right, Bottom
    }
}
