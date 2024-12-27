using UnityEngine;
using UnityEngine.InputSystem;

public class MultiHandLaserSetup : MonoBehaviour
{
    // this is designed to be used by the canvas objects to determine which camera to use
    public Camera activeUiCamera => _lastHandSide == HandSide.RIGHT ? 
        rightLaserObject.GetComponent<Camera>() : leftLaserObject.GetComponent<Camera>();
    
    [SerializeField] private VRHand leftHand;
    [SerializeField] private VRUILaserSetup leftLaserObject;
    
    [SerializeField] private VRHand rightHand;
    [SerializeField] private VRUILaserSetup rightLaserObject;

    [SerializeField] private GameObject menu;
    
    // [SerializeField] private InputActionAsset inputActions;
    
    private InputAction _menuAction; 
    private bool _uiActive;
    private HandSide _lastHandSide = HandSide.RIGHT;

    private void OnEnable()
    {
        // todo, this should be abstracted from the input system
        // _menuAction = inputActions.FindAction("XRI LeftHand/Menu");
        
        leftLaserObject.HandSideActive += SetHandSideLaserActive;
        rightLaserObject.HandSideActive += SetHandSideLaserActive; 
    }
    
    // intended for external use
    public void SetLaserActive(HandSide handSide) =>
        SetHandSideLaserActive(handSide);

    private void SetHandSideLaserActive(HandSide handSide)
    {
        _lastHandSide = handSide;

        if (handSide == HandSide.RIGHT)
        {
            rightLaserObject.gameObject.SetActive(true);
            rightHand.SetUIAnimation(true);

            leftLaserObject.gameObject.SetActive(false);
            leftHand.SetUIAnimation(false);
        }
        else
        {
            rightLaserObject.gameObject.SetActive(false);
            rightHand.SetUIAnimation(false);

            leftLaserObject.gameObject.SetActive(true);
            leftHand.SetUIAnimation(true);
        }
    }

    private void OnDisable()
    {
        _uiActive = false;
        leftLaserObject.HandSideActive -= SetHandSideLaserActive; 
        rightLaserObject.HandSideActive -= SetHandSideLaserActive; 
    }

    public void MenuAction()
    {
        if (_uiActive)
        {
            leftLaserObject.gameObject.SetActive(false);
            rightLaserObject.gameObject.SetActive(false);
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
            SetHandSideLaserActive(_lastHandSide);      
        }
        
        _uiActive = !_uiActive;
        rightHand.ui = _uiActive;
        leftHand.ui = _uiActive;
       
    }
}
