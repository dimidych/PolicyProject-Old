using System.Security;

namespace PolicyProjectManagementClient
{
    public interface IPasswordContainer
    {
        string HashedPassword { get; }
        string HashedElsePassword { get; }
    }
}