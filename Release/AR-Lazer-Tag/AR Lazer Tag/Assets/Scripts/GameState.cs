

/// <summary>
/// Represents the different states that server and the client can be in at any given moment
/// </summary>
public enum GameState
{
    Offline,
    Host,
    Pregame,
    Waiting,
    Alive,
    Dead
}