using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public GameObject[] levelPanels;
    public Button[] leftArrowButtons;
    public Button[] rightArrowButtons;

    private int currentPanelIndex = 0;

    private void Start()
    {

        for (int i = 0; i < leftArrowButtons.Length; i++)
        {
            leftArrowButtons[i].onClick.AddListener(OnLeftArrowClick);
        }

        for (int i = 0; i < rightArrowButtons.Length; i++)
        {
            rightArrowButtons[i].onClick.AddListener(OnRightArrowClick);
        }
    }

    public void LoadLevel(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void OnLeftArrowClick()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            UpdatePanels();
        }
    }

    public void OnRightArrowClick()
    {
        if (currentPanelIndex < levelPanels.Length - 1)
        {
            currentPanelIndex++;
            UpdatePanels();
        }
    }

    public void Exit()
	{
        Application.Quit();
    }

    private void UpdatePanels()
    {
        for (int i = 0; i < levelPanels.Length; i++)
        {
            levelPanels[i].SetActive(i == currentPanelIndex);
        }
    }
}