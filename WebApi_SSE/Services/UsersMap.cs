namespace WebApi_SSE.Services
{
    public static class UsersMap
    {
        public static readonly Dictionary<string, string> Users = new()
        {
            { "Alice Johnson", "alice.johnson@example.com" },
            { "Bob Smith", "bob.smith@example.com" },
            { "Charlie Brown", "charlie.brown@example.com" },
            { "Diana Prince", "diana.prince@example.com" },
            { "Eve Wilson", "eve.wilson@example.com" },
            { "Frank Miller", "frank.miller@example.com" },
            { "Grace Lee", "grace.lee@example.com" },
            { "Henry Davis", "henry.davis@example.com" },
            { "John Doe", "john.doe@example.com" },
            { "Jane Air", "jane.air@example.com" }
        };

        public static readonly HashSet<string> GetUserNames = [..Users.Keys];

        public static string? GetUserEmail(string userName)
        {
            return Users.GetValueOrDefault(userName);
        }
    }
}
