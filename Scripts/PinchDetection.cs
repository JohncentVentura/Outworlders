using System;
using System.Collections;
using UnityEngine;

public class PinchDetection : MonoBehaviour
{
    [SerializeField] private bool isDebugging = true;
    [SerializeField] private float cameraMoveSpeed = 2f;
    [SerializeField] private float cameraZoomSpeed = 20f;
    [SerializeField] private float cameraOrthographicMaxSize = 6f; //Zoomout
    [SerializeField] private float cameraOrthographicMinSize = 1.5f; //Zoomin

    private TouchControls controls;
    private Coroutine moveCoroutine;
    private Coroutine zoomCoroutine;

    private void Awake()
    {
        controls = new TouchControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        //DEBUG
        if (isDebugging)
        {
            controls.Touch.PrimaryTouchContact.started += _ => ZoomStart();
        }

        controls.Touch.PrimaryTouchContact.started += _ => MoveStart();
        controls.Touch.SecondaryTouchContact.started += _ => ZoomStart();

        controls.Touch.PrimaryTouchContact.canceled += _ => MoveEnd();
        controls.Touch.PrimaryTouchContact.canceled += _ => ZoomEnd();
        controls.Touch.SecondaryTouchContact.canceled += _ => ZoomEnd();
    }

    private void MoveStart()
    {
        moveCoroutine = StartCoroutine(MoveDetection());
        Debug.Log("MoveStart");
    }

    private void MoveEnd()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            Debug.Log("MoveEnd");
        }
    }

    private void ZoomStart()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            Debug.Log("MoveEnd");
        }

        zoomCoroutine = StartCoroutine(ZoomDetection());
        Debug.Log("ZoomStart");
    }

    private void ZoomEnd()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
            Debug.Log("ZoomEnd");
        }
    }

    private IEnumerator MoveDetection()
    {
        Vector2 previousPosition = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
        
        while (true)
        {
            Vector2 currentPosition = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
            Vector2 delta = currentPosition - previousPosition;

            Vector3 move = cameraMoveSpeed * Time.deltaTime * new Vector3(-delta.x, -delta.y, 0);
            Camera.main.transform.position += move;

            /*
            if (delta.magnitude > 0.01f) // To avoid minor unwanted movements
            {
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * cameraMoveSpeed * Time.deltaTime;
                Camera.main.transform.position += move;
            }
            */

            previousPosition = currentPosition;
            yield return null;
        }
    }

    private IEnumerator ZoomDetection()
    {
        float previousDistance = Vector2.Distance(controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>(), controls.Touch.SecondaryFingerPosition.ReadValue<Vector2>());
        float distance = 0;

        while (true)
        {
            distance = Vector2.Distance(controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>(), controls.Touch.SecondaryFingerPosition.ReadValue<Vector2>());

            //Zoom in (Decreases Camera.main.orthographicSize)
            if (distance > previousDistance)
            {
                if (Camera.main.orthographicSize > cameraOrthographicMinSize) Camera.main.orthographicSize -= cameraZoomSpeed * Time.deltaTime;
                else Camera.main.orthographicSize = cameraOrthographicMinSize;
            }
            //Zoom out (Increases Camera.main.orthographicSize)
            else if (distance < previousDistance)
            {
                if (Camera.main.orthographicSize < cameraOrthographicMaxSize) Camera.main.orthographicSize += cameraZoomSpeed * Time.deltaTime;
                else Camera.main.orthographicSize = cameraOrthographicMaxSize;
            }

            previousDistance = distance;
            yield return null;
        }
    }
}
