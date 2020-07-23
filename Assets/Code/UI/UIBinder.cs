using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class UIBinder : MonoBehaviour
{
    #region Required Components
    private UIDocument _uiDocument;
    public UIDocument uiDocument => _uiDocument ? _uiDocument : (_uiDocument = GetComponent<UIDocument>());
    #endregion
}
