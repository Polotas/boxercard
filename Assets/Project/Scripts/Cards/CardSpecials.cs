using UnityEngine;

public class CardSpecials : MonoBehaviour
{
    private CardController _cardController;

    public void Setup(CardController card)
    {
        _cardController = card;
    }

    public void UseSpecial()
    {
        var special = _cardController.data.special;
        
        switch (special)
        {
            case SpecialType.ExtraCards:
                GetExtraCards();
                break;
            case SpecialType.ExtraDamage:
               // GetExtraCards();
                break;
            case SpecialType.SuperDefense:
                SuperDefense();
                break;
            case SpecialType.DestroyDefense:
                DestroyDefense();
                break;
            default:
                break;
        }
    }

    private void GetExtraCards() => FindFirstObjectByType<BattleManager>().GetExtrasCards(_cardController.isPlayer, _cardController.data.power);
    
    private void SuperDefense() => FindFirstObjectByType<BattleManager>().SuperDefense(_cardController.isPlayer);
    
    private void DestroyDefense() => FindFirstObjectByType<BattleManager>().GetExtrasCards(_cardController.isPlayer, _cardController.data.power);
}
