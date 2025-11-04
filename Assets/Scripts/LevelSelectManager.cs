using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;

    private string saveKey = "MaxLevelElert";

    void Start()
    {
        int maxLevelElert = PlayerPrefs.GetInt(saveKey, 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if ((i + 1) > maxLevelElert)
            {
                levelButtons[i].interactable = false;
            }
            else
            {
                levelButtons[i].interactable = true;
            }
        }
    }
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}