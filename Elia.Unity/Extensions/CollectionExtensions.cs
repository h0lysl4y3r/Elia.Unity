using System.Collections;

namespace Elia.Unity.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ICollection"/> interface.
    /// </summary>
	public static class CollectionExtensions
	{
        /// <summary>
        /// Checks whether <paramref name="collection"/> is null or empty.
        /// </summary>
        /// <param name="collection">Instance of <see cref="ICollection"/></param>
        /// <returns>True if <paramref name="collection"/> is null or empty</returns>
		public static bool IsNullOrEmpty(this ICollection collection)
		{
			return collection == null || collection.Count == 0;
		}
	}
}
