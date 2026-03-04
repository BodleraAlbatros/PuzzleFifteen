using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using SFB; // StandaloneFileBrowser

public class SlidingPuzzle : MonoBehaviour
{
    [Header("References")]
    public GameObject tilePrefab;
    public RectTransform boardParent;
    public GameObject winText;
    public TMP_Text movesText;
    public TMP_Text recordText;

    [Header("Settings")]
    public int size = 4;
    public float maxBoardSize = 400f;

    private List<Tile> tiles = new List<Tile>();
    private Vector2Int emptyPosition;
    private int moveCount;
    private float tileSize;

    void Start()
    {
        AdjustBoardSize();
        NewGame();
    }

    // ======================
    // Выбор уровня
    // ======================
    public void SetLevel(int gridSize)
    {
        size = gridSize;
        AdjustBoardSize();
        NewGame();
    }

    void AdjustBoardSize()
    {
        tileSize = maxBoardSize / size;
        if (boardParent != null)
            boardParent.sizeDelta = new Vector2(tileSize * size, tileSize * size);
    }

    public void NewGame()
    {
        moveCount = 0;
        UpdateMovesUI();
        GenerateBoard();
        Shuffle(100);

        if (winText != null) winText.SetActive(false);
        LoadRecord();
    }

    void GenerateBoard()
    {
        foreach (Transform child in boardParent)
            Destroy(child.gameObject);

        tiles.Clear();
        int number = 1;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == size - 1 && y == size - 1)
                {
                    emptyPosition = new Vector2Int(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, boardParent);
                RectTransform rect = obj.GetComponent<RectTransform>();

                Tile tile = obj.GetComponent<Tile>();
                tile.correctPosition = new Vector2Int(x, y);
                tile.currentPosition = new Vector2Int(x, y);
                tile.puzzle = this;

                rect.sizeDelta = new Vector2(tileSize, tileSize);
                rect.anchoredPosition = GetAnchoredPosition(tile.currentPosition);
                tile.SetNumber(number);

                tiles.Add(tile);
                number++;
            }
        }
    }

    Vector2 GetAnchoredPosition(Vector2Int gridPos)
    {
        return new Vector2(
            -(size - 1) * tileSize / 2f + gridPos.x * tileSize,
            (size - 1) * tileSize / 2f - gridPos.y * tileSize
        );
    }

    // ======================
    // Движение плитки
    // ======================
    public bool TryMove(Tile tile)
    {
        if (Vector2Int.Distance(tile.currentPosition, emptyPosition) == 1)
        {
            MoveTile(tile);
            return true;
        }
        return false;
    }

    void MoveTile(Tile tile)
    {
        Vector2Int oldPos = tile.currentPosition;
        tile.currentPosition = emptyPosition;
        emptyPosition = oldPos;

        tile.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPosition(tile.currentPosition);

        moveCount++;
        UpdateMovesUI();

        if (IsSolved())
        {
            if (winText != null) winText.SetActive(true);
            SaveRecord();
        }
    }

    void MoveTileInternal(Tile tile)
    {
        Vector2Int oldPos = tile.currentPosition;
        tile.currentPosition = emptyPosition;
        emptyPosition = oldPos;
        tile.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPosition(tile.currentPosition);
    }

    List<Tile> GetNeighbors()
    {
        List<Tile> result = new List<Tile>();
        foreach (Tile t in tiles)
            if (Vector2Int.Distance(t.currentPosition, emptyPosition) == 1)
                result.Add(t);
        return result;
    }

    bool IsSolved()
    {
        foreach (Tile t in tiles)
            if (t.currentPosition != t.correctPosition)
                return false;
        return true;
    }

    void UpdateMovesUI()
    {
        if (movesText != null)
            movesText.text = "Счет: " + moveCount;
    }

    // ======================
    // Рекорд
    // ======================
    void SaveRecord()
    {
        string key = "Record_" + size;
        int best = PlayerPrefs.GetInt(key, int.MaxValue);
        if (moveCount < best)
        {
            PlayerPrefs.SetInt(key, moveCount);
            PlayerPrefs.Save();
        }
        LoadRecord();
    }

    void LoadRecord()
    {
        string key = "Record_" + size;
        int best = PlayerPrefs.GetInt(key, 0);
        if (best > 0 && recordText != null)
            recordText.text = "Рекорд: " + best;
        else if (recordText != null)
            recordText.text = "Рекорд: -";
    }

    // ======================
    // Перемешивание
    // ======================
    void Shuffle(int moves)
    {
        for (int i = 0; i < moves; i++)
        {
            List<Tile> neighbors = GetNeighbors();
            Tile randomTile = neighbors[Random.Range(0, neighbors.Count)];
            MoveTileInternal(randomTile);
        }

        if (IsSolved())
            Shuffle(moves);

        moveCount = 0;
        UpdateMovesUI();
    }

    // ======================
    // Сохранение через диалог
    // ======================
    public void SaveToFileWithDialog()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save Puzzle", "", "puzzle_save", "json");
        if (!string.IsNullOrEmpty(path))
        {
            SaveToJson(path);
        }
    }

    void SaveToJson(string path)
    {
        PuzzleSaveData data = new PuzzleSaveData();
        data.size = size;
        data.moves = moveCount;
        data.emptyX = emptyPosition.x;
        data.emptyY = emptyPosition.y;

        foreach (Tile t in tiles)
        {
            data.tileX.Add(t.currentPosition.x);
            data.tileY.Add(t.currentPosition.y);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved: " + path);
    }

    // ======================
    // Загрузка через диалог
    // ======================
    public void LoadFromFileWithDialog()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Load Puzzle", "", "json", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            LoadFromJson(paths[0]);
        }
    }

    private void LoadFromJson(string path)
    {
        if (!File.Exists(path))
        {
            Debug.Log("Save not found");
            return;
        }

        string json = File.ReadAllText(path);
        PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);

        if (data.size != size || data.tileX.Count != tiles.Count)
        {
            Debug.Log("Save corrupted or size mismatch");
            return;
        }

        emptyPosition = new Vector2Int(data.emptyX, data.emptyY);
        moveCount = data.moves;

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile t = tiles[i];
            t.currentPosition = new Vector2Int(data.tileX[i], data.tileY[i]);
            t.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPosition(t.currentPosition);
        }

        UpdateMovesUI();
        LoadRecord();
        if (winText != null)
            winText.SetActive(false);
    }


}