using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Instrucciones : MonoBehaviour
{
    [SerializeField] TMP_Text temporizador;
    [SerializeField] TMP_Text ultimoResultado;
    [SerializeField] bool final;
    [SerializeField] string disciplina;

    private float tiempoRestante = 20f;
    private CambiarEscena cambiarEscena;

    // Start is called before the first frame update
    void Start()
    {
        cambiarEscena = FindObjectOfType<CambiarEscena>();
        if (final)
        {
            AllRecords allRecords = AllRecords.Load();
            ultimoResultado.text = "Resultado anterior: " + allRecords.GetUltimoResultado(disciplina);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            StartCoroutine(cambiarEscena.CargarSiguienteEscena());
            return;
        }
        ActualizarTiempoTexto();
    }

    private void ActualizarTiempoTexto()
    {
        //int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        temporizador.text = segundos.ToString("00");
    }
}
