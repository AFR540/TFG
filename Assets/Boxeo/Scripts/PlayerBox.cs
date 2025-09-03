using nuitrack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBox : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform boxEnemy;
    [SerializeField] GameObject jointPrefab;

    private BoxGameManager gameManager;
    private Skeleton skeleton;
    private bool boxerPositioned;
    private GameObject headGO, neckGO, rightHandGO, leftHandGO, rightElbowGO, leftElbowGO, rightShoulderGO, leftShoulderGO, rightAnkleGO, leftAnkleGO;


    // Start is called before the first frame update
    void Start()
    {
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        gameManager = FindObjectOfType<BoxGameManager>();
        boxerPositioned = false;
    }

    void OnDestroy()
    {
        if (NuitrackManager.Instance != null &&
            NuitrackManager.sensorsData != null &&
            NuitrackManager.sensorsData[0].SkeletonTracker != null)
        {
            NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (skeleton == null) return;

        // Detección de los tobillos del jugador
        nuitrack.Joint leftAnkleJoint = skeleton.GetJoint(JointType.LeftAnkle);
        nuitrack.Joint rightAnkleJoint = skeleton.GetJoint(JointType.RightAnkle);

        if (!boxerPositioned)
        {
            PositionBoxEnemy(leftAnkleJoint, rightAnkleJoint);
            return;
        }

        // Pasamos a Vector3 los Joint
        UnityEngine.Vector3 leftAnklePos = ToVector3(leftAnkleJoint.Real);
        UnityEngine.Vector3 rightAnklePos = ToVector3(rightAnkleJoint.Real);

        // Posición de cabeza, cuello, hombros, codos y manos 
        UnityEngine.Vector3 headPos = ToVector3(skeleton.GetJoint(JointType.Head).Real);
        UnityEngine.Vector3 neckPos = ToVector3(skeleton.GetJoint(JointType.Neck).Real);
        UnityEngine.Vector3 rightHandPos = ToVector3(skeleton.GetJoint(JointType.RightHand).Real);
        UnityEngine.Vector3 leftHandPos = ToVector3(skeleton.GetJoint(JointType.LeftHand).Real);
        UnityEngine.Vector3 rightElbowPos = ToVector3(skeleton.GetJoint(JointType.RightElbow).Real);
        UnityEngine.Vector3 leftElbowPos = ToVector3(skeleton.GetJoint(JointType.LeftElbow).Real);
        UnityEngine.Vector3 rightShoulderPos = ToVector3(skeleton.GetJoint(JointType.RightShoulder).Real);
        UnityEngine.Vector3 leftShoulderPos = ToVector3(skeleton.GetJoint(JointType.LeftShoulder).Real);

        UpdateJoint(ref headGO, headPos);
        UpdateJoint(ref neckGO, neckPos);
        UpdateJoint(ref rightHandGO, rightHandPos);
        UpdateJoint(ref leftHandGO, leftHandPos);
        UpdateJoint(ref rightElbowGO, rightElbowPos);
        UpdateJoint(ref leftElbowGO, leftElbowPos);
        UpdateJoint(ref rightShoulderGO, rightShoulderPos);
        UpdateJoint(ref leftShoulderGO, leftShoulderPos);
        UpdateJoint(ref rightAnkleGO, rightAnklePos);
        UpdateJoint(ref leftAnkleGO, leftAnklePos);
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        if (skeletonData != null && skeletonData.Skeletons.Length > 0)
        {
            skeleton = skeletonData.Skeletons[0];
        }
    }

    private void PositionBoxEnemy(nuitrack.Joint leftAnkleJoint, nuitrack.Joint rightAnkleJoint)
    {
        if (!boxerPositioned)
        {
            gameManager.setUserDetected(false);
            gameManager.setUserCorrectDistance(false);
        }
        // Verifica que ambos pies se han detectado con na confianza del 50%
        if (leftAnkleJoint.Confidence < 0.5f || rightAnkleJoint.Confidence < 0.5f)
        {
            return; // Si no están bien detectados, salta este frame
        }

        if (!boxerPositioned)
        {
            gameManager.setUserDetected(true);
        }

        // Posición de los tobillos del jugador
        UnityEngine.Vector3 leftAnkle = ToVector3(leftAnkleJoint.Real);
        UnityEngine.Vector3 rightAnkle = ToVector3(rightAnkleJoint.Real);

        // Movemos al boxeador enemigo la posición del pie más bajo en los ejes z e y
        UnityEngine.Vector3 minFoot = leftAnkle.y < rightAnkle.y ? leftAnkle : rightAnkle;

        UnityEngine.Vector3 minFootLocal = Camera.main.transform.InverseTransformPoint(minFoot);
        if (minFootLocal.z < 2f) return;

        if (!boxerPositioned)
        {
            gameManager.setUserCorrectDistance(true);
        }

        UnityEngine.Vector3 boxerPos = boxEnemy.position;

        boxerPos.y = minFoot.y;
        boxerPos.z = minFoot.z;
        boxEnemy.position = boxerPos;

        boxerPositioned = true;
        gameManager.setActiveGame(true);
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

    UnityEngine.Vector3 ToVector3(nuitrack.Vector3 v)
    {
        return new UnityEngine.Vector3(v.X / 1000f, v.Y / 1000f, v.Z / 1000f);
    }
}
