using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boyas : MonoBehaviour
{
    [SerializeField] Transform rio;

    public float fuerzaFlotacion = 60f;
    public float damping = 0.5f;
    private Rigidbody rb;

    private static Vector3 vientoActual = Vector3.zero;
    private static float tiempoProximoCambio = 0f;
    private static float intervaloViento = 5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        AplicarFlotacion();
        AplicarVientoCompartido();
    }

    void AplicarFlotacion()
    {
        float diferenciaAltura = rio.transform.localPosition.y - transform.position.y + 0.4f;

        if (diferenciaAltura > 0)
        {
            float fuerza = fuerzaFlotacion * diferenciaAltura - rb.velocity.y * damping;
            rb.AddForce(Vector3.up * fuerza, ForceMode.Force);
        }
    }

    void AplicarVientoCompartido()
    {
        // Cambio de viento cada intervalo
        if (Time.time >= tiempoProximoCambio)
        {
            tiempoProximoCambio = Time.time + intervaloViento;

            Vector2 viento2D = Random.insideUnitCircle.normalized;
            float fuerza = Random.Range(0f, 1f);

            vientoActual = new Vector3(viento2D.x, 0, viento2D.y) * fuerza;
        }

        // Torque aplicado por el viento
        Vector3 torqueViento = Vector3.Cross(Vector3.up, vientoActual) * 0.5f;
        rb.AddTorque(torqueViento, ForceMode.Force);

        // Limitar rotación con torque restaurador estilo resorte
        Vector3 euler = transform.localEulerAngles;
        euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
        euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;

        float maxAngulo = 15f;
        float stiffness = 5f;  // fuerza del resorte
        float dampingTorque = 5f;  // fricción para amortiguar el balanceo

        // Torque restaurador para X
        if (euler.x > maxAngulo)
            rb.AddTorque(-transform.right * stiffness * (euler.x - maxAngulo), ForceMode.Force);
        else if (euler.x < -maxAngulo)
            rb.AddTorque(-transform.right * stiffness * (euler.x + maxAngulo), ForceMode.Force);

        // Torque restaurador para Z
        if (euler.z > maxAngulo)
            rb.AddTorque(-transform.forward * stiffness * (euler.z - maxAngulo), ForceMode.Force);
        else if (euler.z < -maxAngulo)
            rb.AddTorque(-transform.forward * stiffness * (euler.z + maxAngulo), ForceMode.Force);

        // Torque amortiguador para evitar oscilaciones infinitas
        Vector3 angularVel = rb.angularVelocity;
        Vector3 dampingForce = new Vector3(angularVel.x * dampingTorque, 0, angularVel.z * dampingTorque);
        rb.AddTorque(-dampingForce, ForceMode.Force);
    }
}
