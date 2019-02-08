using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelector : MonoBehaviour {

    public GameObject loadingScreen;
    public Slider slider;
    public Text progressText;

    public string TownName;

    private int townIndex;
    private int levelIndex;
    
    private int stars = 0;
    public int HScore1 = 0;
    public int HScore2 = 0;
    public int HScore3 = 0;
    public Text HighScore1;
    public Text HighScore2;
    public Text HighScore3;


    public SceneFader fader;
    public Button[] levelButtons;


    

    void Start()
    {
        //loop thorugh all the worlds
        for (int i = 1; i <= LockLevel.towns; i++)
        {
            if (Application.loadedLevelName == "Town" + i)
            {
                townIndex = i;  //save the world index value
                CheckLockedLevels(); //check for the locked levels 
            }
        }

        HScore1 = PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 1.ToString() + "Score");
        if (HScore1 > PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 1.ToString() + "0", 0))
        {
            PlayerPrefs.SetInt("0", HScore1);
            HighScore1.text = HScore1.ToString();
        }

        HScore2 = PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 2.ToString() + "Score");
        if (HScore2 > PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 2.ToString() + "0", 0))
        {
            PlayerPrefs.SetInt("0", HScore2);
            HighScore2.text = HScore2.ToString();
        }

        HScore3 = PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 3.ToString() + "Score");
        if (HScore3 > PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + 3.ToString() + "0", 0))
        {
            PlayerPrefs.SetInt("0", HScore3);
            HighScore3.text = HScore3.ToString();
        }

        Debug.Log("T" + townIndex.ToString() + ":" + TownName + "\n" + "     " +
                  "L" + 1.ToString() + " " + "Score" + "=" + HScore1 + "     " +
                  "L" + 2.ToString() + " " + "Score" + "=" + HScore2 + "     " +
                  "L" + 3.ToString() + " " + "Score" + "=" + HScore3);

        Debug.Log("---------------------------------------------------------");

    }

    public void Selectlevel(string worldLevel)
    {
        StartCoroutine(LoadAsynchronously(worldLevel));
    }

    void CheckLockedLevels()
    {
        Debug.Log("---------------------------------------------------------");
        //loop through the levels of a particular world
        for (int j = 1; j < LockLevel.levels; j++)
        {
            //get the number of stars obtained for that particular level
            //used to enable the image which should be displayed in the World1 scene beside the individual levels
            stars = PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + j.ToString() + "stars");
            levelIndex = (j + 1);
            //enable the respective image based on the stars variable value
            GameObject.Find(j + "star" + stars).GetComponent<Image>().enabled = true;
            Debug.Log("T" + townIndex.ToString() + ":" + TownName + "  " + "L" + j + " " + "star" + stars);
            //check if the level is locked 
            if ((PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + levelIndex.ToString())) == 1)
            {//disable the lock object which hides the level button
                GameObject.Find("LockedLevel" + levelIndex).SetActive(false);
            }
        }
    }

    public void Select(string levelName)
    {
        fader.FadeTo(levelName);
    }

    IEnumerator LoadAsynchronously(string worldLevel)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("level" + worldLevel);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            slider.value = progress;
            progressText.text = progress * 100f + "%";

            yield return null;
        }
    }
    public void Backbtn(int sceneIndex)
    {
        StartCoroutine(Back(sceneIndex));
    }
    IEnumerator Back(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex);
        yield return null;
    }
}
