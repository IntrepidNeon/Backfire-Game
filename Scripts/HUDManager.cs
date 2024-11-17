using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarAmount;

    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private TMP_Text ammoText;

    private float seconds;
    private int minutes;

    private void AddTime()
    {
        seconds += Time.deltaTime;
        timeText.text = minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
        if (seconds >= 60)
        {
            ++minutes;
            seconds = 0;
        }
    }

    private void AmmoCount()
    {

    }

    private void Update()
    {
        AddTime();
        AmmoCount();
    }



}
