namespace Tct.ActivityRecorderService.Update
{
	public interface IFileData
	{
		string FilePath { get; }
		byte[] Data { get; }
	}
}