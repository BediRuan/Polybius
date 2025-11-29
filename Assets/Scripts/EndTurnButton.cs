using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public void OnClick()
    {
        TurnManager.Instance.EndPlayerTurn();
    }
}

