using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelLoader : MonoBehaviour
{
    public Button btn;
    private void Start()
    {
        btn.onClick.AddListener(changeLevel);
    }
   
    public void changeLevel()
    {
        int levelIndex = SceneManager.sceneCount - SceneManager.GetActiveScene().buildIndex;
       // StartCoroutine(LoadLevel(levelIndex));
        SceneManager.LoadScene(levelIndex);
    }

}
