using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReorientationHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionMap;
    [SerializeField] public Transform targetPosition;
    [SerializeField] private float reorientTime = 1f;
    private MultiHandLaserSetup _multiHandLaserSetup;
    private InputAction _reorientAction;
    private XROrigin _xrOrigin;
    private float _reorientTimer;
    private bool _reorienting;
    [SerializeField] private float yOffset = 1.6f;

    public static Action onReorient;

    private void Start()
    {
        _multiHandLaserSetup = FindObjectOfType<MultiHandLaserSetup>();
        _xrOrigin = FindObjectOfType<XROrigin>();
        _reorientAction = actionMap.FindAction("XRI Left/Menu", true);
        _reorientAction.performed += ctx => ReorientPlayer();
        _reorientAction.Enable();
    }

    public void RepositionPlayer()
    {
        if (_xrOrigin == null || targetPosition == null) return;

        // GameManager.reorienting = true;
        var originPos = targetPosition.position;
        originPos.y = _xrOrigin.CameraYOffset;

        var controllerPosition = targetPosition.position;
        controllerPosition.y = 0;

        _xrOrigin.transform.position = controllerPosition;
        _xrOrigin.MoveCameraToWorldLocation(originPos);
        _xrOrigin.transform.position = controllerPosition;

        var targetYaw = targetPosition.eulerAngles.y;
        var currentYaw = _xrOrigin.Camera.transform.eulerAngles.y;
        var yawDifference = targetYaw - currentYaw;
        _xrOrigin.RotateAroundCameraPosition(Vector3.up, yawDifference);

        // GameManager.reorienting = false;
    }

    private IEnumerator DelayCall()
    {
        yield return null;
        RepositionPlayer();
        yield return null;

        _xrOrigin.transform.position = new Vector3(_xrOrigin.transform.position.x,
            _xrOrigin.transform.position.y - (Camera.main.transform.position.y - yOffset), _xrOrigin.transform.position.z);
        yield return null;
        
        onReorient?.Invoke();
        onReorient = null;
        
        yield return null;
        
        _reorienting = false;
    }

    private void ReorientPlayer()
    {
        if (_reorienting) return;
        _reorienting = true;
        StartCoroutine(DelayCall());
    }
}