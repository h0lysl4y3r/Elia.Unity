namespace Elia.Unity.Logging
{
    /// <summary>
    /// <see cref="LoggerFactory"/> extension methods.
    /// </summary>
	public static class LoggerFactoryExtensions
	{
        /// <summary>
        /// Adds <see cref="UnityLogProvider"/> to factory.
        /// </summary>
        /// <param name="factory"><see cref="Logger"/> factory</param>
        /// <returns></returns>
		public static LoggerFactory AddUnityLog(this LoggerFactory factory)
		{
			factory.AddProvider(new UnityLogProvider());
			return factory;
		}
	}
}
