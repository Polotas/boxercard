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
    
    public GameObject healFX;
    public GameObject attackFX;
    
    public void Setup(BoxerData data, bool isPlayer)
    {
        cardName.text = data.boxerInfo.displayName.ToUpper();
        visual.sprite = data.boxerInfo.visual;
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
            attackFX.SetActive(false);
            CardImpactAnimation.PlayDamageImpact(visual);
            attackFX.SetActive(true);
        }
        else if (isHealing)
        {
            healFX.SetActive(false);
            CardImpactAnimation.PlayHealingImpact(visual);
            healFX.SetActive(true);
        }
        
        currentHealth = heath;
    }
}
