using UnityEngine;

public class BoxerManager : MonoBehaviour
{
    public BoxerData[] adversaryBoxers;

    public BoxerData GetAdversary()
    {
        var random = Random.Range(0, adversaryBoxers.Length);
        return adversaryBoxers[random];
    }
}
