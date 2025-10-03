using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBoxerView : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI textHealth;
    public Image visual;
    
    private int currentHealth;
    public GameObject blueBG;
    public GameObject redBG;
    
    public void Setup(BoxerData data, bool isPlayer)
    {
        cardName.text = data.displayName.ToUpper();
        visual.sprite = data.visual;
        currentHealth = data.health;
        UpdateHealthValor(data.health);
        blueBG.SetActive(isPlayer);
        redBG.SetActive(!isPlayer);
    }

    public void UpdateHealthValor(int heath)
    {
        textHealth.text = $"{heath}";
        
        bool isDamage = heath < currentHealth;
        bool isHealing = heath > currentHealth;
        
        CardImpactAnimation.PlayTextPunchScale(textHealth.transform);
        
        if (isDamage)
        {
            CardImpactAnimation.PlayDamageImpact(visual);
        }
        else if (isHealing)
        {
            CardImpactAnimation.PlayHealingImpact(visual);
        }
        
        currentHealth = heath;
    }
}
