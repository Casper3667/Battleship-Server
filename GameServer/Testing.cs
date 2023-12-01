namespace GameServer
{
    public static class Testing
    {
        public static bool IsTesting=false;


        public static void Print(string message)
        {
            if(IsTesting)
            {
                Console.WriteLine(message);
            }
            
        }
        
    }
}
