using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{

    [NonSerialized]
    public Camera Camera;
    public Vector3 current;

    public float Sensivity = 1;
    public float ScrollDelta = 15;
    private bool _drag = false;
    public static CameraMovement Instance;

    public static bool MovedLastFrame;
    [NonSerialized]
    public float MoveTreshold = 1.2f;
    public static EventSystem EventSystem;

    void Awake()
    {
        Register.float1 = Sensivity;
        Register.float2 = ScrollDelta;
        EventSystem = FindObjectOfType<EventSystem>();
        Camera = GetComponent<Camera>();
        Instance = this;
        Camera.orthographicSize = 8;
        GameController.OnPlaceCreated += OnCastleCreated;
        Sensivity = 0;
        ScrollDelta = 0;
    }

    private void OnCastleCreated(Place place)
    {
        GameController.OnPlaceCreated -= OnCastleCreated;
        Camera.DOOrthoSize(10, 2f);
        Sensivity = Register.float1;
        ScrollDelta = Register.float2;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUIObject())
                return;
            _drag = true;
            current = Input.mousePosition;
            MovedLastFrame = false;
        }

        if (Input.GetMouseButton(0) && _drag)
        {
            var delta = Input.mousePosition - current;
            transform.Translate(-delta.x * Sensivity, -delta.y * Sensivity, 0);
            current = Input.mousePosition;
            if (!MovedLastFrame)
            {
                MovedLastFrame = Mathf.Abs(delta.x + delta.y) > MoveTreshold;
                if (MovedLastFrame)
                {
                    GameController.Instance.OnCameraMove();
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            _drag = false;
        }

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
        {
            if (!IsPointerOverUIObject())
            {
                Camera.orthographicSize -= ScrollDelta * Input.mouseScrollDelta.y * Time.unscaledDeltaTime;
                if (Camera.orthographicSize > 12)
                {
                    Camera.orthographicSize = 12;
                }
                else if (Camera.orthographicSize < 3)
                {
                    Camera.orthographicSize = 3;
                }
            }
        }
    }
}
