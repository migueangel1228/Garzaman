using UnityEngine;
using TMPro;

public class FeatherHUD : MonoBehaviour
{
    public TextMeshProUGUI featherText;
    public TextMeshProUGUI promptText;

    private void Start()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.onFeatherCollected.AddListener(UpdateFeatherCount);
            UpdateFeatherCount(PlayerStats.Instance.feathersCollected);
        }
        
        if (promptText != null) promptText.text = "";
        if (collectionMessageText != null) collectionMessageText.text = "";
    }

    private void UpdateFeatherCount(int count)
    {
        if (featherText != null)
        {
            featherText.text = $"Feathers: {count} / {PlayerStats.Instance.maxFeathers}";
        }

        if (count > 0)
        {
            ShowCollectionMessage($"Collected Feather ({count}/{PlayerStats.Instance.maxFeathers})");
        }

        if (count >= PlayerStats.Instance.maxFeathers)
        {
            ShowCollectionMessage("All Feathers Collected! Find the Exit!");
        }
    }

    private void ShowCollectionMessage(string message)
    {
        if (collectionMessageText != null)
        {
            collectionMessageText.text = message;
            CancelInvoke(nameof(ClearCollectionMessage));
            Invoke(nameof(ClearCollectionMessage), 3f);
        }
    }

    private void ClearCollectionMessage()
    {
        if (collectionMessageText != null) collectionMessageText.text = "";
    }

    public void SetPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }
    }

    public TextMeshProUGUI collectionMessageText;
    }
