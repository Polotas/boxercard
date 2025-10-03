using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBoxerView : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI textHealth;
    public Image visual;

    public void Setup(BoxerData data)
    {
        cardName.text = data.displayName.ToUpper();
        visual.sprite = data.visual;
        UpdateHealthValor(data.health);
    }

    public void UpdateHealthValor(int heath)
    {
        textHealth.text = $"{heath}";
        var punchScale = .5f;
        var punch = new Vector3(punchScale, punchScale, punchScale);
        textHealth.transform.DOPunchScale(punch, 1);
    }
}
