using Reporter.Model;

namespace Reporter.Interfaces
{
	public interface IMobileWorkItem : IWorkItem
	{
		long Imei { get; }
        MobileWorkitemType MobileWorkitemType { get; }
        long? CallId { get; }
    }
}
