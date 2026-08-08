using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public string itemName = "Generic Item";

    public void Interact()
    {
        Debug.Log("Picked up: " + itemName);
        // Simple logic: disable or destroy the object
        gameObject.SetActive(false);
    }
}
