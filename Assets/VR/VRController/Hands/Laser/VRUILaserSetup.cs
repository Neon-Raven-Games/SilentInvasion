using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(LineRenderer))]
public class VRUILaserSetup : MonoBehaviour
{
    [SerializeField] private HandSide handSide;

    // todo, convert cam far clip plane to distance for raycast
    [SerializeField] private float distance = 20;

    // components
    private Camera _laserCamera;
    private GameObject _crosshair;

    // input
    private bool _click;
    private bool _pointerDown;
    private InputAction pointerDown;

    // events
    private GameObject lastHitObject;
    private EventSystem eventSystem;

    // visuals
    private LineRenderer _lineRenderer;
    private Transform _anchor;


    // coordinates
    private Vector3 hitPoint;

    public event Action<HandSide> HandSideActive;

    private void InvokeHandSideActive() =>
        HandSideActive?.Invoke(handSide);

    private void Awake()
    {
        _anchor = transform;
        _laserCamera = GetComponent<Camera>();
        _laserCamera.nearClipPlane = 0.01f;
        _laserCamera.farClipPlane = 2.5f;

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.02f;
        _lineRenderer.SetPosition(0, _anchor.transform.position);

        _crosshair = transform.GetChild(0).gameObject;

        if (!_crosshair)
        {
            Debug.LogError(
                "No crosshair child found in the prefab. Please add a crosshair object as a child of the prefab.");
            _crosshair = new GameObject("Crosshair");
            _crosshair.transform.SetParent(transform);
        }

        _crosshair.SetActive(false);

        var eSystem = FindAnyObjectByType<EventSystem>();
        if (eSystem != null)
        {
            eventSystem = eSystem;
            return;
        }

        var eventSystemObject = new GameObject("EventSystem");
        eventSystem = eventSystemObject.AddComponent<EventSystem>();
    }

    private InputAction _uiAction;
    [SerializeField] private InputActionAsset actionAsset;

    private void OnEnable()
    {
        var side = "Right";
        if (handSide == HandSide.LEFT) side = "Left";
        _uiAction = actionAsset.FindAction($"XRI {side} Interaction/UI Press", true);
        _uiAction.performed += ctx => OnUITrigger();
        _uiAction.canceled += ctx => OnUITriggerRelease();
        _uiAction.Enable();
    }

    private void OnDisable()
    {
        _uiAction.Disable();
        _click = false;
        _pointerDown = false;
        slider = null;
    }

    public void OnUITriggerRelease()
    {
        _pointerDown = false;
        ExecuteEvents.ExecuteHierarchy(lastHitObject, new PointerEventData(eventSystem),
            ExecuteEvents.pointerUpHandler);
        eventSystem.SetSelectedGameObject(null);
        slider = null;
    }

    public void OnUITrigger()
    {
        InvokeHandSideActive();
        _click = true;
        _pointerDown = true;
    }

    private void Update()
    {
        var raycastResults = new List<RaycastResult>();
        var screenPosition = _laserCamera.ScreenToWorldPoint(_anchor.position + _anchor.forward * distance);
        var pointerEventData = new PointerEventData(eventSystem) {position = screenPosition};
        eventSystem.RaycastAll(pointerEventData, raycastResults);
        pointerEventData.pointerPressRaycast = pointerEventData.pointerCurrentRaycast;

        _lineRenderer.SetPosition(0, _anchor.transform.position);

        if (raycastResults.Count > 0)
        {
            var hitObject = raycastResults[0].gameObject;
            hitPoint = raycastResults[0].worldPosition;
            _lineRenderer.SetPosition(1, hitPoint);
            _crosshair.transform.position = hitPoint;
            _crosshair.transform.rotation = Quaternion.LookRotation(hitObject.transform.forward);


            if (!slider && lastHitObject != hitObject)
            {
                if (lastHitObject != null)
                    ExecuteEvents.ExecuteHierarchy(lastHitObject, pointerEventData, ExecuteEvents.pointerExitHandler);

                _crosshair.SetActive(true);
                ExecuteEvents.ExecuteHierarchy(hitObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
                lastHitObject = hitObject;
            }

            if (_click)
            {
                ExecuteEvents.ExecuteHierarchy(hitObject, pointerEventData, ExecuteEvents.pointerClickHandler);
                var drag = ExecuteEvents.GetEventHandler<IDragHandler>(hitObject);
                if (drag)
                {
                    slider = drag.GetComponent<Slider>();
                    sliderRectTransform = drag.GetComponent<RectTransform>();
                }

                _click = false;
            }

            if (!_pointerDown) return;

            ExecuteEvents.ExecuteHierarchy(hitObject, pointerEventData, ExecuteEvents.pointerDownHandler);
            Debug.Log(hitObject.name);
            if (!slider) return;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(sliderRectTransform, screenPosition, _laserCamera,
                out var localPoint);
            Vector2 localPosition = sliderRectTransform.InverseTransformPoint(localPoint);

            var normalizedValue = Mathf.InverseLerp(sliderRectTransform.rect.xMin, sliderRectTransform.rect.xMax,
                localPosition.x);
            var mappedValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
            if (slider.wholeNumbers) mappedValue = Mathf.Round(mappedValue);

            slider.value = mappedValue;
        }
        else
        {
            if (lastHitObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(lastHitObject, pointerEventData, ExecuteEvents.pointerExitHandler);
                lastHitObject = null;
            }

            _lineRenderer.SetPosition(1, _anchor.transform.position + _anchor.forward * distance);
            _crosshair.SetActive(false);
        }
    }

    private Slider slider;
    private RectTransform sliderRectTransform;


    private void OnDrawGizmos()
    {
        if (hitPoint == Vector3.zero) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint, 0.05f);
    }
}