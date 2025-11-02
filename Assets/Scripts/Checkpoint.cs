using UnityEngine;
using UnityEngine.UI;
using System;

public class LifeCounterUI : MonoBehaviour
{
    public Text lifeText; // assign a UI Text in the inspector

    void OnEnable()
    {
        PlayerHealth.OnLivesChanged += UpdateLifeText;
    }

    void OnDisable()
    {
        PlayerHealth.OnLivesChanged -= UpdateLifeText;
    }

    void Start()
    {
        // initialize display if needed
        if (lifeText == null)
        {
            Debug.LogWarning("LifeCounterUI: lifeText not assigned.");
        }
    }

    void UpdateLifeText(int lives)
    {
        if (lifeText != null)
        {
            lifeText.text = "Life: " + lives;
        }
    }
}