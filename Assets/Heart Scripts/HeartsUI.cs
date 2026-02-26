using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Images (size should equal heartsToWin)")]
    public Image[] heartSlots;

    [Header("Sprites")]
    public Sprite emptyHeart;
    public Sprite fullHeart;

    public void SetHearts(int collected)
    {
        if (heartSlots == null) return;

        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] == null) continue;

            heartSlots[i].sprite = (i < collected) ? fullHeart : emptyHeart;
        }
    }
}
