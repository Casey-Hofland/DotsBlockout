using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuBinder : UIBinder
{
    private void Start()
    {
        uiDocument.rootVisualElement.Q<Button>("start-button").clicked += () => SceneManager.LoadScene("SampleScene");

#if UNITY_EDITOR
        uiDocument.rootVisualElement.Q<Button>("quit-button").clicked += () => UnityEditor.EditorApplication.isPlaying = false;
#else
        uiDocument.rootVisualElement.Q<Button>("quit-button").clicked += () => Application.Quit();
#endif
    }
}
