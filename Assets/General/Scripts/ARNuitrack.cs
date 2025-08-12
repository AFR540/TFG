using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using nuitrack;
using NuitrackSDK.Frame;

public class ARNuitrack : MonoBehaviour
{
    ulong lastTimestamp;

    Texture2D rgbTexture2d;

    [SerializeField] MeshGenerator meshGenerator;
    [SerializeField] new Camera camera;

    ComputeBuffer depthDataBuffer;
    byte[] depthDataArray = null;

    [SerializeField] float maxDepthSensor = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NuitrackManager.sensorsData[0] == null) return; 

        nuitrack.ColorFrame colorFrame = NuitrackManager.sensorsData[0].ColorFrame;
        nuitrack.DepthFrame depthFrame = NuitrackManager.sensorsData[0].DepthFrame;
        nuitrack.UserFrame userFrame = NuitrackManager.sensorsData[0].UserFrame;

        if (colorFrame == null || depthFrame == null || userFrame == null || colorFrame.Timestamp == lastTimestamp)
        {
            return;
        }
        lastTimestamp = colorFrame.Timestamp;

        UpdateRGB(colorFrame, userFrame);
        UpdateHeightMap(depthFrame);
    }

    void UpdateRGB(nuitrack.ColorFrame frame, nuitrack.UserFrame mask)
    {
        if (rgbTexture2d == null)
        {
            meshGenerator.Generate(frame.Cols, frame.Rows);

            rgbTexture2d = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            meshGenerator.Material.SetTexture("_MainTex", rgbTexture2d);
        }

        rgbTexture2d.LoadRawTextureData(frame.Data, frame.DataSize);
        rgbTexture2d.Apply();

        if (mask != null)
        {
            Texture2D maskTexture = mask.ToTexture2D();
            meshGenerator.Material.SetTexture("_UserMask", maskTexture);
        }

        fitMeshIntoFrame(frame);
    }

    void UpdateHeightMap (nuitrack.DepthFrame frame)
    {
        nuitrack.OutputMode outputMode = NuitrackManager.sensorsData[0].DepthSensor.GetOutputMode();
        float vFOV = outputMode.HFOV * ((float)frame.Rows / frame.Cols);

        camera.fieldOfView = vFOV * Mathf.Rad2Deg;

        if (depthDataBuffer == null)
        {
            depthDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
            meshGenerator.Material.SetBuffer("_DepthFrame", depthDataBuffer);

            meshGenerator.Material.SetInt("_textureWidth", frame.Cols);
            meshGenerator.Material.SetInt("_textureHeight", frame.Rows);

            depthDataArray = new byte[frame.DataSize];
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        depthDataBuffer.SetData(depthDataArray);

        meshGenerator.Material.SetFloat("_maxDepthSensor", maxDepthSensor);
        meshGenerator.transform.localPosition = UnityEngine.Vector3.forward * maxDepthSensor;

        UnityEngine.Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(camera.transform.position);
        meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);
    }

    void fitMeshIntoFrame(nuitrack.ColorFrame frame)
    {
        float frameAspectRatio = (float)frame.Cols / frame.Rows;
        float targetAspectRatio = frameAspectRatio > camera.aspect ? camera.aspect : frameAspectRatio;

        float v_angle = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float scale = UnityEngine.Vector3.Distance(meshGenerator.transform.position, camera.transform.position) * Mathf.Tan(v_angle) * targetAspectRatio;

        meshGenerator.transform.localScale = new UnityEngine.Vector3(scale * 2, scale * 2, 1);  
    }
}
