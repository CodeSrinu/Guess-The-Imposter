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


    public static void ResetData()
    {
        roundsCount = 0;
        playersCount = 0;
        imposterCount = 0;
        canImposterHaveWord = false;
        votingDuration = 0.0f;
        playerNames.Clear();
    }
}


