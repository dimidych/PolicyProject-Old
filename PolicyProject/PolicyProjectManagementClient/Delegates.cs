namespace PolicyProjectManagementClient
{
    public delegate void EmbeddedCollectionRefreshed(string collectionName);

    public delegate void LoginApplied(LoginEventArgs loginArgs);

    public delegate void MessageSended(string message);
}