using System;
using System.Drawing;

namespace TcT.OcrSnippets
{
    public interface IContributionItem
    {
        Image Image { get; set; }
        Guid Guid { get; }
        string Content { get; set; }
        string UserId { get; set; }
        Rectangle Area { get; set; }
        string ProcessName { get; set; }
    }
}