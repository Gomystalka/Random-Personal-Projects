using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

class LOLHide {
    internal static readonly string lineSeparator = Environment.NewLine;
    private bool found;
    private List<string> usableDrives = new List<string>();
    public List<string> leagueSystemPaths = new List<string>();
    private FileInfo configurationFile = new FileInfo("lolhide.conf");
    private bool configurationValuesEmpty = false;
    private readonly Dictionary<string, string> configurationDictionary = new Dictionary<string, string>();
    public bool configLoaded;
    public bool serverState = false;

    public static bool debug = false;
    public static bool credits = false;

    public Server selectedServer;
    public string mainLeaguePath;

    static void Main(string[] args) {
        if (args.Length > 0) {
            if (args[0] == "-d")
            {
                debug = true;
                Karafuru("Debug mode enabled." + lineSeparator);
                Console.WriteLine("Press any key to continue..." + (debug ? lineSeparator : ""));
                Console.ReadKey();
            }
            else if (args[1] == "-c") {
                credits = true;
                Karafuru("Created fully by Tomasz Galka (tommy.galk@gmail.com) Discord Nanachi#0526" + lineSeparator);
            }

            if (args.Length > 1) {
                if (args[1] == "-c")
                {
                    credits = true;
                    Karafuru("Created fully by Tomasz Galka (tommy.galk@gmail.com) Discord Nanachi#0526" + lineSeparator);
                } else if (args[1] == "-d") {
                    debug = true;
                    Karafuru("Debug mode enabled." + lineSeparator);
                    Console.WriteLine("Press any key to continue..." + (debug ? lineSeparator : ""));
                    Console.ReadKey();
                }
            }
        }

        LOLHide p = new LOLHide();
        Console.Title = "LOLHide";
        p.Init();
    }

    private static void Karafuru(string str) {
        char[] strChars = str.ToCharArray();
        ConsoleColor[] colors = { ConsoleColor.Red, ConsoleColor.DarkYellow, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Cyan};
        int colorIndex = 0;
        for (int k = 0; k < strChars.Length; k++) {
            if (colorIndex < colors.Length) {
                Console.ForegroundColor = colors[colorIndex];
                colorIndex++;
            }
            else {
                colorIndex = 0;
                Console.ForegroundColor = colors[colorIndex];
                colorIndex++;
            }
            Console.Write(strChars[k]);
            if (lineSeparator.Contains(strChars[k]))
            {
                //Console.WriteLine();
            }
            //Console.ResetColor();
        }
    }

    public void Init() {
        ScanForConfigurationFile();

        if (leagueSystemPaths.Count == 0) {
            Print(lineSeparator + "[Warning] Using this application will make you appear offline on the server of your choice." + lineSeparator + "South East Asia, Korea and China servers are not supported due to client restrictions." + lineSeparator + "This application will block any client chats (Friend list and champion select) In-game chat still works." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
            Print("Press any key to continue..." + (debug ? lineSeparator : ""));
            End();
            Clear();
            CollectDriveInfo();
            SearchForLeagueOfLegendsInstallDirectory();
        }
        else {
            Print("[Configuration] League of Legends path recognised, skipping auto-detection." + (debug ? lineSeparator : ""));
        }

        if (leagueSystemPaths.Count > 0) {
            Run(selectedServer == null);
        } else {
            Print("[Error] League of Legends was not found on your computer." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
            Print("[Info] A configuration file has been generated, please specify the path to the 'Riot Games' folder within it and restart this application." + (debug ? lineSeparator : ""));
            Print("Press any key to continue..." + (debug ? lineSeparator : ""));
            End();
        }
    }

    public void Reset() {
        Thread.Sleep(2000); //Reset after 2000ms
        Clear();
        found = false;
        usableDrives.Clear();
        leagueSystemPaths.Clear();
        configurationValuesEmpty = true;
        configurationDictionary.Clear();
        configLoaded = false;
        Init();
    }

    public void SearchForLeagueOfLegendsInstallDirectory() {
        foreach (string drive in usableDrives) {
            SearchDrive(drive);
            if (found) { 
                found = false;
                break;
            }
        }
    }

    public void CollectDriveInfo() {
        DriveInfo[] drives = DriveInfo.GetDrives();

        foreach (DriveInfo drive in drives) {
            if (drive.IsReady) {
                if (drive.DriveType == DriveType.Fixed) {
                    usableDrives.Add(drive.Name);
                }
            }
        }
    }
    
    public void SearchDrive(string drivePath) {
        try {
            foreach (string dir in Directory.GetDirectories(drivePath)) {
                if (dir.ToLower().Contains("riot games")) {
                    found = VerifyPath(dir);
                    if (found) {
                        mainLeaguePath = dir;
                        UpdateConfigFile();
                        break;
                    }
                }
            }
        }
        catch (Exception e) {
            Print(e.Message, ErrorLevel.ERROR);
            Print(e.StackTrace, ErrorLevel.ERROR);
        }
    }

    private bool VerifyPath(string path) {
        DirectoryInfo legends = new DirectoryInfo(path + "\\League of Legends\\RADS\\projects\\league_client\\releases");
        List<string> versionPaths = new List<string>();

        foreach (string ver in Directory.GetDirectories(legends.FullName)) {
            if (!ver.ToLower().Contains("installer")) {
                versionPaths.Add(ver);
            }
        }

        foreach (string verPasu in versionPaths) {
            FileInfo fairu = new FileInfo(verPasu + "\\deploy\\system.yaml");
            if (fairu.Exists) {
                leagueSystemPaths.Add(fairu.FullName);
            }
        }

        return leagueSystemPaths.Count > 0;
    }

    public void ScanForConfigurationFile() {
        Print("[Configuration] Configuration file expected path " + configurationFile.FullName + (debug ? lineSeparator : ""));
        configurationFile = new FileInfo(configurationFile.FullName);
        if (configurationFile.Exists) {
            Print("[Configuration] Configuration file found, it will be loaded." + (debug ? lineSeparator : ""));
            LoadConfig();
        } else {
            Print("[Configuration] Configuration file could not be found. A new file will be generated." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
            GenerateConfig(false);
            Reset();
        }
    }

    public void Run(bool firstRun) {
        if (firstRun)
        {
            Console.Title = "LOLHide - Server Selection";
            Print("Select your region" + lineSeparator);
            for (int i = 0; i < Server.Values.Count(); i++)
            {
                Print("[" + (i + 1) + "] " + Server.Values.ToArray()[i] + (debug ? lineSeparator : ""));
            }
            //Print("[" + (Server.Values.Count() + 1) + "] " + "Restore backups (In case your League of Legends client fails to load.)");


            string choice = Console.ReadLine();
            int c = ProcessString(choice);

            if (c == -1)
            {
                Print("[Warning] Invalid input type." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
                Run(false);
            }
            else
            {
                if (c > Server.Values.Count() || c < 1)
                {
                    Print("[Warning] That is not a valid choice." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
                    Run(false);
                }
                else
                {
                    Clear();
                    if (c < Server.Values.Count())
                    {
                        selectedServer = Server.Values.ToArray()[c - 1];
                        //Print("Selected server: " + selectedServer);
                        UpdateConfigFile();
                        //UpdateGameFiles(false);
                        ModifyGameFiles();
                        ActionSelect(false);
                    }
                    //End();
                }
            }
        }
        else {
            ModifyGameFiles();
            ActionSelect(true);
        }
    }
    private void ActionSelect(bool delay) {
        ActionSelect(delay, true);
    }

    public static void Clear() {
        Console.Clear();
        if (credits)
        {
            Karafuru("Created fully by Tomasz Galka (tommy.galk@gmail.com) Discord Nanachi#0526" + lineSeparator + lineSeparator);
        }
    }

    private void ActionSelect(bool delay, bool printSelect) {
        UpdateGameFiles(false, true);
        if (delay)
        {
            Thread.Sleep(2000);
            Clear();
        }
        if (selectedServer != null)
        {
            Console.Title = "LOLHide - " + selectedServer + " - " + (serverState ? "Offline" : "Online");
            if (printSelect)
            {
                Print("[Info] You are appearing " + (serverState ? "offline" : "online") + " on " + selectedServer.ToString() + "." + (debug ? lineSeparator : ""));
                Print("[Important] Appearing offline/online requires a client restart." + lineSeparator, ErrorLevel.WARNING);
                Print("Select an action" + (debug ? lineSeparator : ""));
                Print("[1] Appear offline on " + selectedServer.ToString() + (debug ? lineSeparator : ""));
                Print("[2] Appear online on " + selectedServer.ToString() + (debug ? lineSeparator : ""));
                Print("[3] Change server" + (debug ? lineSeparator : ""));
                Print("[4] Restore game files from backups. (In case your League of Legends fails to load.)" + (debug ? lineSeparator : ""));
            }
            ConsoleKeyInfo cki = Console.ReadKey(true);
            switch (cki.Key)
            {
                case ConsoleKey.D1:
                    UpdateGameFiles(true);
                    ActionSelect(true);
                    break;
                case ConsoleKey.D2:
                    UpdateGameFiles(false);
                    ActionSelect(true);
                    break;
                case ConsoleKey.D3:
                    Clear();
                    Run(true);
                    break;
                case ConsoleKey.D4:
                    foreach (string path in leagueSystemPaths)
                    {
                        RestoreBackup(path);
                    }
                    Print("Restored." + (debug ? lineSeparator : ""));
                    Print("Upon launching this application, the game files are modified." + lineSeparator + "The application will now close as it is unable to work without these modifications." + (debug ? lineSeparator : ""));
                    End();
                    Environment.Exit(0); //Terminate the application.
                    //ActionSelect(true);
                    break;
                default:
                    Print("[Warning] Invalid option" + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
                    ActionSelect(false, false);
                    break;
            }
        }
        else {
            Print("[Error] No server selected." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
        }
    }

    private void ModifyGameFiles() {
        foreach (string pasu in leagueSystemPaths) {
            string content = File.ReadAllText(pasu);
            if (!content.Contains("block_active")) {
                Modify(pasu);
            }
        }
    }

    private void UpdateGameFiles(bool state) {
        UpdateGameFiles(state, false);
    }

    private void UpdateGameFiles(bool state, bool stateCheck) {
        if (leagueSystemPaths.Count > 0) {
            for (int i = 0; i < leagueSystemPaths.Count; i++) {
                string[] contents = File.ReadAllLines(leagueSystemPaths[i]);

                int sIndex = 0;
                    for (int k = 0; k < contents.Length; k++) {
                        string address = "";
                        if (k - 2 >= 0) {
                            address = contents[k - 2];
                        }
                        string line = contents[k];
                        if (selectedServer == null) {
                            Print("[Error] Error updating game files." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
                            break;
                        }

                        //Check for server location.
                        if (line.ToLower().Contains("block_active") && address.ToLower().Contains("chat_host")) {
                            if (sIndex == Server.IndexOf(selectedServer)) {
                                string blockStateStr = line.Substring(line.IndexOf(":"));
                                blockStateStr = RebuildString(blockStateStr);
                                bool.TryParse(blockStateStr, out bool blockState);

                                serverState = blockState;
                                
                                if (blockState && state)
                                {
                                    if (!stateCheck) {
                                        Print("[Info] You are already appearing offline on " + selectedServer.ToString() + "." + (debug ? lineSeparator : ""));
                                    return;
                                    }
                                }
                                else if (!blockState && state)
                                {
                                    address = address.Substring(0, address.IndexOf(":") + 1); //Returns chat_host:
                                    string newAddress = address + " " + selectedServer.ReplacementAddress;
                                    line = line.Substring(0, line.IndexOf(":") + 1); //Returns block_active:
                                    string newState = line + " true";
                                    contents[k - 2] = newAddress;
                                    contents[k] = newState;
                                    if (i == 0)
                                    {
                                        if (!stateCheck) {
                                            Print("You will now appear offline on " + selectedServer.ToString() + (debug ? lineSeparator : ""));
                                        }
                                    }
                                }
                                else if (blockState && !state)
                                {
                                    address = address.Substring(0, address.IndexOf(":") + 1); //Returns chat_host:
                                    string newAddress = address + " " + selectedServer.ServerAddress;
                                    line = line.Substring(0, line.IndexOf(":") + 1); //Returns block_active:
                                    string newState = line + " false";
                                    contents[k - 2] = newAddress;
                                    contents[k] = newState;
                                    if (i == 0)
                                    {
                                        if (!stateCheck) {
                                            Print("You will now appear online on " + selectedServer.ToString() + (debug ? lineSeparator : ""));
                                        }
                                    }
                                }
                                else {
                                    if (!stateCheck) {
                                        Print("[Info] You are not appearing offline on " + selectedServer.ToString() + "." + (debug ? lineSeparator : ""));
                                        return;
                                    }
                                }
                            }
                            sIndex++;
                        }
                    }
                if (!stateCheck)
                {
                    File.WriteAllLines(leagueSystemPaths[i], contents);
                }
            }
        }
    }

    private string RebuildString(string str) {
        char[] strChars = str.ToCharArray();
        StringBuilder sb = new StringBuilder();
        foreach (char c in strChars) {
            if (!char.IsWhiteSpace(c) && !char.IsSymbol(c) && char.IsLetter(c)) {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private void Modify(string path) {
        string sys = "";
        string[] contents = File.ReadAllLines(path);
        int index = 0;
        foreach (string l in contents) {
            sys += l + lineSeparator;
            if (index < contents.Length) {
                if (l.ToLower().Contains("chat_port") && !contents[index + 1].ToLower().Contains("block_active")) {
                    string toReplace = l.Substring(l.ToLower().IndexOf("chat"));
                    toReplace = l.Replace(toReplace, "block_active: false");
                    sys += toReplace + lineSeparator;
                }
            }
            index++;
        }
        if (debug)
        {
            Print("[Info] Entries added to " + path + (debug ? lineSeparator : ""));
        }
        CreateBackup(path);
        File.WriteAllText(path, sys);
    }

    private void CreateBackup(string path) {
        string newPath = path.Substring(0, path.LastIndexOf(".")) + ".back";
        FileInfo backupFile = new FileInfo(newPath);
        if (!backupFile.Exists) {
            byte[] contents = File.ReadAllBytes(path);
            File.WriteAllBytes(newPath, contents);
            if (debug)
            {
                Print("[Backup] Backup created at " + newPath + (debug ? lineSeparator : ""));
            }
        } else {
            if (debug)
            {
                Print("[Backup] Backup for " + new FileInfo(path).Name + " already exists. Operation skipped." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
            }
        }
    }

    private void RestoreBackup(string path) {
        string nonBackPath = path;
        path = path.Substring(0, path.LastIndexOf(".")) + ".back";
        FileInfo backupFile = new FileInfo(path);
        FileInfo nonBackupFile = new FileInfo(nonBackPath);
        if (backupFile.Exists) {
            if (nonBackupFile.Exists) {
                File.Replace(path, nonBackPath, null);
                if (debug) {
                    Print("File " + nonBackupFile.FullName + " restored." + (debug ? lineSeparator : ""));
                }
            }
        } else {
            Print("[Backup] No backup file detected. Cannot restore." + (debug ? lineSeparator : ""));
        }
    }

    private int ProcessString(string str) {
        Regex rx = new Regex(@"[^\d]");
        str = rx.Replace(str, "");

        if (str != null && str != "") {
            return Convert.ToInt32(str);
        }
        else {
            return -1;
        }
    }

    public void End() {
        //Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public void Print(string str) {
        Print(str, ErrorLevel.NONE);
    }

    public void Print(string str, ErrorLevel level) {
        if (debug) {
            Karafuru(str);
            return;
        }
        ConsoleColor errorLevelColor = ConsoleColor.White;
        switch (level) {
            case ErrorLevel.WARNING:
                errorLevelColor = ConsoleColor.Yellow;
                break;
            case ErrorLevel.ERROR:
                errorLevelColor = ConsoleColor.Red;
                break;
            case ErrorLevel.CRITICAL_ERROR:
                errorLevelColor = ConsoleColor.DarkRed;
                Console.Beep();
                break;
            default:
                errorLevelColor = ConsoleColor.White;
                break;
        }
        Console.ForegroundColor = errorLevelColor;
        Console.WriteLine(str);
        Console.ResetColor();
    }

    private void GenerateConfig(bool isReplacement) {
        Dictionary<string, string> configDictionary = new Dictionary<string, string>
        {
            { "Server", "" },
            { "LolPath", "" }
        };
        string configurationString = "";

        foreach (KeyValuePair<string, string> element in configDictionary) {
            configurationString += element.Key + "=" + element.Value + lineSeparator;
        }

        File.WriteAllText(configurationFile.FullName, configurationString);
        if (isReplacement) {
            Print("[Configuration] Corrupted configuration file replaced." + (debug ? lineSeparator : ""));
        }
        else {
            Print("[Configuration] New configuration file generated at " + configurationFile.FullName + (debug ? lineSeparator : ""));
        }
        configLoaded = true;
    }

    private void LoadConfig() {
        try {
            string[] configurationEntries = File.ReadAllLines(configurationFile.FullName);
            if (ParseConfig(configurationEntries)) {
                foreach (KeyValuePair<string, string> pair in configurationDictionary) {
                    switch (pair.Key.ToLower()) {
                        case "server":
                            selectedServer = Server.TryParse(pair.Value);
                            break;
                        case "lolpath":
                            mainLeaguePath = pair.Value;
                            break;
                        default:
                            break;
                    }
                }
                Print("[Configuration] Configuration file loaded." + (debug ? lineSeparator : ""));
                configLoaded = true;
            }
            else {
                if (!configurationValuesEmpty) { //Don't replace the file if the reason for the fail is empty values.
                    configurationValuesEmpty = false;
                    Print("[Configuration] Failed to load configuration file." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
                    GenerateConfig(true);
                    Reset();
                }
            }
        } catch (Exception e) {
            configLoaded = false;
            Print(e.Message, ErrorLevel.ERROR);
            Print(e.StackTrace, ErrorLevel.ERROR);
        }
    }

    private bool ParseConfig(string[] entries) { //Returns true if parsing succeeds, otherwise this function will return false.
        bool fail = false;
        bool immediateFail = false;
        if (entries.Length > 2 || entries.Length < 2) { //Fail immediately if there are more than two or less than 2 entries.
            return false;
        } 
        for (int i = 0; i < entries.Length; i++) { //Loop through entries. There won't be more than two but this is here for other possible entries.
            try {
                string entry = entries[i];
                string key = entry.Substring(0, entry.IndexOf("="));
                string value = entry.Substring(entry.IndexOf("=") + 1);

                if (key.Any(char.IsWhiteSpace)) { //If there are ANY whitespace characters within the key string, fail the parse.
                    Print("[Configuration] Error at key [" + key + "]. No whitespace characters are allowed within the key." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
                    fail = true;
                    immediateFail = true;
                }

                if (i == 0 && key.ToLower() != "server") { //If the first entry is not server, fail the parse.
                    Print("[Configuration] Error at key [" + key + "] expected key [Server] is not present." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
                    fail = true;
                } else if (i == 0 && key.ToLower() == "server") {
                    if (!Server.IsValidServer(value)) { //If the specified server is not valid, fail the parse.              
                        fail = true;
                    } else {
                        configurationDictionary.Add(key, value); //Add entry to the configuration dictionary if both key and value are valid.
                    }
                }

                if (i == 1 && key.ToLower() != "lolpath") { //If the second entry is not lolpath, fail the parse.
                    Print("[Configuration] Error at key [" + key + "] expected key [LolPath] is not present." + (debug ? lineSeparator : ""), ErrorLevel.ERROR);
                    fail = true;
                } else if (i == 1 && key.ToLower() == "lolpath") {
                    if (!IsValidPath(value)) { //If the specified path is not valid, fail the parse.
                        fail = true;
                    }
                    else {
                        configurationDictionary.Add(key, value);
                    }
                }

                if (string.IsNullOrWhiteSpace(value)) {
                    Print("[Configuration] Value of key [" + key + "] is empty." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
                    configurationValuesEmpty = true;
                }
            }
            catch (Exception e) {
                Print("[Configuration] Could not parse configuration file. " + e.Message + (debug ? lineSeparator : ""), ErrorLevel.CRITICAL_ERROR);
                configurationValuesEmpty = false;
                return !(fail = true); //Set the value of fail to true and return the opposite (false)
            }
        }

        if (immediateFail) {
            configurationValuesEmpty = false;
            return !immediateFail;
        }

        return !fail;
    }

    public void UpdateConfigFile() {
        if (SuspendUntilFileExists(configurationFile)) {
            string serverStr = selectedServer == null ? "" : selectedServer.ToString();
            string[] values = { serverStr, mainLeaguePath };
            string[] entries = File.ReadAllLines(configurationFile.FullName);
            for (int i = 0; i < entries.Length; i++) {
                if (i < values.Length) {
                    string entry = entries[i];
                    string key = entry.Substring(0, entry.IndexOf("="));
                    if (key.ToLower() == "server") {
                        entry = key + "=" + values[0];
                    }

                    if (key.ToLower() == "lolpath") {
                        entry = key + "=" + values[1];
                    }

                    if (!string.IsNullOrEmpty(values[i]) && !string.IsNullOrWhiteSpace(values[i])) {
                        entries[i] = entry;
                    }
                }
            }
            File.WriteAllLines(configurationFile.FullName, entries);
            Print("[Configuration] Configuration file updated." + (debug ? lineSeparator : ""));
        } else {
            Print("[Configuration] Configuration file does not exist." + (debug ? lineSeparator : ""), ErrorLevel.WARNING);
        }
    }

    public bool SuspendUntilFileExists(FileInfo file) {
        int attempts = 0;
        while (!file.Exists) {
            Thread.Sleep(200);
            if (attempts > 40) { //If the file doesn't exist after 8 seconds (200ms * 40) return false.
                return false;
            }
            attempts++;
        }
        return true;
    }
    public bool IsValidPath(string path) { //This function returns true if the path is set to the path of the 'Riot Games' folder.
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)) return false;
        DirectoryInfo pasu = new DirectoryInfo(path);
        if (pasu.Exists) {
            return VerifyPath(path);
        }
        return false;
    }
}

[Serializable]
class Server { //Emulates Java-styled Enum
    public static readonly Server BR = new Server("BR", "chat.br.lol.riotgames.com", "127.0.0.1.BR");
    public static readonly Server EUNE = new Server("EUNE", "chat.eun1.lol.riotgames.com", "127.0.0.1.EUNE");
    public static readonly Server EUW = new Server("EUW", "chat.euw1.lol.riotgames.com", "127.0.0.1.EUW");
    public static readonly Server LAN = new Server("LAN", "chat.la1.lol.riotgames.com", "127.0.0.1.LAN");
    public static readonly Server LAS = new Server("LAS", "chat.la2.lol.riotgames.com", "127.0.0.1.LAS");
    public static readonly Server NA = new Server("NA", "chat.na2.lol.riotgames.com", "127.0.0.1.NA");
    public static readonly Server OCE = new Server("OCE", "chat.oc1.lol.riotgames.com", "127.0.0.1.OCE");
    public static readonly Server RU = new Server("RU", "chat.ru.lol.riotgames.com", "127.0.0.1.RU");
    public static readonly Server TR = new Server("TR", "chat.tr.lol.riotgames.com", "127.0.0.1.TR");
    public static readonly Server JP = new Server("JP", "chat.jp1.lol.riotgames.com", "127.0.0.1.JP");
    public static readonly Server PBE = new Server("PBE", "chat.na2.lol.riotgames.com", "127.0.0.1.PBE");

    public static IEnumerable<Server> Values {
        get {
            yield return BR;
            yield return EUNE;
            yield return EUW;
            yield return JP;
            yield return LAN;
            yield return LAS;
            yield return NA;
            yield return OCE;
            yield return RU;
            yield return PBE;
            yield return TR;
        }
    }

    public string Name { get; private set; }
    public string ServerAddress { get; private set; }
    public string ReplacementAddress { get; private set; }

    Server(string name, string serverAddress, string replacementAddress) {
        Name = name;
        ServerAddress = serverAddress;
        ReplacementAddress = replacementAddress;
    }

    public static Server TryParse(string server) { //Try to parse the Server string.
        Server[] values = Values.ToArray();
        if (IsValidServer(server)) {
            foreach (Server s in values) {
                if (s.ToString().ToLower() == server.ToLower()) {
                    return s;
                }
            }
        }
        return null;
    }

    public static bool IsValidServer(string server) { //Check if the specified server is valid.
        Server[] values = Values.ToArray();
        foreach (Server s in values) {
            if (s.ToString().ToLower() == server.ToLower()) {
                return true;
            }
        }
        return false;
    }

    public static int IndexOf(Server server) {
        return Array.FindIndex(Values.ToArray(), element => element == server);
    }

    public override string ToString() {
        return Name;
    }
}

enum ErrorLevel {
    NONE,
    WARNING,
    ERROR,
    CRITICAL_ERROR
}