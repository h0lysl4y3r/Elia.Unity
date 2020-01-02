namespace Elia.Unity.Infrastructure
{
    /// <summary>
    /// Interface to notify other objects on pause status change.
    /// </summary>
	public interface INotifyPaused
	{
		void NotifyAppPaused(bool pauseStatus);
		void NotifyGamePaused(bool pauseStatus);
	}
}
