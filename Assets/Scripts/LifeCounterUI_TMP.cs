using UnityEngine;
using TMPro;

public class LifeCounterUI_TMP : MonoBehaviour
{
    [Tooltip("Assign a TextMeshPro - Text (UI) component here")]
    public TextMeshProUGUI lifeText;

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
        if (lifeText == null)
            Debug.LogWarning("LifeCounterUI_TMP: lifeText not assigned. Create a TextMeshProUGUI in the Canvas and assign it.");
    }

    void UpdateLifeText(int lives)
    {
        if (lifeText != null)
            lifeText.text = "Life: " + lives;
    }
}