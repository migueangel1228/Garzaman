using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Collection")]
    public int feathersCollected = 0;
    public int maxFeathers = 10;

    public UnityEvent<int> onFeatherCollected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectFeather()
    {
        feathersCollected++;
        onFeatherCollected?.Invoke(feathersCollected);
        
        Debug.Log($"Feather collected! Total: {feathersCollected}/{maxFeathers}");
        
        if (feathersCollected >= maxFeathers)
        {
            Debug.Log("All feathers collected! Head to the exit.");
        }
    }
}
