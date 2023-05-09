namespace VkIntern.API.Login
{
	public interface ILoginHandler
	{
		CancellationTokenSource GetToken(string login);
		void AddLoginWithToken(string login, CancellationTokenSource token);
		void RemoveLogin(string login);
		bool IsInRecent(string login);
	}
}
