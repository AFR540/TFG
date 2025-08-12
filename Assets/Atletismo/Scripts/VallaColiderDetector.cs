using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VallaColiderDetector : MonoBehaviour
{
    private int contador; // Variable entera que se usar� para lanzar �nicamente la animaci�n una vez
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        contador = 0;
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (contador > 0) return;
            gameManager.Penalizar();
            contador++;
        }
    }
}
