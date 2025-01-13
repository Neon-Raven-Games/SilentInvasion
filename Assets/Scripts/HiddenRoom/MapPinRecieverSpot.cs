using UnityEngine;

public class MapPinRecieverSpot : MonoBehaviour
{
    public PinGoalController pinGoalController;
    [SerializeField] private Transform pinSpot;
    private void OnTriggerEnter(Collider other)
    {
        var pin = other.GetComponent<MapPin>();
        if (!pin) return;
        
        pinGoalController.ActivateEffect();
        pin.transform.position = pinSpot.position;
        gameObject.SetActive(false);
    }
}