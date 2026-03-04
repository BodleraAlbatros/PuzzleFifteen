using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2 correctPosition;
    public Vector2 currentPosition;
    public SlidingPuzzle puzzle;

    private TMP_Text label;

    void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetNumber(int n)
    {
        label.text = n.ToString();
    }

    void OnClick()
    {
        puzzle.TryMove(this);
    }
}

