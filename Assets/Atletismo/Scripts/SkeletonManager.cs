using UnityEngine;
using System.Collections.Generic;
using System;


namespace NuitrackSDK.Tutorials.ARNuitrack.Extensions
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/Extensions/Rigidbody Skeleton Manager")]
    public class SkeletonManager : MonoBehaviour
    {
        [SerializeField] GameObject rigidBodySkeletonPrefab;
        [SerializeField] Transform space;

        Dictionary<int, SkeletonController> skeletons = new Dictionary<int, SkeletonController>();

        void Update()
        {
            foreach (UserData user in NuitrackManager.sensorsData[0].Users)
                if (!skeletons.ContainsKey(user.ID))
                {
                    SkeletonController rigidbodySkeleton = Instantiate(rigidBodySkeletonPrefab, space).GetComponent<SkeletonController>();
                    rigidbodySkeleton.UserID = user.ID;
                    rigidbodySkeleton.SetSpace(space);

                    skeletons.Add(user.ID, rigidbodySkeleton);
                }

            foreach (int skeletonID in new List<int>(skeletons.Keys))
                if (NuitrackManager.sensorsData[0].Users.GetUser(skeletonID) == null)
                {
                    Destroy(skeletons[skeletonID].gameObject);
                    skeletons.Remove(skeletonID);
                }
        }
    }
}