using System.Collections;
using UnityEngine;
using TMPro; // for TextMeshPro

public class DialogueManager : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject uiRoot;              // ← assign your Panel here (child of the Canvas)
    [SerializeField] private TextMeshProUGUI dialogueText;   // ← TMP under that panel
    [SerializeField] private float showSeconds = 3f;

    [Header("Head-Lock Follow")]
    [SerializeField] private bool followHead = true;
    [SerializeField] private float followDistance = 1.2f;
    [SerializeField] private float verticalOffset = -0.05f;
    [SerializeField] private float followLerp = 12f;
    [SerializeField] private float rotateLerp = 20f;
    [SerializeField] private float minY = 0.2f;

    public static DialogueManager Instance { get; private set; }

    private Transform cam;
    private bool isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Get HMD camera (MainCamera-tagged recommended)
        var xrCam = Camera.main;
        if (xrCam == null)
        {
            var cams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (cams.Length > 0) xrCam = cams[0];
        }
        cam = xrCam != null ? xrCam.transform : null;

        // Keep Canvas active; hide only the panel
        if (uiRoot != null) uiRoot.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
    }

    public void ShowDialogue(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (dialogueText != null) dialogueText.text = message;

        // Snap to a good pose before showing
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
            Quaternion desiredRot = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(toCam.normalized, Vector3.up),
                1f - Mathf.Exp(-rotateLerp * Time.deltaTime)
            );
            transform.rotation = desiredRot;
        }
    }
}