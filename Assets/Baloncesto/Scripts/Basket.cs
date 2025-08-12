using nuitrack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static nuitrack.SkeletonTracker;

public class Basket : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform floor;
    [SerializeField] GameObject jointPrefab;
    [SerializeField] VisualEffect smoke;

    private Skeleton skeleton;
    private GameObject rightHandGO, leftHandGO, headGo;
    private BasketGameManager gameManager;
    private bool activeGame;


    // Start is called before the first frame update
    void Start()
    {
        smoke.SendEvent("OnExit");
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        gameManager = FindObjectOfType<BasketGameManager>();
        if (!gameManager.getActiveGame()) gameManager.setActiveGame(false);
    }

    void OnDestroy()
    {
        NuitrackManager.sensorsData[0].SkeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        if (skeletonData != null && skeletonData.Skeletons.Length > 0)
        {
            skeleton = skeletonData.Skeletons[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (skeleton == null) return;

        // Posición de manos y cabeza
        UnityEngine.Vector3 rightHandPos = ToVector3(skeleton.GetJoint(JointType.RightHand).Real);
        UnityEngine.Vector3 leftHandPos = ToVector3(skeleton.GetJoint(JointType.LeftHand).Real);
        UnityEngine.Vector3 headPos = ToVector3(skeleton.GetJoint(JointType.Head).Real);

        UpdateJoint(ref rightHandGO, rightHandPos);
        UpdateJoint(ref leftHandGO, leftHandPos); 
        UpdateJoint(ref headGo, headPos);

        if (!activeGame)
        {
            activeGame = CheckPlayerCameraDistance(headGo, 2f);
            gameManager.setActiveGame(activeGame);
            return;
        }

        gameManager.setUserDetected(true);
        gameManager.setUserCorrectDistance(true);

        // Cambiamos los tags de las manos y cabeza
        rightHandGO.gameObject.tag = "Hand";
        leftHandGO.gameObject.tag = "Hand";
        headGo.gameObject.tag = "Head";
    }

    private bool CheckPlayerCameraDistance(GameObject joint, float minDist)
    {
        if (joint == null) return false;

        float jointZ = joint.transform.position.z;
        return jointZ >= minDist;
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
