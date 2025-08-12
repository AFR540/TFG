using UnityEngine;
using nuitrack;
using System;
using static NuitrackSDK.UserData.SkeletonData;
using System.Net;

public class BodyMovementDetection : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform vallas;
    [SerializeField] GameObject jointPrefab;
    [SerializeField] float minDifY = 0.05f;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float impulsoMinimo = 0f;
    [SerializeField] float impulsoMaximo = 1f;
    [SerializeField] float velocidadMaxima = 1f;

    private GameManager gameManager;
    private bool cameraPositioned = false;
    private Skeleton skeleton;

    private float prevLeftKneePos, prevRightKneePos;
    private float detectionTimer = 0f;
    private float detectionCooldown = 0.25f;

    private GameObject leftKneeGO, rightKneeGO, leftAnkleGO, rightAnkleGO;
    private Rigidbody rbCameraTransform;

    void Start()
    {
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        gameManager = FindObjectOfType<GameManager>();
        rbCameraTransform = cameraTransform.GetComponent<Rigidbody>();

        if (rbCameraTransform != null)
        {
            rbCameraTransform.useGravity = false;
            rbCameraTransform.drag = 1f;
            rbCameraTransform.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }
    }

    void OnDestroy()
    {
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        if (skeletonData != null && skeletonData.Skeletons.Length > 0)
            skeleton = skeletonData.Skeletons[0];
    }

    void Update()
    {
        if (skeleton == null || rbCameraTransform == null) return;

        // Joints
        nuitrack.Joint leftAnkleJoint = skeleton.GetJoint(JointType.LeftAnkle);
        nuitrack.Joint rightAnkleJoint = skeleton.GetJoint(JointType.RightAnkle);
        UnityEngine.Vector3 leftAnklePos = ToVector3(leftAnkleJoint.Real);
        UnityEngine.Vector3 rightAnklePos = ToVector3(rightAnkleJoint.Real);

        nuitrack.Joint leftKneeJoint = skeleton.GetJoint(JointType.LeftKnee);
        nuitrack.Joint rightKneeJoint = skeleton.GetJoint(JointType.RightKnee);
        UnityEngine.Vector3 leftKneePos = ToVector3(leftKneeJoint.Real);
        UnityEngine.Vector3 rightKneePos = ToVector3(rightKneeJoint.Real);

        UpdateJoint(ref leftKneeGO, leftKneePos);
        UpdateJoint(ref rightKneeGO, rightKneePos);
        UpdateJoint(ref leftAnkleGO, leftAnklePos);
        UpdateJoint(ref rightAnkleGO, rightAnklePos);

        if (!cameraPositioned)
        {
            AdjustCameraYPosition(leftKneeJoint, rightKneeJoint);
            return;
        }

        if (!gameManager.getActiveGame()) return;

        // Diferencias de altura
        float leftKneeDifY = leftKneePos.y - prevLeftKneePos;
        float rightKneeDifY = rightKneePos.y - prevRightKneePos;

        if (IsRunning(leftKneeDifY, rightKneeDifY))
        {
            float intensidad = (Mathf.Abs(leftKneeDifY) + Mathf.Abs(rightKneeDifY)) * speedMultiplier;
            float impulso = Mathf.Clamp(intensidad, impulsoMinimo, impulsoMaximo);
            rbCameraTransform.AddForce(UnityEngine.Vector3.right * impulso, ForceMode.Impulse);
        } else if (IsJumping(leftKneeDifY, rightKneeDifY))
        {
            float impulso = Mathf.Clamp(4, impulsoMinimo, impulsoMaximo);
            rbCameraTransform.AddForce(UnityEngine.Vector3.right * impulso, ForceMode.Impulse);
        }

        // Limitar velocidad
        if (rbCameraTransform.velocity.magnitude > velocidadMaxima)
        {
            rbCameraTransform.velocity = rbCameraTransform.velocity.normalized * velocidadMaxima;
        }

        // Distancia recorrida
        gameManager.IncrementarDistancia(rbCameraTransform.velocity.magnitude * Time.deltaTime);

        // Actualiza las posiciones antiguas de rodillas cada cierto tiempo
        if (detectionTimer > detectionCooldown)
        {
            prevLeftKneePos = leftKneePos.y;
            prevRightKneePos = rightKneePos.y;
            detectionTimer = 0f;
        }
        detectionTimer += Time.deltaTime;
    }

    private bool IsRunning(float leftKneeDifY, float rightKneeDifY)
    {
        if (Mathf.Abs(leftKneeDifY) < minDifY && Mathf.Abs(rightKneeDifY) < minDifY)
            return false;

        return (leftKneeDifY > 0 && rightKneeDifY < 0) || (leftKneeDifY < 0 && rightKneeDifY > 0);
    }

    private bool IsJumping(float leftKneeDifY, float rightKneeDifY)
    {
        if (Math.Abs(leftKneeDifY) < minDifY && Math.Abs(rightKneeDifY) < minDifY)
            return false;
        if (leftKneeDifY > 0 && rightKneeDifY > 0) // Si ambas rodillas están más altas que antes
            return true;

        return false;
    }

    private void AdjustCameraYPosition(nuitrack.Joint leftAnkleJoint, nuitrack.Joint rightAnkleJoint)
    {
        if (!cameraPositioned)
        {
            gameManager.setUserDetected(false);
            gameManager.setUserCorrectDistance(false);
        }
        if (leftAnkleJoint.Confidence < 0.5f || rightAnkleJoint.Confidence < 0.5f) return;

        if (!cameraPositioned)
        {
            gameManager.setUserDetected(true);
        }

        UnityEngine.Vector3 leftAnkle = ToVector3(leftAnkleJoint.Real);
        UnityEngine.Vector3 rightAnkle = ToVector3(rightAnkleJoint.Real);

        UnityEngine.Vector3 leftAnkleLocal = Camera.main.transform.InverseTransformPoint(leftAnkle);
        UnityEngine.Vector3 rightAnkleLocal = Camera.main.transform.InverseTransformPoint(rightAnkle);

        if (leftAnkleLocal.z < 2f || rightAnkleLocal.z < 2f) return;

        if (!cameraPositioned)
        {
            gameManager.setUserCorrectDistance(true);
        }

        float minFootY = Mathf.Min(leftAnkle.y, rightAnkle.y);
        UnityEngine.Vector3 camPos = cameraTransform.position;
        camPos.y -= minFootY - 0.4f;
        cameraTransform.position = camPos;

        UnityEngine.Vector3 vallasGroup = vallas.position;
        vallasGroup.z = leftAnkle.z;
        vallasGroup.x = leftAnkle.x;
        vallas.position = vallasGroup;

        gameManager.setActiveGame(true);
        cameraPositioned = true;
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
}
