using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PinGoalController : MonoBehaviour
{
    [SerializeField] private List<GameObject> effects = new();
    
    // unlocks door
    public UnityEvent onAllEffectsActivated;
    private int _index;

    public void ActivateEffect()
    {
        if (_index == -1) return;
        
        effects[_index].SetActive(true);
        _index++;

        if (_index != effects.Count) return;
        
        _index = -1;
        onAllEffectsActivated.Invoke();
    }           
}