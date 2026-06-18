using System.Collections.Generic;

public static class GameData 
{
    public static int roundsCount = 2;
    public static int playersCount = 3;
    public static List<string> playerNames = new List<string>();
    public static int imposterCount = 1;
    public static bool canImposterHaveWord = false;
    public static bool isOnline = false;
    public static float votingDuration = 30f;
}
