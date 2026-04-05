using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Used for multiplayer buttons on the MainMenu canvas
public class MultiplayerMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject joinPanel;          // panel for IP entry
    [SerializeField] private TMP_InputField ipInputField;   // IP address for host
    [SerializeField] private TMP_Text statusText;           // status message

    // used when selecting Host button
    public void OnHostClicked()
    {
        if (statusText != null) statusText.text = "Starting host...";
        NetworkGameManager.NGMInstance.HostGame();
    }

    // used when selecting Server button
    public void OnServerClicked()
    {
        if (statusText != null) statusText.text = "Starting server...";
        NetworkGameManager.NGMInstance.StartServer();
    }

    // used when selecting the Client button to enter the IP
    public void OnClientClicked()
    {
        if (joinPanel != null) joinPanel.SetActive(true);
    }

    // used when selecting the Solo Play button 
    public void OnSoloClicked()
    {
        NetworkGameManager.NGMInstance.PlaySolo();
    }

    // used when selecting Confirm Join button from after entering the IP
    public void OnConfirmJoinClicked()
    {
        string ip = ipInputField != null ? ipInputField.text.Trim() : "127.0.0.1";
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
        if (statusText != null) statusText.text = $"Connecting to {ip}...";
        NetworkGameManager.NGMInstance.JoinGame(ip);
    }

    // used when selecting the Cancel button from the IP panel
    public void OnCancelJoinClicked()
    {
        if (joinPanel != null) joinPanel.SetActive(false);
    }
}
