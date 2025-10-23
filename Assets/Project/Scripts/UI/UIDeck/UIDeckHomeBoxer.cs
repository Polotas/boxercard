using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckHomeBoxer : MonoBehaviour
{
    [Header("Component")] 
    public Button button;
    
    [Header("View")]
    public Image visual;
    public Image bg;
    public Image bgBoxer;
    public Image bgColor;
    public TextMeshProUGUI textName;

    public Color colorSelect;
    public Color colorDeselect;
    
    [SerializeField] private BoxerInfo _boxerInfo;
    private UIDeckHomeSelector _selector;
    
    public void Setup(BoxerInfo info, UIDeckHomeSelector selector)
    {
        _boxerInfo = info;
        _selector = selector;
        visual.sprite = _boxerInfo.visual;
        textName.text = _boxerInfo.displayName;
        button.onClick.AddListener(OnSelect);
    }

    public void OnSelect() => _selector.OnSelect(_boxerInfo.id);

    public void OnUpdate(string boxer)
    {
        bg.color = _boxerInfo.id == boxer ? Color.yellow : Color.black;
        bgBoxer.color = _boxerInfo.id == boxer ? Color.yellow : Color.black;
        bgColor.color = _boxerInfo.id == boxer ? colorSelect : colorDeselect;
    }
}
