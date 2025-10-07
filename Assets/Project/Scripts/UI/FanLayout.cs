using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FanLayout : MonoBehaviour
{
    [Header("Configurações do Leque")]
    [SerializeField] private float radius = 300f;          
    [SerializeField] private float totalAngle = 60f;        
    [SerializeField] private float verticalOffset = -100f;  

    [Header("Animação")]
    [SerializeField] private float animationDuration = 0.5f;
    
    [SerializeField] private List<RectTransform> cards = new List<RectTransform>();

    
    public void AddCard(RectTransform card, int siblingIndex = -1)
    {
        if (cards.Contains(card)) return;
        
        card.SetParent(transform);
        card.transform.localScale = Vector3.one;
        
        if (siblingIndex != -1 && siblingIndex < cards.Count)
            cards.Insert(siblingIndex,card);
        else
            cards.Add(card);
        
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetSiblingIndex(i);
        }
        
        UpdateLayout();
    }

    public void RemoveCard(RectTransform card)
    {
        cards.Remove(card);
        UpdateLayout();
    }
    
    public List<RectTransform> GetCardsInLayout() => new List<RectTransform>(cards);

    public void UpdateLayout()
    {
        int count = cards.Count;
        if (count == 0) return;

        float angleStep = (count > 1) ? totalAngle / (count - 1) : 0;
        float startAngle = -totalAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            // Calcula posição no arco
            Vector2 pos = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius + verticalOffset);

            // Aplica posição e rotação com DOTween
            cards[i].DOAnchorPos(pos, animationDuration).SetEase(Ease.OutQuad);
            cards[i].DORotateQuaternion(Quaternion.Euler(0, 0, -angle), animationDuration);
        }
    }
}