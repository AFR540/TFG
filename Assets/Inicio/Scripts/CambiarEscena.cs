using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class CambiarEscena : MonoBehaviour
{
    [SerializeField] TMP_InputField player;
    public void CargarEscena(string escena)
    {
        AllRecords all = AllRecords.Load();
        all.AddNewPlayer(player.text);
        SceneManager.LoadScene(escena);
    }
}