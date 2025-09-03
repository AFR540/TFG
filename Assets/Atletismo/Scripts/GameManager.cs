using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text distancia;
    [SerializeField] TMP_Text tiempo;
    [SerializeField] TMP_Text penalizacionText;
    [SerializeField] TMP_Text inicioFinalText;
    [SerializeField] GameObject deteccionUsuario;
    [SerializeField] GameObject deteccionDistanciaUsuario;
    [SerializeField] string nombrePrueba = "Atletismo";

    MostrarPuntuaciones mp;
    private CambiarEscena cambiarEscena;
    private float tiempoRestante = 30f;
    private float distanciaActual = 0f;
    private bool activeGame = false;
    private bool userDetected = false;
    private bool userCorrectDistance = false;

    // Start is called before the first frame update
    void Start()
    {
        ActualizarDistanciaTexto();
        ActualizarTiempoTexto();
        mp = GetComponent<MostrarPuntuaciones>();
        cambiarEscena = FindObjectOfType<CambiarEscena>();
        inicioFinalText.gameObject.SetActive(false);
        penalizacionText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Si no se detectan las rodillas, damos feedback al usuario
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
                allRecords.GuardarPuntuacion(nombrePrueba, distanciaActual.ToString() + " m");
                StartCoroutine(mp.MostrarRecords("Atletismo", "m"));

                StartCoroutine(cambiarEscena.CargarSiguienteEscena(10f));
            }

            ActualizarTiempoTexto();
        }
        
    }

    private void ActualizarDistanciaTexto()
    {
        distancia.text = distanciaActual.ToString() + " m";
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


    // método getter para obtener eñ estado de active game desde otros scripts
    public bool getActiveGame()
    {
        return activeGame;
    }

    // método setter para actualiazr el valor de active game
    public void setActiveGame(bool value)
    {
        inicioFinalText.gameObject.SetActive(true);
        if (value)
        {
            StartCoroutine(InicioCuentaAtras(3));
        } else
        {
            activeGame = value;
        }
    }

    // método para incrementar la distancia desde otros scripts
    public void IncrementarDistancia(float cantidad)
    {
        distanciaActual += (cantidad * 2);
        distanciaActual = Mathf.Round(distanciaActual * 100f) / 100f;
        ActualizarDistanciaTexto();
    }

    public void Penalizar()
    {
        distanciaActual -= 5;
        ActualizarDistanciaTexto();
        StartCoroutine(MostrarPenalizacion());
    }

    private IEnumerator MostrarPenalizacion()
    {
        // Asegura que el texto sea visible y completamente opaco
        penalizacionText.alpha = 1f;
        penalizacionText.gameObject.SetActive(true);

        // Espera 1 segundo
        yield return new WaitForSeconds(1f);

        // Desvanecer gradualmente
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            penalizacionText.alpha = alpha;
            yield return null;
        }

        // Ocultar el texto por si acaso
        penalizacionText.gameObject.SetActive(false);
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
    }
}
