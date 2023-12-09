namespace GameServer.StaticContent
{
    internal static class PlayerCount
    {
        internal static int Players { get {
                try
                {
                    return Program.Server.PlayerCount;
                }
                catch { return 99; } // If Cant Get PlayerCount Assume Server isnt running thus there isnt space
               
            } }
    }
}
