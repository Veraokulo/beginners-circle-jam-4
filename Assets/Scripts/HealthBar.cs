﻿using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Text text;

    public void SetHealth(float health)
    {
        slider.value = health;
        text.text = ((int) health).ToString();
    }
}