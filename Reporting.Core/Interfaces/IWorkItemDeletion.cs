using Reporter.Model;

namespace Reporter.Interfaces
{
	public interface IWorkItemDeletion : IInterval
	{
		int UserId { get; }
		DeletionTypes Type { get; }
	}
}
