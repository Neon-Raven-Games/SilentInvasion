using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VRUIMenuTemplate : MonoBehaviour
{
    [SerializeField] private int sceneToLoad;
    [SerializeField] private Button loadSceneButton;
    [SerializeField] private Canvas menuCanvas;
    
    private MultiHandLaserSetup _multiHandLaserSetup;

    private void OnEnable()
    {
        _multiHandLaserSetup = FindObjectOfType<MultiHandLaserSetup>();
        menuCanvas.worldCamera = _multiHandLaserSetup.activeUiCamera;
        
        loadSceneButton.onClick.AddListener(() => LoadScene(sceneToLoad));
    }

    private void OnDisable() =>
        loadSceneButton.onClick.RemoveAllListeners();

    public void LoadScene(int sceneIndex) => SceneManager.LoadScene(sceneIndex);
}
