using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Puzzle : MonoBehaviour {

    public TextMeshProUGUI HS;
    public int SecTime;
    public int SetTime3S;
    public int SetTime2S;

    protected string currentLevel;
    protected int townIndex;
    protected int levelIndex;

    public string Level;
    public GameObject StarUI;
    public Text scoreboard;

    private GameObject star1;
    private GameObject star2;
    private GameObject star3;
    //reference to next button
    private GameObject SkipBtn;

    public GameObject GameOverUI;
    public int timer = 5;
    public Text CountdownText;
    bool Playing = true;

    public Texture2D image;
    public int blocksPerLine = 4;
    public int shuffleLength = 20;
    public float defaultMoveDuration = .2f;
    public float shuffleMoveDuration = .1f;

    enum PuzzleState { Solved, Shuffling, InPlay};
    PuzzleState state;
   
    Block emptyBlock;
    Block[,] blocks;
    Queue<Block> inputs;
    bool blockIsMoving;
    int shuffleMoveRemaining;
    Vector2Int prevShuffleOffset;

    private void Start()
    {
        CreatePuzzle();
        StartShuffle();

        star1 = GameObject.Find("star1");
        star2 = GameObject.Find("star2");
        star3 = GameObject.Find("star3");

        StarUI.SetActive(false);
        //disable the image component of all the star images
        star1.GetComponent<Image>().enabled = false;
        star2.GetComponent<Image>().enabled = false;
        star3.GetComponent<Image>().enabled = false;

        HS.GetComponent<TextMeshProUGUI>().enabled = false;
        currentLevel = Application.loadedLevelName;
    }

    void Update()
    {
        if (Playing == true)
        {
            CountdownText.text = ("" + timer);

            if (timer == 0)
            {
                StopCoroutine("LostTime");
                CountdownText.text = "Time Up!";
                GameOverUI.SetActive(true);
                if (GameOverUI == true)
                {
                    FindObjectOfType<AudioManager>().Play("Lose");
                }
            }
        }
        if (state == PuzzleState.InPlay && Input.GetKeyDown(KeyCode.Space))
        {
            StartShuffle();
        }
    }

    void CreatePuzzle()
    {
        blocks = new Block[blocksPerLine, blocksPerLine];
        Texture2D[,] imageSlices = ImageSlicer.GetSlices(image, blocksPerLine);
        for (int y = 0; y < blocksPerLine; y++)
        {
            for (int x = 0; x < blocksPerLine; x++)
            {
                GameObject blockObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                blockObject.transform.position = -Vector2.one * (blocksPerLine - 1) * .5f + new Vector2(x, y);
                blockObject.transform.parent = transform;

                Block block = blockObject.AddComponent<Block>();
                block.OnBlockPressed += PlayerMoveBlockInput;
                block.OnFinishedMoving += OnBlockFinishedMoving;
                block.Init(new Vector2Int(x, y), imageSlices[x, y]);
                blocks[x, y] = block;

                if (y == 0 && x == blocksPerLine - 1)
                {
                    emptyBlock = block;
                }
            }
        }

        Camera.main.orthographicSize = blocksPerLine * .92f;
        inputs = new Queue<Block>();
    }

    void PlayerMoveBlockInput(Block blockToMove)
    {
        if (state == PuzzleState.InPlay)
        {
            inputs.Enqueue(blockToMove);
            MakeNextPlayerMode();
        }
    }

    void MakeNextPlayerMode()
    {
        while (inputs.Count > 0 && !blockIsMoving)
        {
            MoveBlock(inputs.Dequeue(), defaultMoveDuration);
        }
    }

    void MoveBlock(Block blockToMove, float duration)
    {
        if ((blockToMove.coord - emptyBlock.coord).sqrMagnitude == 1)
        {
            blocks[blockToMove.coord.x, blockToMove.coord.y] = emptyBlock;
            blocks[emptyBlock.coord.x, emptyBlock.coord.y] = blockToMove;

            Vector2Int targetCoord = emptyBlock.coord;
            emptyBlock.coord = blockToMove.coord;
            blockToMove.coord = targetCoord;

            Vector2 targetPosition = emptyBlock.transform.position;
            emptyBlock.transform.position = blockToMove.transform.position;
            blockToMove.MoveToPosition(targetPosition, duration);
            blockIsMoving = true;
        }
    }

    void OnBlockFinishedMoving()
    {
        blockIsMoving = false;
        CheckIfSolved();

        if (state == PuzzleState.InPlay)
        {
            MakeNextPlayerMode();
        }
        else if (state == PuzzleState.Shuffling)
        {
            if (shuffleMoveRemaining > 0)
            {
                MakeNextShuffleMove();
            }
            else
            {
                state = PuzzleState.InPlay;
                StartCoroutine("LoseTime");
            }
        }
    }

    void StartShuffle()
    {
        state = PuzzleState.Shuffling;
        shuffleMoveRemaining = shuffleLength;
        emptyBlock.gameObject.SetActive(false);
        MakeNextShuffleMove();
    }

    void MakeNextShuffleMove()
    {
        Vector2Int[] offsets = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        int randomIndex = Random.Range(0, offsets.Length);

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int offset = offsets[(randomIndex + i) % offsets.Length];
            if (offset != prevShuffleOffset * -1)
            {
                Vector2Int moveBlockCoord = emptyBlock.coord + offset;

                if (moveBlockCoord.x >= 0 && moveBlockCoord.x < blocksPerLine && moveBlockCoord.y >= 0 && moveBlockCoord.y < blocksPerLine)
                {
                    MoveBlock(blocks[moveBlockCoord.x, moveBlockCoord.y], shuffleMoveDuration);
                    shuffleMoveRemaining--;
                    prevShuffleOffset = offset;
                    break;
                }
            }
        }
    }

    void CheckIfSolved()
    {
        foreach (Block block in blocks)
        {
            if (!block.IsAtStartingCoord())
            {
                return;
            }
        }
        new WaitForSeconds(.3f);
        emptyBlock.gameObject.SetActive(true);
        state = PuzzleState.Solved;
        
        if (timer > SetTime3S)
        {
            star3.GetComponent<Image>().enabled = true;
            UnlockLevels(3);
        }
        else if (timer > SetTime2S)
        {
            star2.GetComponent<Image>().enabled = true;
            UnlockLevels(2);
        }
        else if (timer > 0)
        {
            star1.GetComponent<Image>().enabled = true;
            UnlockLevels(1);
        }
        
        int Score = (SecTime - (SecTime - timer)) * 123;
        PlayerPrefs.SetInt("ScoreBoard", Score);
        scoreboard.text = Score.ToString();

        PlayerPrefs.SetInt(Level + "HighScore", Score);

        HighScore(Score);
        GameOverUI.SetActive(false);
        StarUI.SetActive(true);
        if (StarUI == true)
        {
            Playing = false;
        }


        FindObjectOfType<AudioManager>().Play("Win");
        
    }
    public void Skip()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    IEnumerator LoseTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            timer--;
        }
    }
    public void Play()
    {
        Playing = true;
        Time.timeScale = 1f;
    }
    public void Stop()
    {
        Playing = false;
        Time.timeScale = 0f;
    }
    protected void HighScore(int Score)
    {
        for (int i = 0; i < LockLevel.towns; i++)
        {
            for (int j = 1; j < LockLevel.levels; j++)
            {
                if (currentLevel == "Level" + (i + 1).ToString() + "." + j.ToString())
                {
                    townIndex = (i + 1);
                    levelIndex = (j + 1);
                    if (PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + j.ToString() + "Score") < Score)
                        //overwrite the stars value with the new value obtained
                        HS.GetComponent<TextMeshProUGUI>().enabled = true;
                        PlayerPrefs.SetInt("level" + townIndex.ToString() + ":" + j.ToString() + "Score", Score);

                }
            }

        }
    }
    protected void UnlockLevels(int stars)
    {

        //set the playerprefs value of next level to 1 to unlock
        //also set the playerprefs value of stars to display them on the World levels menu
        for (int i = 0; i < LockLevel.towns; i++)
        {
            for (int j = 1; j < LockLevel.levels; j++)
            {
                if (currentLevel == "Level" + (i + 1).ToString() + "." + j.ToString())
                {
                    townIndex = (i + 1);
                    levelIndex = (j + 1);
                    PlayerPrefs.SetInt("level" + townIndex.ToString() + ":" + levelIndex.ToString(), 1);
                    //check if the current stars value is less than the new value
                    if (PlayerPrefs.GetInt("level" + townIndex.ToString() + ":" + j.ToString() + "stars") < stars)
                        //overwrite the stars value with the new value obtained
                        PlayerPrefs.SetInt("level" + townIndex.ToString() + ":" + j.ToString() + "stars", stars);
                }
            }
        }

    }

}
