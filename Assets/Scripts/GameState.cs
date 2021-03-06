using System.Collections.Generic;

public static class GameState 
{
    public static Queue<string> GodsMarked = new Queue<string>();
    public static State state = State.NotStarted;

    public static void GameStart() => state=State.Ongoing;
    public static void GameLost() => state=State.Lost;
    public static void GameWon() => state=State.Won;

    public static string GodWon = null;

    public static Mode GameMode = Mode.Cult;

}

public enum State{
    NotStarted,Ongoing,Won,Lost
}

public enum Mode{
    Cult,FightCult
}
