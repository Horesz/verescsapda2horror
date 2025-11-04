using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteTrigger : MonoBehaviour
{
    [Tooltip("Ennek a pályának a sorszáma. Level 1 esetén 1, Level 2 esetén 2, stb.")]
    public int palyaSorszama;

    [Tooltip("A jelenet, amit betölt a pálya teljesítése után (pl. LevelSelect)")]
    public string jelenetAmitBetolt = "LevelSelect";

    private string saveKey = "MaxLevelElert";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int kovetkezoLevel = palyaSorszama + 1;

            int maxLevelElert = PlayerPrefs.GetInt(saveKey, 1);

            if (kovetkezoLevel > maxLevelElert)
            {
                PlayerPrefs.SetInt(saveKey, kovetkezoLevel);
                PlayerPrefs.Save();
            }
            SceneManager.LoadScene(jelenetAmitBetolt);
        }
    }
}
