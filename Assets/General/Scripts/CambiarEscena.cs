using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class CambiarEscena : MonoBehaviour
{
    [SerializeField] TMP_InputField player;
    [SerializeField] GameObject validatorText;

    private bool nombreValido = true;
    private bool checkNombre = false;

    public void Start()
    {
        if (validatorText != null) validatorText.SetActive(false);
    }
    public void CargarEscena(string escena)
    {
        if (checkNombre)
        {
            if (nombreValido)
            {
                AllRecords all = AllRecords.Load();
                all.AddNewPlayer(player.text);
                SceneManager.LoadScene(escena);
            }
        } else
        {
            SceneManager.LoadScene(escena);
        }
    }

    public void validarNombre()
    {
        checkNombre = true;
        if (player.text.Length < 3)
        {
            validatorText.SetActive(true);
            nombreValido = false;
        } else
        {
            nombreValido = true;
        }
    }

    public IEnumerator CargarSiguienteEscena(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        int totalEscenas = SceneManager.sceneCountInBuildSettings;

        // Si es la última, vuelve a la primera
        int siguiente = (escenaActual + 1) % totalEscenas;

        SceneManager.LoadScene(siguiente);
    }
}