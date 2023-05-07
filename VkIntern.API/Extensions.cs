using Microsoft.EntityFrameworkCore;
using VkIntern.API.DbContexts;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;

namespace VkIntern.API
{
    public static class Extensions
    {
        public static IQueryable<User> GetUsersSummary(this ApplicationDbContext db)
        {
            return from user in db.Users
                   join userGroup in db.UserGroups on user.UserGroupId equals userGroup.Id
                   join userState in db.UserStates on user.UserStateId equals userState.Id
                   select new User
                   {
                       Id = user.Id,
                       Login = user.Login,
                       Password = user.Password,
                       CreatedDate = user.CreatedDate,
                       UserGroupId = user.UserGroupId,
                       UserGroup = userGroup,
                       UserStateId = user.UserStateId,
                       UserState = userState
                   };
        }

        //public static Task<List<TSource>> ToListAsyncSafe<TSource>(this IQueryable<TSource> source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException(nameof(source));
        //    if (!(source is IAsyncEnumerable<TSource>))
        //        return Task.FromResult(source.ToList());
        //    return source.ToListAsync();
        //}
    }

}
