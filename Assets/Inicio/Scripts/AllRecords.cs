using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

[System.Serializable]
public class RecordEntry
{
    public string id;
    public PlayerRecord record;
}

[System.Serializable]
public class AllRecords
{
    public List<RecordEntry> entries = new();

    // Este diccionario no se serializa, es para acceso rápido en runtime
    [JsonIgnore]
    private Dictionary<string, PlayerRecord> recordsDict = new();

    public void AddNewPlayer(string playerName)
    {
        int newId = GetNextId();
        string id = newId.ToString();

        PlayerRecord newRecord = new PlayerRecord();
        newRecord.playerName = playerName;

        entries.Add(new RecordEntry { id = id, record = newRecord });
        recordsDict[id] = newRecord;

        Save();
        PlayerPrefs.SetString("CurrentPlayerId", id);
    }

    public void GuardarPuntuacion(string nombrePrueba, string puntuacion)
    {
        string playerId = PlayerPrefs.GetString("CurrentPlayerId", null);

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("No hay ID de jugador actual en PlayerPrefs.");
            return;
        }

        if (!recordsDict.ContainsKey(playerId))
        {
            Debug.LogWarning($"No se encontró el jugador con ID {playerId}.");
            return;
        }

        PlayerRecord player = recordsDict[playerId];

        if (player.scores.ContainsKey(nombrePrueba))
            player.scores[nombrePrueba] = puntuacion;
        else
            player.scores.Add(nombrePrueba, puntuacion); // Por si acaso

        Save();
    }


    public void Save()
    {
        // Serializa con Newtonsoft.Json
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(GetPath(), json);
    }

    public static AllRecords Load()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                File.Delete(path);

                return new AllRecords();
            }

            AllRecords data = JsonConvert.DeserializeObject<AllRecords>(json);
            data.RebuildDictionary();
            return data;
        }
        else
        {
            return new AllRecords();
        }
    }

    public string GetUltimoResultado(string nombrePrueba)
    {
        var entry = entries.LastOrDefault(e => e.record.scores.ContainsKey(nombrePrueba));
        if (entry != null && entry.record.scores.TryGetValue(nombrePrueba, out string valorTexto))
        {
            return valorTexto;
        }

        return "";
    }

    public List<(string playerName, string puntuacion)> GetTop5Scores(string nombrePrueba, string unidadPuntuacion)
    {
        List<(string playerName, float puntuacion)> resultadosTemporales = new();

        foreach (var entry in entries)
        {
            if (entry.record.scores.TryGetValue(nombrePrueba, out string valorTexto))
            {
                // Extrae el número antes del primer carácter no numérico (ej: "10.5 m")
                string numero = new string(valorTexto.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

                if (float.TryParse(numero.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float valor))
                {
                    resultadosTemporales.Add((entry.record.playerName, valor));
                }
            }

            if (entry.record.scores.TryGetValue(nombrePrueba + " 2", out string valorTexto2))
            {
                // Extrae el número antes del primer carácter no numérico (ej: "10.5 m")
                string numero = new string(valorTexto2.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

                if (float.TryParse(numero.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float valor))
                {
                    resultadosTemporales.Add((entry.record.playerName, valor));
                }
            }
        }

        // Ordenar de mayor a menor puntuación
        resultadosTemporales.Sort((a, b) => b.puntuacion.CompareTo(a.puntuacion));

        // Convertir a formato string final
        List<(string playerName, string puntuacion)> resultadosFinales = new();
        foreach (var r in resultadosTemporales)
        {
            resultadosFinales.Add((r.playerName, $"{r.puntuacion} " + unidadPuntuacion));
        }

        // Rellenar con "admin" si hay menos de 5 resultados
        while (resultadosFinales.Count < 5)
        {
            resultadosFinales.Add(("admin", "No puntuado"));
        }

        return resultadosFinales;
    }



    private void RebuildDictionary()
    {
        recordsDict.Clear();
        foreach (var entry in entries)
        {
            recordsDict[entry.id] = entry.record;
        }
    }

    private static string GetPath() => Application.persistentDataPath + "/records.json";

    private int GetNextId()
    {
        int max = 0;
        foreach (var entry in entries)
        {
            if (int.TryParse(entry.id, out int id))
            {
                if (id > max) max = id;
            }
        }
        return max + 1;
    }
}
