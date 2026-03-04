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

    public void SetNumber(int number)
    {
        label.text = number.ToString();
    }

    void OnClick()
    {
        if (puzzle != null)
            puzzle.TryMove(this);
    }
}