using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    public GameObject GameOverUI;
    public GameObject HUD;
    public Damagable damagable;
    public bool gameIsOver;

    public bool GameIsOver { get { return gameIsOver; } }

    private void Start()
    {
        gameIsOver = false;
    }

    private void Update()
    {
        if (damagable.Health <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        GameOverUI.SetActive(true);
        HUD.SetActive(false);
        Time.timeScale = 0f;
        gameIsOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
