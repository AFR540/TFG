using System.Collections.Generic;

[System.Serializable]
public class PlayerRecord
{
    public string playerName;
    public Dictionary<string, string> scores;

    public PlayerRecord()
    {
        scores = new Dictionary<string, string>()
        {
            { "Atletismo", "No Puntuado" },
            { "Boxeo", "No Puntuado" },
            { "Baloncesto", "No Puntuado" },
            { "Piragua", "No Puntuado" },
            { "Hípica", "No Puntuado" }
        };
    }
}
