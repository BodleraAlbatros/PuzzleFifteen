using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlidingPuzzle : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject tilePrefab;
    public RectTransform boardParent;
    public int size = 3;

    [Header("UI")]
    public Button RestartButton;
    public Button Button8Tile;
    public Button Button15Tile;
    public TMP_Text MoveText;
    public TMP_Text WinText;

    private List<Tile> tiles = new List<Tile>();
    private Vector2 emptyPosition;

    private float tileSize;
    private float startX;
    private float startY;

    private int moves = 0;
    private bool isShuffling = false;
    private bool gameWon = false;

    void Start()
    {
        if (RestartButton != null)
            RestartButton.onClick.AddListener(RestartGame);

        if (Button8Tile != null)
            Button8Tile.onClick.AddListener(() => SetGridSize(3));

        if (Button15Tile != null)
            Button15Tile.onClick.AddListener(() => SetGridSize(4));

        StartGame();
    }

    void StartGame()
    {
        moves = 0;
        gameWon = false;

        if (WinText != null)
            WinText.gameObject.SetActive(false);

        UpdateMoveText();

        CalculateTileSize();
        CalculateStartPosition();
        GenerateBoard();
        Shuffle(100);
    }

    public void RestartGame()
    {
        foreach (Tile t in tiles)
            Destroy(t.gameObject);

        tiles.Clear();
        StartGame();
    }

    public void SetGridSize(int newSize)
    {
        size = newSize;
        RestartGame();
    }

    void CalculateTileSize()
    {
        // tileSize = ширина панели / количество плиток
        tileSize = Mathf.Min(boardParent.rect.width, boardParent.rect.height) / size;
    }

    void CalculateStartPosition()
    {
        startX = -(size - 1) * tileSize / 2f;
        startY = (size - 1) * tileSize / 2f;
    }

    void GenerateBoard()
    {
        int number = 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == size - 1 && y == size - 1)
                {
                    emptyPosition = new Vector2(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, boardParent);
                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(tileSize, tileSize); // 🟢 подстраиваем размер
                rect.anchoredPosition = new Vector2(
                    startX + x * tileSize,
                    startY - y * tileSize
                );

                Tile tile = obj.GetComponent<Tile>();
                tile.SetNumber(number);
                tile.correctPosition = new Vector2(x, y);
                tile.currentPosition = new Vector2(x, y);
                tile.puzzle = this;

                tiles.Add(tile);
                number++;
            }
        }
    }

    public void TryMove(Tile tile)
    {
        if (gameWon) return;

        if (Vector2.Distance(tile.currentPosition, emptyPosition) == 1)
        {
            MoveTile(tile);

            if (!isShuffling)
            {
                moves++;
                UpdateMoveText();

                if (CheckWin())
                {
                    gameWon = true;
                    if (WinText != null)
                        WinText.gameObject.SetActive(true);
                }
            }
        }
    }

    void MoveTile(Tile tile)
    {
        Vector2 oldPos = tile.currentPosition;
        tile.currentPosition = emptyPosition;
        emptyPosition = oldPos;

        RectTransform rect = tile.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(
            startX + tile.currentPosition.x * tileSize,
            startY - tile.currentPosition.y * tileSize
        );
    }

    void Shuffle(int shuffleMoves)
    {
        isShuffling = true;

        for (int i = 0; i < shuffleMoves; i++)
        {
            List<Tile> neighbors = GetNeighbors();
            Tile randomTile = neighbors[Random.Range(0, neighbors.Count)];
            MoveTile(randomTile);
        }

        isShuffling = false;
    }

    List<Tile> GetNeighbors()
    {
        List<Tile> result = new List<Tile>();
        foreach (Tile t in tiles)
            if (Vector2.Distance(t.currentPosition, emptyPosition) == 1)
                result.Add(t);
        return result;
    }

    bool CheckWin()
    {
        foreach (Tile tile in tiles)
            if (tile.currentPosition != tile.correctPosition)
                return false;
        return true;
    }

    void UpdateMoveText()
    {
        if (MoveText != null)
            MoveText.text = "Счет: " + moves.ToString();
    }
}