using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PiraguaGameManager : MonoBehaviour
{
    [SerializeField] TMP_Text distanciaTexto;
    [SerializeField] TMP_Text tiempo;
    [SerializeField] TMP_Text inicioFinalText;
    [SerializeField] GameObject deteccionUsuario;
    [SerializeField] GameObject deteccionDistanciaUsuario;

    MostrarPuntuaciones mp;
    private float tiempoRestante = 30f;
    private float distancia = 0f;
    private bool activeGame = false;
    private bool userDetected = false;
    private bool userCorrectDistance = false;

    // Start is called before the first frame update
    void Start()
    {
        ActualizarDistanciaTexto();
        ActualizarTiempoTexto();
        mp = GetComponent<MostrarPuntuaciones>();
        inicioFinalText.gameObject.SetActive(false);
        tiempo.gameObject.SetActive(false);
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
            tiempo.gameObject.SetActive(true);
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0f)
            {
                inicioFinalText.gameObject.SetActive(true);
                inicioFinalText.alpha = 1;
                inicioFinalText.text = "¡FINISH!";
                tiempoRestante = 0f;
                activeGame = false;
                AllRecords allRecords = AllRecords.Load();
                allRecords.GuardarPuntuacion("Piragua", distancia.ToString() + " m");
                StartCoroutine(mp.MostrarRecords("Piragua", "m"));

                StartCoroutine(CargarSiguienteEscena(15f));
            }

            ActualizarTiempoTexto();
        }
    }

    private void ActualizarDistanciaTexto()
    {
        distanciaTexto.text = distancia.ToString() + " m";
    }

    private void ActualizarTiempoTexto()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        tiempo.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    private IEnumerator CargarSiguienteEscena(float delay = 10f)
    {
        yield return new WaitForSeconds(delay);

        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(escenaActual + 1);
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
        }
        else
        {
            activeGame = value;
        }
    }

    // método para incrementar la distancia desde otros scripts
    public void IncrementarDistancia(float cantidad)
    {
        distancia += cantidad;
        distancia = Mathf.Round(distancia * 100f) / 100f;
        ActualizarDistanciaTexto();
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
