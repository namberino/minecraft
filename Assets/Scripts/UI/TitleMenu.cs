using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// a class of the title menu
public class TitleMenu : MonoBehaviour
{
    [Header("Main menu UI elements")]
    public TextMeshProUGUI seedField;

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks;
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
