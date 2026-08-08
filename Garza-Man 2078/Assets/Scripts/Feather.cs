using UnityEngine;

public class Feather : Interactable
{
    [Header("Pickup Audio")]
    public AudioClip pickupClip;
    public float pickupVolume = 0.8f;

    protected override void Interact()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.CollectFeather();

            if (pickupClip != null)
            {
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
            }

            Destroy(gameObject);
        }
    }
}