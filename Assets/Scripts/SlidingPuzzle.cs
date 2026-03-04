using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidingPuzzle : MonoBehaviour
{
    public GameObject tilePrefab;
    public RectTransform boardParent;
    public int size = 3;
    public Button RestartButton; // кнопка перезапуска

    private List<Tile> tiles = new List<Tile>();
    private Vector2 emptyPosition;

    [HideInInspector] public float tileSize = 100f;
    [HideInInspector] public float startX;
    [HideInInspector] public float startY;

    void Start()
    {
        startX = -(size - 1) * tileSize / 2f;
        startY = (size - 1) * tileSize / 2f;

        // Привязываем кнопку Restart
        if (RestartButton != null)
            RestartButton.onClick.AddListener(RestartGame);

        GenerateBoard();
        Shuffle(100);
    }

    // Метод для кнопки Restart
    public void RestartGame()
    {
        // Удаляем старые плитки
        foreach (Tile t in tiles)
        {
            Destroy(t.gameObject);
        }
        tiles.Clear();

        // Создаём заново
        GenerateBoard();
        Shuffle(100);
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

    public bool TryMove(Tile tile)
    {
        if (Vector2.Distance(tile.currentPosition, emptyPosition) == 1)
        {
            MoveTile(tile);
            return true;
        }
        return false;
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

    void Shuffle(int moves)
    {
        for (int i = 0; i < moves; i++)
        {
            List<Tile> neighbors = GetNeighbors();
            Tile randomTile = neighbors[Random.Range(0, neighbors.Count)];
            MoveTile(randomTile);
        }
    }

    List<Tile> GetNeighbors()
    {
        List<Tile> result = new List<Tile>();

        foreach (Tile t in tiles)
        {
            if (Vector2.Distance(t.currentPosition, emptyPosition) == 1)
                result.Add(t);
        }

        return result;
    }
}
