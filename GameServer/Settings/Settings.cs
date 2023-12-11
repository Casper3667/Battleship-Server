using System.Text.Json;

namespace GameServer.Settings
{
    public static class Settings
    {
        public static bool IsSettingsLoaded { get; private set; } = false;

        public static ServerSettings ServerSettings { get; private set; }

        public static void LoadSettings()
        {
            Console.WriteLine("Game Server: Load Settings Called");
            if (IsSettingsLoaded == false)
            {
                Console.WriteLine("Loading Game Server Settings");
                LoadAppSettings();
                IsSettingsLoaded = true;
                Console.WriteLine("Settings Are Loaded" +
                    "\nChatServiceIP: " + ServerSettings.ChatServiceIP+
                    "\nChatServicePort: " + ServerSettings.ChatServicePort);
            }
        }
        public static ServerSettings LoadAppSettings()
        {
            //return new AppSettings() { Token= "Thesecretstomakeatokenkeyistodothis" };
            var settings = LoadJSON<ServerSettings>("ServerSettings.JSON");
            ServerSettings = settings[0];
            return settings[0];

        }


        public static string GetPathToSettingsFile(string FileName)
        {
            var newFileName = "/Settings/" + FileName;


#if DEBUG
            newFileName = "Settings\\" + FileName;
            //Testing.Print("Is In Debug Mode");
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Testing.Print("Path: " + System.IO.Path.GetDirectoryName(path));

            string Identifier = "\\GameServer\\GameServer\\";
            string TestIdentifier = "\\GameServer\\GameTest\\"; // Indicates that it is a test and splits where the Test Begins to get the part of the Path that should vary form computer to Computer
            string forginPath = "";
            if (path.Contains(Identifier))
            {
                forginPath = path.Split(Identifier)[0];
            }
            else if (path.Contains(TestIdentifier))
            {
                forginPath = path.Split(TestIdentifier)[0];
            }

            // Testing.Print("forgin Path: " + forginPath);

            string newPath = forginPath + Identifier;
            // Testing.Print("new Path: " + newPath);
            newFileName = newPath /*+ "Settings\\"*/ + newFileName;
            // Testing.Print("views Path: " + FileName);


#endif



            return newFileName;
        }
        private static List<T> LoadJSON<T>(string FileName)
        {
            FileName = GetPathToSettingsFile(FileName);

          

            List<T> items = new List<T>();
            using (StreamReader r = new StreamReader(FileName))
            {
                string json = r.ReadToEnd();
                items = JsonSerializer.Deserialize<List<T>>(json);

            }


            return items;
        }
    }
}
