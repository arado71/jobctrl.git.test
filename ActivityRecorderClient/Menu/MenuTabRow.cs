using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.Menu
{
	class MenuTabRow
	{
		public int RowNumber { get; set; }
		public List<Element> Elements { get; set; }
		public int HeightPercent { get; set; }
		public bool AutoSize { get; set; } = true;

		public class Element
		{
			public int colNumber { get; set; }
			public Bitmap Image { get; set; }
			public Table Table { get; set; }
			public string Url { get; set; }
			public ElementType Type { get; set; }
			public string Text { get; set; }
			public Feature FeatureLink { get; set; }
			public bool IsBold { get; set; }
			public bool IsSmallCaps { get; set; }
		}

		public class Table
		{
			public TableCell[,] Content { get; set; }
			public int RowCount { get { return Content.GetLength(0); } }
			public int ColumnCount { get { return Content.GetLength(1); } }
		}

		public class TableCell
		{
			public Color BackgroundColor { get; set; }
			public Color TextColor { get; set; }
			public DataGridViewContentAlignment TextAlign { get; set; }
			public string Content { get; set; }
			public string Tooltip { get; set; }
			public bool TopBorder { get; set; }
			public bool RightBorder { get; set; }
			public bool BottomBorder { get; set; }
			public bool LeftBorder { get; set; }
			public bool IsBold { get; set; }
			public bool IsSmallCaps { get; set; }
			public Bitmap Image { get; set; }
		}

		public enum ElementType
		{
			Image,
			Table,
			Link,
			StatText,
			Label,
			FavoriteReport,
			FeatureLink
		}

		public enum Feature
		{
			Messaging = 0,
		}

		internal static Image GetFeatureImage(Feature featureLink)
		{
			switch (featureLink)
			{
				case Feature.Messaging:
					return Properties.Resources.messages_picto;
				default:
					return Properties.Resources.messages_picto;
			}
		}
	}
}
