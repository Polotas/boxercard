using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class CardImpactAnimation
{
    /// <summary>
    /// Aplica animação de impacto de dano (vermelho + shake)
    /// </summary>
    public static void PlayDamageImpact(Image visual)
    {
        if (visual == null) return;
        
        // Punch scale
        visual.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.3f, 8, 0.8f);
        
        // Shake rotation para simular impacto
        visual.transform.DOShakeRotation(0.3f, 10f, 20, 90f);
        
        // Flash vermelho
        var originalColor = visual.color;
        visual.DOColor(new Color(1f, 0.3f, 0.3f, 1f), 0.1f)
            .OnComplete(() => visual.DOColor(originalColor, 0.2f));
    }
    
    /// <summary>
    /// Aplica animação de cura (verde claro + brilho suave)
    /// </summary>
    public static void PlayHealingImpact(Image visual)
    {
        if (visual == null) return;
        
        // Punch scale
        visual.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.3f, 8, 0.8f);
        
        // Flash verde claro
        var originalColor = visual.color;
        visual.DOColor(new Color(0.6f, 1f, 0.6f, 1f), 0.15f)
            .OnComplete(() => visual.DOColor(originalColor, 0.25f));
    }
    
    /// <summary>
    /// Aplica animação de punch scale em um transform (para textos, etc)
    /// </summary>
    public static void PlayTextPunchScale(Transform target, float punchScale = 0.8f, float duration = 0.4f)
    {
        if (target == null) return;
        
        var punch = new Vector3(punchScale, punchScale, punchScale);
        target.DOPunchScale(punch, duration, 10, 0.5f).SetEase(Ease.OutBounce);
    }
    
    /// <summary>
    /// Aplica flash de cor customizado em uma imagem
    /// </summary>
    public static void PlayColorFlash(Image visual, Color flashColor, float flashDuration = 0.1f, float returnDuration = 0.2f)
    {
        if (visual == null) return;
        
        var originalColor = visual.color;
        visual.DOColor(flashColor, flashDuration)
            .OnComplete(() => visual.DOColor(originalColor, returnDuration));
    }
}

