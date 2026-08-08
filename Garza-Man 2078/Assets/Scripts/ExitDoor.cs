using UnityEngine;

public class ExitDoor : Interactable
{
    public int requiredFeathers = 10;
    public string winMessage = "You Escaped!";

    protected override void Interact()
    {
        if (PlayerStats.Instance != null)
        {
            if (PlayerStats.Instance.feathersCollected >= requiredFeathers)
            {
                Win();
            }
            else
            {
                Debug.Log($"You need {requiredFeathers - PlayerStats.Instance.feathersCollected} more feathers!");
                // We could update the prompt dynamically or show a message
            }
        }
    }

    private void Win()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerVictory();
        }
        else
        {
            Debug.Log("VICTORY - Puchi escaped the sanctuary! (GameManager missing)");
            Time.timeScale = 0;
        }
    }
}
