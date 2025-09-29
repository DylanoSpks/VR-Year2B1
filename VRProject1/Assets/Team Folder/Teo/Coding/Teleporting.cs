using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;       // the other portal
    public MeshRenderer screen;       // Quad surface
    public Camera portalCamera;       // portal's own camera

    private Transform playerCam;      // CenterEyeAnchor (tagged MainCamera)
    private Transform rigRoot;        // OVRCameraRig
    private float lastDot;
    private float cooldown = 0f;

    void Start()
    {
        playerCam = Camera.main.transform;    // your headset
        rigRoot = playerCam.root;             // OVRCameraRig root
        screen.material.mainTexture = linkedPortal.portalCamera.targetTexture;

        lastDot = Vector3.Dot(transform.forward, playerCam.position - transform.position);
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        UpdatePortalCamera();

        Vector3 portalToHead = playerCam.position - transform.position;
        float currentDot = Vector3.Dot(transform.forward, portalToHead);

        // crossed the plane from front -> back
        if (lastDot > 0f && currentDot <= 0f && cooldown <= 0f)
        {
            TeleportPlayer();
            cooldown = 0.3f;
        }

        lastDot = currentDot;
    }

    void UpdatePortalCamera()
    {
        if (!playerCam || !linkedPortal) return;

        // Player relative to entry portal
        Vector3 localPos = transform.InverseTransformPoint(playerCam.position);
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * playerCam.rotation;

        // Map to linked portal space
        Vector3 newWorldPos = linkedPortal.transform.TransformPoint(localPos);
        Quaternion newWorldRot = linkedPortal.transform.rotation * localRot;

        // Apply to this portal's camera
        portalCamera.transform.SetPositionAndRotation(newWorldPos, newWorldRot);
    }

    void TeleportPlayer()
    {
        // Headset position relative to entry portal
        Vector3 localHeadPos = transform.InverseTransformPoint(playerCam.position);
        Quaternion localHeadRot = Quaternion.Inverse(transform.rotation) * playerCam.rotation;

        // Where that headset pos/rot should be relative to the exit portal
        Vector3 newHeadPos = linkedPortal.transform.TransformPoint(localHeadPos);
        Quaternion newHeadRot = linkedPortal.transform.rotation * localHeadRot;

        // Offset between rig root and headset before teleport
        Vector3 rigToHead = playerCam.position - rigRoot.position;

        // Place rig root so headset lines up at the new position
        rigRoot.position = newHeadPos - rigToHead;

        // Rotate rig root so headset facing matches new facing
        Quaternion rotDiff = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation);
        rigRoot.rotation = rotDiff * rigRoot.rotation;
    }
}
