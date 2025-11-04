using System.Collections;
using UnityEngine;
using TMPro;

public class FirstTimePickupUIManager : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject uiRoot;              // assign your Panel (child of Canvas)
    [SerializeField] private TextMeshProUGUI dialogueText;   // assign TMP text under the Panel
    [SerializeField] private float showSeconds = 3f;

    [Header("Head-Lock Follow")]
    [SerializeField] private bool followHead = true;
    [SerializeField] private float followDistance = 1.2f;
    [SerializeField] private float verticalOffset = -0.05f;
    [SerializeField] private float followLerp = 12f;
    [SerializeField] private float rotateLerp = 20f;
    [SerializeField] private float minY = 0.2f;

    [Header("Camera")]
    [SerializeField] private Camera overrideCamera;          // drag your HMD camera here for Quest builds

    public static FirstTimePickupUIManager Instance { get; private set; }

    private Transform cam;
    private bool isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var camRef = overrideCamera != null ? overrideCamera : Camera.main;
        if (camRef == null)
        {
#if UNITY_6000_0_OR_NEWER
            camRef = FindAnyObjectByType<Camera>();
#else
            var cams = FindObjectsOfType<Camera>();
            if (cams.Length > 0) camRef = cams[0];
#endif
        }
        cam = camRef != null ? camRef.transform : null;

        if (uiRoot != null) uiRoot.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
    }

#if !UNITY_EDITOR
    private IEnumerator Start()
    {
        // Device probe: force-show once after boot to verify wiring on headset
        yield return new WaitForSeconds(1f);

        Debug.Log($"[FTPUIMgr] Device probe | uiRoot={(uiRoot?uiRoot.name:"NULL")} text={(dialogueText?dialogueText.name:"NULL")} cam={(cam?cam.name:"NULL")}");

        if (uiRoot != null)
        {
            uiRoot.SetActive(true);
            if (dialogueText != null) dialogueText.text = "DEVICE PROBE: UI wired.";
            yield return new WaitForSeconds(1.5f);
            uiRoot.SetActive(false);
            if (dialogueText != null) dialogueText.text = "";
        }
        else
        {
            // Fallback visual marker so you know code ran even if UI refs are missing
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.position = (cam ? cam.position + cam.forward * 1.0f : Vector3.zero + Vector3.forward);
            marker.transform.localScale = Vector3.one * 0.05f;
        }
    }
#endif

    public void ShowDialogue(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (dialogueText != null) dialogueText.text = message;

        if (followHead && cam != null)
        {
            Vector3 flatFwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 target = cam.position + flatFwd * followDistance;
            target.y = Mathf.Max(target.y + verticalOffset, minY);
            transform.position = target;
            transform.rotation = Quaternion.LookRotation((transform.position - cam.position).normalized, Vector3.up);
        }

        if (uiRoot != null) uiRoot.SetActive(true);
        isShowing = true;

        yield return new WaitForSeconds(showSeconds);

        isShowing = false;
        if (uiRoot != null) uiRoot.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
    }

    private void LateUpdate()
    {
        if (!followHead || !isShowing || cam == null) return;

        Vector3 flatFwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 desiredPos = cam.position + flatFwd * followDistance;
        desiredPos.y = Mathf.Max(desiredPos.y + verticalOffset, minY);

        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followLerp * Time.deltaTime));

        Vector3 toCam = transform.position - cam.position;
        if (toCam.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(toCam.normalized, Vector3.up),
                1f - Mathf.Exp(-rotateLerp * Time.deltaTime)
            );
        }
    }
}
