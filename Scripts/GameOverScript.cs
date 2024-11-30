using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    public GameObject GameOverUI;
    public GameObject HUD;
    public Damagable damagable;
    public bool GameIsOver;

    private void Start()
    {
        GameIsOver = false;
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
        GameIsOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
