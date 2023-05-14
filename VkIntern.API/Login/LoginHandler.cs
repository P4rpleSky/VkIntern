using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace VkIntern.API.Login
{
	public class LoginHandler : ILoginHandler
	{
		private readonly ConcurrentDictionary<string, CancellationTokenSource> _loginTokenDict;

		public LoginHandler()
		{
			_loginTokenDict = new ConcurrentDictionary<string, CancellationTokenSource>();
		}

		public void AddLoginWithToken(string login, CancellationTokenSource token)
		{
			_loginTokenDict.TryAdd(login, token);
		}

		public CancellationTokenSource GetToken(string login)
		{
			var cancellationToken = new CancellationTokenSource();
			return _loginTokenDict.GetOrAdd(login, x => cancellationToken);
		}

		public bool IsInRecent(string login)
		{
			return _loginTokenDict.ContainsKey(login);
		}

		public void RemoveLogin(string login)
		{
			CancellationTokenSource? token;
			_loginTokenDict.TryGetValue(login, out token);
			if (token != null)
				_loginTokenDict.TryRemove(
					new KeyValuePair<string, CancellationTokenSource>(
						login, 
						token));
		}
	}
}
