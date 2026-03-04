using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    public Vector2Int correctPosition;
    public Vector2Int currentPosition;
    public SlidingPuzzle puzzle;

    private TMP_Text label;

    void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetNumber(int n)
    {
        if (label != null)
            label.text = n.ToString();
    }

    void OnClick()
    {
        if (puzzle != null)
            puzzle.TryMove(this);
    }
}