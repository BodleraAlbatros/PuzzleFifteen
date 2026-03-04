using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public SlidingPuzzle puzzle;
    public Button button8;
    public Button button15;

    void Start()
    {
        button8.onClick.AddListener(() => puzzle.SetLevel(3)); // 3x3
        button15.onClick.AddListener(() => puzzle.SetLevel(4)); // 4x4
    }
}