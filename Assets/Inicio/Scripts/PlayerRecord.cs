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
            { "Atletismo 2", "No Puntuado" },
            { "Boxeo 2", "No Puntuado" },
            { "Baloncesto 2", "No Puntuado" },
            { "Piragua 2", "No Puntuado" },
        };
    }
}
