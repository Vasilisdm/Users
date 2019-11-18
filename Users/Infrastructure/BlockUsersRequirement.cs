using Microsoft.AspNetCore.Authorization;

namespace Users.Infrastructure
{
    public class BlockUsersRequirement : IAuthorizationRequirement
    {
        public string[] BlockedUsers { get; set; }

        public BlockUsersRequirement(params string[] users)
        {
            BlockedUsers = users;
        }
    }
}
