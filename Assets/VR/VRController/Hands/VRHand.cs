using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[RequireComponent(typeof(HapticImpulsePlayer))]
public class VRHand : MonoBehaviour
{
    public HandSide handSide;
    public bool ui;
    [SerializeField] protected InputActionAsset actionAsset;
    
    protected Animator animator;
    private HapticImpulsePlayer _impulsePlayer;
    private InputAction _interaction;
    
    private static readonly int _SState = Animator.StringToHash("State");
    private static readonly int _SUI = Animator.StringToHash("UI");
    public Action onGrip { get; set; }

    public void PlayHapticImpulse(float amplitude, float duration)
    {
        if (ui) return;
        _impulsePlayer.SendHapticImpulse(amplitude, duration);
    }

    public void Awake()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged += ActivateHandOnFocusChanged;
        _impulsePlayer = GetComponent<HapticImpulsePlayer>();
    }

    protected virtual void OnEnable()
    {
        SetInputAction();
    }

    protected void OnDisable()
    {
        _interaction.Disable();
        _interaction.performed -= OnInteraction;
        _interaction.canceled -= OnInteractionCancelled;
    }

    private void SetInputAction()
    {
        var handString = handSide == HandSide.LEFT ? "Left" : "Right";
        _interaction = actionAsset.FindAction($"XRI {handString} Interaction/Select", true);
        _interaction.Enable();
        _interaction.performed += OnInteraction;
        _interaction.canceled += OnInteractionCancelled;
    }

    private void OnInteractionCancelled(InputAction.CallbackContext obj) => OnInteractionCancelled();
    private void OnInteraction(InputAction.CallbackContext obj) => OnInteraction();

    protected virtual void OnInteraction()
    {
        if (ui) return;
        onGrip?.Invoke();
        animator.SetInteger(_SState, 1);
    }

    protected virtual void OnInteractionCancelled()
    {
        if (ui) return;
        animator.SetInteger(_SState, 0);
    }

    public void OnDestroy()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged -= ActivateHandOnFocusChanged;
    }

    private void ActivateHandOnFocusChanged(bool hasFocus)
    {
        gameObject.SetActive(hasFocus);
    }

    public void SetUIAnimation(bool animating)
    {
        // todo, we need finger anim  
        // animator.SetBool(_SUI, animating);
    }
}