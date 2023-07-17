using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z,
    };

    private Row[] rows;

    private int rowIndex;
    private int columnIndex;

    private string[] solutions;

    List <string> validWords;

    [SerializeField]
    private string word;

    public GameManager gameManager = GameManager.Instance;

    [Header("Tiles")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();
        NewGame();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameManager.wordLength == 4)
        {
            foreach (var row in rows)
            {
                row.tiles[4].gameObject.SetActive(false);
                row.tiles[5].gameObject.SetActive(false);
            }
        }
        else if (gameManager.wordLength == 5)
        {
            foreach (var row in rows)
            {
                row.tiles[5].gameObject.SetActive(false);
            }
        }
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n').ToList();
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();

        enabled = true;
    }

    public void TryAgain()
    {
        ClearBoard();

        enabled = true;
    }

    public void OptionMenu()
    {
        SceneManager.LoadScene(0);
        gameManager.wordLength = 0;
    }

    private void SetRandomWord()
    {
        string pickedWord="";
        while(pickedWord.Length != gameManager.wordLength)
        {
            pickedWord = solutions[Random.Range(0, solutions.Length)];
            pickedWord = pickedWord.ToLower().Trim();
            if(pickedWord.Length == gameManager.wordLength)
            {
                word = pickedWord;
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        tryAgainButton.SetActive(true);
        newWordButton.SetActive(true);
    }

    private void Update()
    {
        Row currentRow = rows[rowIndex];
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (columnIndex > 0)
            {
                columnIndex--;
                Tile tile = currentRow.tiles[columnIndex];
                tile.SetLetter('\0');
                tile.SetState(emptyState);

                invalidWordText.SetActive(false);
            }
        }
        else if (columnIndex < gameManager.wordLength)
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                {
                    currentRow.tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.tiles[columnIndex].SetState(occupiedState);

                    columnIndex++;
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && columnIndex == gameManager.wordLength)
        {
            StringBuilder enteredWord = new StringBuilder();
            foreach (Tile tile in currentRow.tiles)
            {
                enteredWord.Append(tile.letter);
            }
            SubmitRow(currentRow);
        }
    }

    private void SubmitRow(Row row)
    {
        Row currentRow = rows[rowIndex];
        if (!IsValidWord(row.word))
        {
            invalidWordText.SetActive(true);
            return;
        }

        string remaining = word;

        // check correct/incorrect letters first
        for (int i = 0; i < gameManager.wordLength; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.letter == word[i])
            {
                tile.SetState(correctState);

                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.letter))
            {
                tile.SetState(incorrectState);
            }
        }

        //then check wrong spots
        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.state != correctState && tile.state != incorrectState)
            {
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);

                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if (HasWon(row)) {
            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length) {
            enabled = false;
        }
    }

    private bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Count; i++)
        {
            if (validWords[i].ToLower().Trim().Equals(word))
            {
                return true;
            }
        }
        return false;
    }

    private bool HasWon(Row row)
    {
        for (int i = 0; i < row.tiles.Length; i++)
        {
            if (row.tiles[i].state != correctState) {
                return false;
            }
        }

        return true;
    }

    private void ClearBoard()
    {
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter('\0');
                rows[row].tiles[col].SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
    }
}