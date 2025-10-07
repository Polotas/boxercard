using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class DragStatus
{
    public static bool canDrag = false;
    
    public static bool CheckDropZones(Vector2 screenPosition, GameObject gameObject, CardController _cardController, List<RaycastResult> results,  PointerEventData pointerEventData)
    {
        Debug.Log("Check Drop Zones");

        foreach (var result in results)
        {
            UIDropZone dropZone = result.gameObject.GetComponent<UIDropZone>();
            if (dropZone != null)
            {
                // Verifica se pode aceitar o drop (validações básicas)
                if (_cardController.isPlayer != dropZone.isPlayer || 
                    _cardController.data.type != CardType.Defense && dropZone.dropZoneType == DropZoneType.Defense || 
                    _cardController.data.type == CardType.Defense && dropZone.dropZoneType == DropZoneType.AttackTable )
                {
                    Debug.Log("CANCEL DEFENSE");
                    return false;
                }
                
                pointerEventData.pointerDrag = gameObject;
                dropZone.HandleDrop(gameObject);
                return true;
            }
        }
        
        return false;
    }
}
