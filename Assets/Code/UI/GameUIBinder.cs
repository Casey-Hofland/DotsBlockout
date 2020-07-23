using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class GameUIBinder : UIBinder
{
    #region Visual Elements
    private Label _scoreLabel;
    public Label scoreLabel => _scoreLabel ?? (_scoreLabel = uiDocument.rootVisualElement.Q<Label>("score"));

    private Label _pauseLabel;
    public Label pauseLabel => _pauseLabel ?? (_pauseLabel = uiDocument.rootVisualElement.Q<Label>("paused"));

    private VisualElement _lives;
    public VisualElement lives => _lives ?? (_lives = uiDocument.rootVisualElement.Q("lives"));

    private Label _countDownLabel;
    public Label countDownLabel => _countDownLabel ?? (_countDownLabel = uiDocument.rootVisualElement.Q<Label>("countDown"));
    #endregion

    [SerializeField] private VisualTreeAsset liveAsset;

    private void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<UISystem>().ResetSystem(this);
    }

    public void AddLive()
    {
        VisualElement live = liveAsset.Instantiate();
        lives.Add(live);
    }

    public void RemoveLive()
    {
        if(lives.childCount > 0)
        {
            lives.RemoveAt(0);
        }
    }

    public int SetLives(int count)
    {
        var countOffset = count - lives.childCount;

        for(int i = 0; i < countOffset; i++)
        {
            AddLive();
        }

        for(int i = 0; i > countOffset; i--)
        {
            RemoveLive();
        }

        return countOffset;
    }
}
