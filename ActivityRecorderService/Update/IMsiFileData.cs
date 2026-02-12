namespace Tct.ActivityRecorderService.Update
{
	public interface IMsiFileData : IFileData
	{
		string MsiVersion { get; }
	}
}