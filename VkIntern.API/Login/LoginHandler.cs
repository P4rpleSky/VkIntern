namespace VkIntern.API.Login
{
	public class LoginHandler : ILoginHandler
	{
		private readonly Dictionary<string, CancellationTokenSource> _loginTokenDict;

		public LoginHandler()
		{
			_loginTokenDict = new Dictionary<string, CancellationTokenSource>();
		}

		public void AddLoginWithToken(string login, CancellationTokenSource token)
		{
			_loginTokenDict[login] = token;
		}

		public CancellationTokenSource GetToken(string login)
		{
			return _loginTokenDict[login];
		}

		public bool IsInRecent(string login)
		{
			return _loginTokenDict.ContainsKey(login);
		}

		public void RemoveLogin(string login)
		{
			_loginTokenDict.Remove(login);
		}
	}
}
