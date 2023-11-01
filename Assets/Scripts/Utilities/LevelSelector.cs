using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [TextArea(3, 10)]
    public List<string> levelDescriptions = new List<string>();
    public List<string> levelNames = new List<string>();

    private int currentLevelIndex = 1;
    public int totalLevels;

    public GlitchyText levelNameText;
    public GlitchyText levelDescriptionText;
    public SceneLoader sceneLoader;

    public void LoadSelectedLevel()
    {
        sceneLoader.LoadScene(currentLevelIndex);
    }

    public void UpdateLevelDescription()
    {
        string levelDescription = levelDescriptions[currentLevelIndex];
        levelDescriptionText.FastRevealText(levelDescription);
    }

    public void UpdateLevelName()
    {
        string levelName = levelNames[currentLevelIndex];
        levelNameText.FastRevealText(levelName);
    }

    public void IncreaseLevelIndex()
    {
        if (currentLevelIndex + 1 <= totalLevels)
        {
            currentLevelIndex++;
        }
        else
        {
            currentLevelIndex = 1;
        }
    }

    public void DecreaseLevelIndex()
    {
        if (currentLevelIndex - 1 > 1)
        {
            currentLevelIndex--;
        }
        else
        {
            currentLevelIndex = totalLevels;
        }
    }

}
