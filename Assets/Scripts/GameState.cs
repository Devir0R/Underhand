public static class GameState 
{
    public static State state = State.NotStarted;

    public static void GameStart() => state=State.Ongoing;
    public static void GameLost() => state=State.Lost;
    public static void GameWon() => state=State.Won;

    public static string GodWon = null;

}

public enum State{
    NotStarted,Ongoing,Won,Lost
}
