using nuitrack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piragua : MonoBehaviour
{
    [SerializeField] Transform piragua;
    [SerializeField] Transform remo;
    [SerializeField] GameObject jointPrefab;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform river;

    //private bool piraguaColocada;
    private PiraguaGameManager gameManager;
    private Skeleton skeleton;
    private GameObject rightElbowGO, leftElbowGO, leftHipGo, rightHipGo;
    private Rigidbody rbCameraTransform;
    private float impulsoMaximo = 2f;
    private float impulsoMinimo = 0.2f;
    private float velocidadMaxima = 5f;
    private bool piraguaColocada = false;

    private UnityEngine.Vector3 posicionAnterior;


    // Start is called before the first frame update
    void Start()
    {
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        gameManager = FindAnyObjectByType<PiraguaGameManager>();
        rbCameraTransform = cameraTransform.GetComponent<Rigidbody>();
        if (rbCameraTransform != null) rbCameraTransform.drag = 1f; // Fricción o rozamiento con el agua

        posicionAnterior = cameraTransform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (skeleton == null) return;

        // Detección de las caderas y manos del jugador, donde colocaremos la piragua
        nuitrack.Joint leftHipJoint = skeleton.GetJoint(JointType.LeftHip);
        nuitrack.Joint rightHipJoint = skeleton.GetJoint(JointType.RightHip);
        nuitrack.Joint leftHandJoint = skeleton.GetJoint(JointType.LeftHand);
        nuitrack.Joint rightHandJoint = skeleton.GetJoint(JointType.RightHand);

        // Posición de los codos
        UnityEngine.Vector3 rightElbowPos = ToVector3(skeleton.GetJoint(JointType.RightElbow).Real);
        UnityEngine.Vector3 leftElbowPos = ToVector3(skeleton.GetJoint(JointType.LeftElbow).Real);

        // Pasamos a Vector3 los Joint
        UnityEngine.Vector3 leftHipPos = ToVector3(leftHipJoint.Real);
        UnityEngine.Vector3 rightHipPos = ToVector3(rightHipJoint.Real);

        UpdateJoint(ref leftHipGo, leftHipPos);
        UpdateJoint(ref rightHipGo, rightHipPos);
        UpdateJoint(ref rightElbowGO, rightElbowPos);
        UpdateJoint(ref leftElbowGO, leftElbowPos);

        // Métodos para posicionar la piragua y el remo en cada frame
        PositionPiragua(leftHipJoint, rightHipJoint);
        UpdateRemoPosition(leftHandJoint, rightHandJoint);

        // Si no ha empezado el juego saltamos al siguiente frame
        if (!gameManager.getActiveGame()) return;


        float distanciaRecorrida = UnityEngine.Vector3.Distance(posicionAnterior, cameraTransform.position);
        if (distanciaRecorrida > 0f)
        {
            gameManager.IncrementarDistancia(distanciaRecorrida);
        }
        posicionAnterior = cameraTransform.position;

        // Método para saber si esl jugador está remando y darle velocidad según la remada
        if (HadPaddle()) Paddle();
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        if (skeletonData != null && skeletonData.Skeletons.Length > 0)
        {
            skeleton = skeletonData.Skeletons[0];
        }
    }

    private void PositionPiragua(nuitrack.Joint leftHipJoint, nuitrack.Joint rightHipJoint)
    {
        if (!piraguaColocada)
        {
            gameManager.setUserDetected(false);
            gameManager.setUserCorrectDistance(false);
        }
        // Verificamos la detección de ambas caderas
        if (leftHipJoint.Confidence < 0.5f || rightHipJoint.Confidence < 0.5f) return;

        if (!piraguaColocada)
        {
            gameManager.setUserDetected(true);
        }

        // Posición de las caderas del jugador
        UnityEngine.Vector3 leftHip = ToVector3(leftHipJoint.Real);
        UnityEngine.Vector3 rightHip = ToVector3(rightHipJoint.Real);

        // Ppunto medio entre las dos caderas
        UnityEngine.Vector3 midPoint = (leftHip + rightHip) / 2f;
        midPoint.y += 0.1f;

        // Convertir el punto al espacio local de la cámara (para usar localPosition.z)
        UnityEngine.Vector3 midPointLocal = Camera.main.transform.InverseTransformPoint(midPoint);
        if (midPointLocal.z < 2.0f && !gameManager.getActiveGame()) return; // Está muy cerca de la cámara

        if (!piraguaColocada)
        {
            gameManager.setUserCorrectDistance(true);
        }

        // Posicionar la piragua en ese punto
        piragua.localPosition = midPoint;

        // Posicionar el río 
        UnityEngine.Vector3 riverPos = river.localPosition;
        riverPos.y = midPoint.y - 0.1f;
        river.localPosition = riverPos;

        if (!piraguaColocada) gameManager.setActiveGame(true);
        piraguaColocada = true;
    }

    private void UpdateRemoPosition(nuitrack.Joint leftHandJoint, nuitrack.Joint rightHandJoint)
    {
        // Convertimos las posiciones de las manos a Vector3
        UnityEngine.Vector3 leftHand = ToVector3(leftHandJoint.Real);
        UnityEngine.Vector3 rightHand = ToVector3(rightHandJoint.Real);

        // Posición media entre las manos
        UnityEngine.Vector3 middlePoint = (leftHand + rightHand) / 2f;

        // Posicionamos el remo en el punto medio
        remo.localPosition = middlePoint;

        // Orientamos el remo para que apunte de una mano a la otra
        UnityEngine.Vector3 direction = rightHand - leftHand;

        if (direction != UnityEngine.Vector3.zero)
            remo.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
    }

    private void UpdateJoint(ref GameObject joint, UnityEngine.Vector3 position)
    {
        if (joint == null)
        {
            joint = Instantiate(jointPrefab, position, Quaternion.identity, cameraTransform);
        }
        else
        {
            joint.transform.localPosition = position;
        }
    }

    private UnityEngine.Vector3 ToVector3(nuitrack.Vector3 v)
    {
        return new UnityEngine.Vector3(v.X / 1000f, v.Y / 1000f, v.Z / 1000f);
    }

    // Función para determinar si el usuario ha remado según la posición de los codos y caderas
    private bool HadPaddle()
    {
        if (rightElbowGO.transform.localPosition.x < rightHipGo.transform.localPosition.x && leftElbowGO.transform.localPosition.x < leftHipGo.transform.localPosition.x ||
            rightElbowGO.transform.localPosition.x > rightHipGo.transform.localPosition.x && leftElbowGO.transform.localPosition.x > leftHipGo.transform.localPosition.x)
        {
            return true;
        }

        return false;
    }

    // Función que impulsa al jugador como si remara
    private void Paddle()
    {
        if (rbCameraTransform == null) return;

        // Distancia relativa de los codos respecto a las caderas (en eje X)
        float distanciaDerecha = Mathf.Abs(rightElbowGO.transform.localPosition.x - rightHipGo.transform.localPosition.x);
        float distanciaIzquierda = Mathf.Abs(leftElbowGO.transform.localPosition.x - leftHipGo.transform.localPosition.x);

        // Media de ambas distancias
        float fuerzaRemada = (distanciaDerecha + distanciaIzquierda) / 2f;

        // Normaliza fuerza dentro de un rango
        float impulso = Mathf.Clamp(fuerzaRemada * 5f, impulsoMinimo, impulsoMaximo); 

        UnityEngine.Vector3 direccion = -cameraTransform.forward;
        rbCameraTransform.AddForce(direccion * impulso, ForceMode.Impulse);

        // Limita la velocidad máxima
        if (rbCameraTransform.velocity.magnitude > velocidadMaxima)
        {
            rbCameraTransform.velocity = rbCameraTransform.velocity.normalized * velocidadMaxima;
        }
    }
}
