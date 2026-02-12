using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;
using TcT.ActivityRecorderClient.SnippingTool;

namespace TcT.ActivityRecorderClient.Capturing.Plugins.Ocr
{
    public partial class ContributionItem
    {
        public DeviceInfo Monitor { set; get; }
        public EnumWindowsHelper.WindowInfo AppWindow { set; get; }

        //public Rectangle Correction
        //{
        //    get
        //    {
        //        if (Monitor.Taskbar.Position == TaskbarPosition.Left)
        //            return new Rectangle(Area.X + Monitor.Taskbar.Area.Width, Area.Y, Area.Width, Area.Height);
        //        return Area;
        //    }
        //}
    }
}
