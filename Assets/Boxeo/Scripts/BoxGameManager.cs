using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoxGameManager : MonoBehaviour
{
    [SerializeField] Animator boxerAnimator;
    [SerializeField] TMP_Text tiempo;
    [SerializeField] TMP_Text puntos;
    [SerializeField] TMP_Text incrementoPuntos;
    [SerializeField] TMP_Text inicioFinalText;
    [SerializeField] GameObject deteccionUsuario;
    [SerializeField] GameObject deteccionDistanciaUsuario;

    MostrarPuntuaciones mp;
    private float tiempoRestante = 30f;
    private int puntuacion = 5;
    private bool activeGame = false;
    private bool userDetected = false;
    private bool userCorrectDistance = false;

    // Start is called before the first frame update
    void Start()
    {
        inicioFinalText.gameObject.SetActive(false);
        incrementoPuntos.gameObject.SetActive(false);
        mp = GetComponent<MostrarPuntuaciones>();
    }

    // Update is called once per frame
    void Update()
    {
        // Si no se detectan los tobillos, damos feedback al usuario
        deteccionUsuario.SetActive(!userDetected);
        // Si el usuario está a menos de la distancia mínima, damos feedback
        deteccionDistanciaUsuario.SetActive(!userCorrectDistance);
        if (activeGame)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                inicioFinalText.gameObject.SetActive(true);
                inicioFinalText.alpha = 1;
                inicioFinalText.text = "¡FINISH!";
                tiempoRestante = 0f;
                activeGame = false;

                AllRecords allRecords = AllRecords.Load();
                allRecords.GuardarPuntuacion("Boxeo", puntuacion.ToString() + " puntos");
                StartCoroutine(mp.MostrarRecords("Boxeo", " puntos"));
                return;
            }

            ActualizarTiempoTexto();
        }
    }

    // Función que lanza el evento de Hit cada tres segundos con una probabilidad del 50%
    private IEnumerator LanzarGolpes()
    {
        while (activeGame)
        {
            yield return new WaitForSeconds(3f); // espera 3 segundos

            if (!activeGame) yield break; // por si termina el juego durante la espera

            if (Random.value < 0.5f) // 50% de probabilidad
            {
                boxerAnimator.SetTrigger("Hit");
            }
        }
    }

    private void ActualizarPuntuacionTexto()
    {
        puntos.text = puntuacion.ToString() + " pts";
    }

    private void ActualizarTiempoTexto()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        tiempo.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    // Setter para la variable que indica si se han detectado las caderas
    public void setUserDetected(bool value)
    {
        userDetected = value;
    }

    // Setter para la variable que indica si se está a la distancia correcta
    public void setUserCorrectDistance(bool value)
    {
        userCorrectDistance = value;
    }

    public void setActiveGame(bool value)
    {
        inicioFinalText.gameObject.SetActive(true);
        if (value)
        {
            StartCoroutine(InicioCuentaAtras(3));
        }
        else
        {
            activeGame = value;
        }
    }

    public void IncrementarPuntuacion(int value)
    {
        puntuacion += value;
        ActualizarPuntuacionTexto();
        StartCoroutine(MostrarIncrementoPuntuacion(value));
    }

    public bool getActiveGame()
    {
        return activeGame;
    }

    private IEnumerator MostrarIncrementoPuntuacion(int value)
    {
        // Asegura que el texto sea visible y completamente opaco
        incrementoPuntos.alpha = 1f;
        incrementoPuntos.gameObject.SetActive(true);

        // Actualiza el texto y color según el valor
        incrementoPuntos.text = value > 0 ? $"+{value}" : value.ToString();
        incrementoPuntos.color = value >= 0 ? Color.green : Color.red;

        // Espera 1 segundo
        yield return new WaitForSeconds(1f);

        // Desvanecer gradualmente
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            incrementoPuntos.alpha = alpha;
            yield return null;
        }

        // Ocultar el texto por si acaso
        incrementoPuntos.gameObject.SetActive(false);
    }

    private IEnumerator InicioCuentaAtras(int num)
    {
        for (int i = num; i > 0; i--)
        {
            inicioFinalText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        inicioFinalText.text = "¡¡¡GO!!!";
        yield return new WaitForSeconds(0.5f);

        // Desvanecer gradualmente el "¡YA!"
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            inicioFinalText.alpha = alpha;
            yield return null;
        }

        inicioFinalText.gameObject.SetActive(false);
        activeGame = true;
        StartCoroutine(LanzarGolpes());
    }
}
