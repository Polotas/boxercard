public class PlayerController : DeckController
{
    public UIDropZone[] defenses;
    public UIDropZone corner;
    public UIDropZone attackTable;

    public void StartGame(BoxerData data)
    {
        cards = data.cards;
        SetupBoxer(data);
    } 
}
