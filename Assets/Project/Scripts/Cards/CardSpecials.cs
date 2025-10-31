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
        
        Debug.Log("SPECIAL TYPE: " + special);
        
        switch (special)
        {
            case SpecialType.ExtraCards:
                GetExtraCards();
                break;
            case SpecialType.ExtraDamage:
                ExtraDamage();
                break;
            case SpecialType.Shield:
                SuperDefense();
                break;
            case SpecialType.DestroyDefense:
                DestroyDefense();
                break;
            case SpecialType.AdrenalineRush:
                AdrenalineRush();
                break;
            case SpecialType.Focus:
                Focus();
                break;
            case SpecialType.Stun:
                Stun();
                break;
            case SpecialType.BreakGuard:
                BreakGuard();
                break;
            case SpecialType.Overcharge:
                Overcharge();
                break;
            case SpecialType.MirrorGuard:
                MirrorGuard();
                break;
            case SpecialType.Precision:
                Precision();
                break;
            case SpecialType.SecondWind:
                SecondWind();
                break;
            default:
                break;
        }
    }

    private void GetExtraCards() => FindFirstObjectByType<BattleManager>().GetExtrasCards(_cardController.isPlayer, _cardController.data.power);
    
    private void ExtraDamage() => FindFirstObjectByType<BattleManager>().ExtraDamage(_cardController.isPlayer);
    
    private void SuperDefense() => FindFirstObjectByType<BattleManager>().SuperDefense(_cardController.isPlayer);
    
    private void DestroyDefense() => FindFirstObjectByType<BattleManager>().DestroyDefenses(_cardController.isPlayer);

    private void AdrenalineRush() => FindFirstObjectByType<BattleManager>().ApplyAdrenalineRush(_cardController.isPlayer);
    private void Focus() => FindFirstObjectByType<BattleManager>().ApplyFocus(_cardController.isPlayer);
    private void Stun() => FindFirstObjectByType<BattleManager>().ApplyStunToOpponent(_cardController.isPlayer);
    private void BreakGuard() => FindFirstObjectByType<BattleManager>().ApplyBreakGuardToOpponent(_cardController.isPlayer);
    private void Overcharge() => FindFirstObjectByType<BattleManager>().ApplyOvercharge(_cardController.isPlayer);
    private void MirrorGuard() => FindFirstObjectByType<BattleManager>().ApplyMirrorGuard(_cardController.isPlayer);
    private void Precision() => FindFirstObjectByType<BattleManager>().ApplyPrecision(_cardController.isPlayer);
    private void SecondWind() => FindFirstObjectByType<BattleManager>().ApplySecondWind(_cardController.isPlayer);
}
