using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MostrarPuntuaciones : MonoBehaviour
{
    [SerializeField] GameObject cameraDisplay;
    [SerializeField] GameObject canvas;
    [SerializeField] TMP_Text pos1;
    [SerializeField] TMP_Text pos2;
    [SerializeField] TMP_Text pos3;
    [SerializeField] TMP_Text pos4;
    [SerializeField] TMP_Text pos5;
    [SerializeField] TMP_Text nombre1;
    [SerializeField] TMP_Text nombre2;
    [SerializeField] TMP_Text nombre3;
    [SerializeField] TMP_Text nombre4;
    [SerializeField] TMP_Text nombre5;
    [SerializeField] TMP_Text puntuacion1;
    [SerializeField] TMP_Text puntuacion2;
    [SerializeField] TMP_Text puntuacion3;
    [SerializeField] TMP_Text puntuacion4;
    [SerializeField] TMP_Text puntuacion5;

    private TMP_Text[] nombres;
    private TMP_Text[] puntuaciones;
    private TMP_Text[] posiciones;

    // Start is called before the first frame update
    void Start()
    {
        canvas.SetActive(false);

        nombres = new TMP_Text[] { nombre1, nombre2, nombre3, nombre4, nombre5 };
        puntuaciones = new TMP_Text[] { puntuacion1, puntuacion2, puntuacion3, puntuacion4, puntuacion5 };
        posiciones = new TMP_Text[] {pos1, pos2, pos3, pos4, pos5 };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator MostrarRecords(string modalidad, string unidadPuntuacion)
    {
        yield return new WaitForSeconds(2f);

        canvas.SetActive(true);
        cameraDisplay.SetActive(false);
        CargarRecords(modalidad, unidadPuntuacion);
    }

    private void CargarRecords(string modalidad, string unidadPuntuacion)
    {
        AllRecords allRecords = AllRecords.Load();
        List<(string nombre, string puntuacion)> top5 = allRecords.GetTop5Scores(modalidad, unidadPuntuacion);

        for (int i = 0; i < top5.Count; i++)
        {
            var (nombre, puntuacion) = top5[i];

            if (puntuacion != "No puntuado")
            {
                nombres[i].text = nombre;
                puntuaciones[i].text = puntuacion;
                nombres[i].gameObject.SetActive(true);
                puntuaciones[i].gameObject.SetActive(true);
                posiciones[i].gameObject.SetActive(true);
            } else
            {
                nombres[i].gameObject.SetActive(false);
                puntuaciones[i].gameObject.SetActive(false);
                posiciones[i].gameObject.SetActive(false);
            } 
        }
    }
}
