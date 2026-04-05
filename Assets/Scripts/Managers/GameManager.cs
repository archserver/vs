using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages overall game state — pause, game over, restart, and survival timer
public class GameManager : NetworkBehaviour
{
    public static GameManager GMInstance;       // pointer so any script can trigger game events
    private bool paused = false;                // tracks whether the game is currently paused
    public float aliveTimer;                    // total seconds the player has survived this run
    public bool gameIsActive = false;           // set to true once the player spawns in

    // prevent duplicate manager instances when changing scenes
    private void Awake()
    {
        if (GMInstance != null && GMInstance != this)
            Destroy(this);
        else
            GMInstance = this;
    }

    // increment the survival timer and update the UI every frame while the game is active
    private void Update()
    {
        if (gameIsActive)
        {
            aliveTimer += Time.deltaTime;
            UIController.UIInstance?.UpdateTime(aliveTimer);
        }
    }

    // stop the timer and show the game over panel after a short delay
    public void GameOver()
    {
        gameIsActive = false;
        StartCoroutine(DelayGameOverPanel());
    }

    // wait 2 seconds before showing the game over panel so death animation can play
    IEnumerator DelayGameOverPanel()
    {
        yield return new WaitForSeconds(2f);
        GameStats.GSInstance.CommitSessionToLifetime();
        if (NetworkGameManager.IsSolo)
        {
            ShowGameOverClientRpc();
        }
        else if (IsServer)
        {
            // tell all clients to show the game over screen
            ShowGameOverClientRpc();
        }
    }

    // shows game over panel on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowGameOverClientRpc()
    {
        UIController.UIInstance.ShowGameOverStats();
        UIController.UIInstance.gameOverPanel.SetActive(true);
    }

    // reload the game scene to restart from the beginning
    public void Restart()
    {
        if (NetworkGameManager.IsSolo)
        {
            SceneManager.LoadScene("Game1");
        }
        else if (IsServer)
        {
            RestartClientRpc();
        }
    }

    // reload the scene on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void RestartClientRpc()
    {
        SceneManager.LoadScene("Game1");
    }

    // toggle pause — freezes time and shows the pause panel, or resumes if already paused
    // does nothing if the game over panel is showing
    public void Pause()
    {
        if (!UIController.UIInstance.gameOverPanel.activeSelf)
        {
            if (!paused)
            {
                paused = true;
                UIController.UIInstance.pausedPanel.SetActive(true);
                Time.timeScale = 0f;
                AudioController.ACInstance.PlaySound(AudioController.ACInstance.pause);
            }
            else
            {
                paused = false;
                UIController.UIInstance.pausedPanel.SetActive(false);
                Time.timeScale = 1f;
                AudioController.ACInstance.PlaySound(AudioController.ACInstance.resume);
            }
        }
    }

    // quit the application
    public void ExitGame()
    {
        Application.Quit();
    }

    // return to the main menu scene
    public void MainManu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
