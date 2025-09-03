using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BallController : MonoBehaviour
{
    [SerializeField] Transform ring;
    [SerializeField] float desviacionMax = 0f;
    [SerializeField] VisualEffect smoke;

    private Rigidbody rb;
    private bool pelotaCogida = false;
    private Transform mano;
    private Transform cabeza;
    private bool cabezaDetectada = false;
    private BasketGameManager gameManager;
    private bool shotMaded = false;

    // Variable estática compartida por todas las pelotas
    private static bool algunaPelotaCogida = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<BasketGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Buscar la cabeza si aún no está detectada
        if (!cabezaDetectada)
        {
            GameObject cabezaGO = GameObject.FindGameObjectWithTag("Head");
            if (cabezaGO != null)
            {
                cabeza = cabezaGO.transform;
                cabezaDetectada = true;
            }
            else
            {
                return; // No hacer nada aún
            }
        }

        if (pelotaCogida && mano != null)
        {
            if (mano.position.y > cabeza.position.y)
            {
                LanzarPelota();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager.getActiveGame())
        {
            if (other.CompareTag("Hand") && !pelotaCogida && !algunaPelotaCogida)
            {
                mano = other.transform;
                pelotaCogida = true;
                transform.SetParent(mano);
                algunaPelotaCogida = true; // Bloqueamos otras pelotas


                // Desactivamos físicas mientras está cogida
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

        }
        // Asignado el tag Valla al objeto dentro de la canasta que detecta cuando se encesta
        if (other.CompareTag("Valla") && !shotMaded)
        {
            shotMaded = true;
            PlaySmoke();
            gameManager.IncrementarPuntuacion();
        }
    }

    void LanzarPelota()
    {
        pelotaCogida = false;
        algunaPelotaCogida = false;

        transform.SetParent(null);
        mano = null;

        rb.isKinematic = false;

        Vector3 velocidadInicial = CalcularVelocidadParabolica(rb.position, ring.position, 2.1f);

        rb.velocity = velocidadInicial;
    }

    Vector3 CalcularVelocidadParabolica(Vector3 origen, Vector3 destino, float alturaMax)
    {
        float gravedad = Mathf.Abs(Physics.gravity.y);

        Vector3 planarDestino = new Vector3(destino.x, 0, destino.z);
        Vector3 planarOrigen = new Vector3(origen.x, 0, origen.z);
        Vector3 planarDir = (planarDestino - planarOrigen);
        float distanciaPlano = planarDir.magnitude;
        planarDir.Normalize();

        // Añadir desviación horizontal (pequeño ángulo aleatorio)
        Vector3 desviacion = Vector3.Cross(Vector3.up, planarDir) * Random.Range(-desviacionMax, desviacionMax);
        Vector3 direccionConDesviacion = (planarDir + desviacion).normalized;

        float tiempoSubida = Mathf.Sqrt(2 * (alturaMax - origen.y) / gravedad);
        float tiempoBajada = Mathf.Sqrt(2 * (alturaMax - destino.y) / gravedad);
        float tiempoTotal = tiempoSubida + tiempoBajada;

        // Velocidad vertical inicial
        float velocidadVertical = Mathf.Sqrt(2 * gravedad * (alturaMax - origen.y));

        // Velocidad horizontal
        float velocidadHorizontal = distanciaPlano / tiempoTotal;

        Vector3 velocidad = direccionConDesviacion * velocidadHorizontal;
        velocidad.y = velocidadVertical;

        return velocidad;
    }

    void PlaySmoke()
    {
        // Emitir evento para activar el humo
        smoke.SendEvent("OnEnter");
        Debug.Log("Empieza humo");

        // Detener el humo después de 0.5 segundos
        Invoke(nameof(StopSmoke), 0.5f);
    }

    void StopSmoke()
    {
        smoke.SendEvent("OnExit");
        Debug.Log("Acaba Humo");
    }
}
