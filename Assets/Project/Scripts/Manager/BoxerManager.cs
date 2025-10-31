using UnityEngine;

public class BoxerManager : MonoBehaviour
{
    public BoxerData[] playerBoxers;
    public BoxerData[] adversaryBoxers;

    public BoxerData GetAdversary()
    {
        var random = Random.Range(0, adversaryBoxers.Length);
        return adversaryBoxers[random];
    }

    public BoxerData GetPlayerBoxerData()
    {
        var playerBoxer = ScriptableObject.CreateInstance<BoxerData>();
        playerBoxer.cards = GameManager.Instance.GetPlayerListOfCardsDeck();

        var playerBoxerId = GameManager.Instance.playerData.currentBoxer;
        
        foreach (var t in playerBoxers)
        {
            if (t.boxerInfo.id != playerBoxerId) continue;
            playerBoxer.health = t.health;
            playerBoxer.boxerInfo = t.boxerInfo;
        }
        
        return playerBoxer;
    }
}
