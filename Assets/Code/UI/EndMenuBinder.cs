using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndMenuBinder : UIBinder
{
    private void Start()
    {
        uiDocument.rootVisualElement.Q<Button>("continue-button").clicked += () => SceneManager.LoadScene("MainMenu");
#if UNITY_EDITOR
        uiDocument.rootVisualElement.Q<Button>("quit-button").clicked += () => UnityEditor.EditorApplication.isPlaying = false;
#else
        uiDocument.rootVisualElement.Q<Button>("quit-button").clicked += () => Application.Quit();
#endif
    }
}
