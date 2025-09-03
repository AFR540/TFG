using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VallaColiderDetector : MonoBehaviour
{
    private bool colisionado = false; // Variable entera que se usará para lanzar únicamente la animación una vez
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        colisionado = false;
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
            if (colisionado) return;
            gameManager.Penalizar();
            colisionado = true;
        }
    }
}
