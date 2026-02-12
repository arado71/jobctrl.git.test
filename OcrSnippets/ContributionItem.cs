using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace TcT.ActivityRecorderClient.Capturing.Plugins.Ocr
{
    [DataContract]
	public partial class ContributionItem
    {
	    private Guid guid = Guid.NewGuid();
        [DataMember]
        public Image Image { get; set; }
	    [DataMember]
		public Guid Guid { get { return guid; }}
	    [DataMember]
	    public string Content { get; set; }
	    [DataMember]
		public string UserId { get; set; }
	    [DataMember]
	    public Rectangle Area { get; set; }
	    [DataMember]
	    public string ProcessName { get; set; }
		[DataMember]
		public string WindowTitle { get; set; }
	}
}