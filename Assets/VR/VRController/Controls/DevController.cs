using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VRController.Hands;
using VRController.Hands.Physics;

public enum RotationMode
{
    Smooth,
    Snap
}

public enum LocomotionMode
{
    None,
    Smooth,
    Teleport,
}

public enum HandSide
{
    LEFT,
    RIGHT
}

[RequireComponent(typeof(CharacterController))]
public class DevController : MonoBehaviour
{
    [Header("Input Settings")] [SerializeField]
    private InputActionAsset actionAsset;

    [SerializeField] private VRHand leftHand;
    [SerializeField] private VRHand rightHand;

    [SerializeField] private float analogThreshold = 0.2f;
    [SerializeField] private Transform hmd;
    [SerializeField] private Transform handsAnchor;
    [SerializeField] private Transform camOffset;

    [Header("Rotation Settings")] [SerializeField]
    private RotationMode rotationMode;

    [SerializeField] private float smoothRotationSpeed = 100.0f;
    [SerializeField] private float snapRotationAmount = 45f;
    [SerializeField] private float snapRotationDelay;

    [Header("Movement Settings")] [SerializeField]
    private LocomotionMode locomotionMode;

    [SerializeField] private float speed = 5.0f;

    [Header("Comfort Settings")] [SerializeField]
    private bool initialRotationVignette;

    [SerializeField] private bool initialLocomotionVignette;

    public bool RotationVignette
    {
        get => _vignetteController.rotationVignette;
        set => _vignetteController.rotationVignette = value;
    }

    public bool LocomotionVignette
    {
        get => _vignetteController.locomotionVignette;
        set => _vignetteController.locomotionVignette = value;
    }
    // todo, teleporter

    // character populated components
    private CharacterController _controller;
    private VignetteController _vignetteController;

    // input properties
    private InputAction _moveForwardAction;
    private InputAction _lookAction;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _lastSnapRotation;

    // todo, we can make splash screen
    // https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.12/api/UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature.html

    private void Start()
    {
        _vignetteController = GetComponentInChildren<VignetteController>();
        _controller = GetComponent<CharacterController>();
        LocomotionVignette = initialLocomotionVignette;
        RotationVignette = initialRotationVignette;
        ResetHandAnchor();
        StartCoroutine(DelayCalled());
    }

    private void RefreshBattersDominance()
    {
        // todo, set batter pos for player 
    }

    public void HapticFeedback(HandSide handSide)
    {
        if (handSide == HandSide.LEFT) PlayLeftFeedback();
        else PlayRightFeedback();
    }

    public void HapticFeedback()
    {
        leftHand.PlayHapticImpulse(0.75f, 0.5f);
        rightHand.PlayHapticImpulse(0.75f, 0.5f);
        _vignetteController.PunchTweenDamageVignette();
    }

    public void PlayLeftFeedback()
    {
        leftHand.PlayHapticImpulse(0.35f, 0.2f);
    }

    public void PlayRightFeedback()
    {
        rightHand.PlayHapticImpulse(0.35f, 0.2f);
    }

    private IEnumerator DelayCalled()
    {
        yield return null;
        yield return null;
        yield return null;
        ResetHandAnchor();
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            StartCoroutine(DelayCalled());
            // GameManager.paused = false;
        }
        else
        {
            // GameManager.paused = true;
        }
    }

    private void ResetHandAnchor()
    {
        var handPos = handsAnchor.localPosition;
        handPos.x = 0;
        handPos.z = 0;
        handPos.y = camOffset.localPosition.y;
        handsAnchor.localPosition = handPos;
    }

    void UpdatePhysicsTimesteps()
    {
        var refreshRate = (float) Screen.currentResolution.refreshRateRatio.value;
        Time.fixedDeltaTime = 1 / (refreshRate > 0 ? refreshRate : 90f);

#if !UNITY_EDITOR
        Time.maximumDeltaTime = 0.0333f;
#endif
    }

    protected void Awake()
    {
        UpdatePhysicsTimesteps();
        AssignInput();

        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged += OnApplicationFocusChanged;
    }

    private void AssignInput()
    {
        actionAsset.Enable();

        _moveForwardAction = actionAsset.FindAction("XRI Left Locomotion/Move", true);
        _moveForwardAction.Enable();

        _lookAction = actionAsset.FindAction("XRI Right Locomotion/Turn", true);
        _lookAction.Enable();

        _moveForwardAction.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _lookAction.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();

        _moveForwardAction.canceled += _ => _moveInput = Vector2.zero;
        _lookAction.canceled += _ => _lookInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged -= OnApplicationFocusChanged;
    }


    private void OnEnable()
    {
        if (_moveForwardAction != null) _moveForwardAction.Enable();
        if (_lookAction != null) _lookAction.Enable();
    }

    private void OnDisable()
    {
        _moveForwardAction.Disable();
        _lookAction.Disable();
    }

    protected void Update()
    {
        // if (GameManager.reorienting) return;

        ResetHandAnchor();

        if (locomotionMode != LocomotionMode.None)
        {
            HandleRotation();
            HandleMovement();
        }

        SyncBaseObjectWithCamera();
        _vignetteController.StopVignette();
    }

    private void SyncBaseObjectWithCamera()
    {
        var hmdPos = hmd.position;
        var targetPosition = new Vector3(hmdPos.x, transform.position.y, hmdPos.z);
        var movementOffset = targetPosition - transform.position;

        transform.position += movementOffset;
        hmd.position = hmdPos;
        handsAnchor.localPosition = new Vector3(hmd.localPosition.x, handsAnchor.localPosition.y, hmd.localPosition.z);
        ResizeControllerHeightToHmd();
    }

    private void ResizeControllerHeightToHmd()
    {
        var hmdPos = hmd.position;
        var controllerPos = _controller.transform.position;
        var heightDifference = hmdPos.y - controllerPos.y;
        _controller.height = heightDifference + 0.1f;
        _controller.center = new Vector3(0, heightDifference / 2 + 0.05f, 0);
    }

    private void HandleRotation()
    {
        if (Mathf.Abs(_lookInput.x) < analogThreshold)
        {
            if (rotationMode == RotationMode.Smooth) _vignetteController.StopRotationLerp();
            return;
        }

        if (rotationMode == RotationMode.Snap)
        {
            if (Time.time - _lastSnapRotation < snapRotationDelay) return;
            _lastSnapRotation = Time.time;
        }

        _vignetteController.StartRotationLerp(rotationMode);

        var angle = rotationMode == RotationMode.Smooth
            ? _lookInput.x * smoothRotationSpeed * Time.fixedDeltaTime
            : _lookInput.x;
        HandleRotationWithVignette(angle);
    }

    private void HandleRotationWithVignette(float angle)
    {
        if (rotationMode == RotationMode.Snap)
        {
            if (angle < -analogThreshold) angle = -snapRotationAmount;
            else if (angle > analogThreshold) angle = snapRotationAmount;
        }

        transform.Rotate(Vector3.up, angle);
    }

    private void HandleMovement()
    {
        if (Mathf.Abs(_moveInput.x) <= analogThreshold) _moveInput.x = 0;
        if (Mathf.Abs(_moveInput.y) <= analogThreshold) _moveInput.y = 0;
        if (_moveInput == Vector2.zero)
        {
            _vignetteController.StopLocomotionLerp();
            return;
        }

        _vignetteController.StartLocomotionLerp();
        var movement = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        movement = hmd.transform.TransformDirection(movement) * (speed * Time.deltaTime);
        movement.y = Physics.gravity.y * Time.deltaTime;
        _controller.Move(movement);
    }
}