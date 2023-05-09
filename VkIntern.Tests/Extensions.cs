using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkIntern.Tests
{
	public static class Extensions
	{
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		public static string SerializeToJSON(this object? objToSerialize)
		{
			return JsonConvert.SerializeObject(objToSerialize, new JsonSerializerSettings()
			{
				//ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
		}
	}
}
