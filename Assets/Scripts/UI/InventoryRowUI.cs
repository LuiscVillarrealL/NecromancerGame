using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRowUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text countText;

    public Button move1Button;
    public Button move5Button;
    public Button moveAllButton;

    private Func<int> _getCount;

    public void Bind(
        ResourceType type,
        Func<int> getCount,
        Action move1,
        Action move5,
        Action moveAll,
        string labelSuffix = ""
    )
    {
        _getCount = getCount;

        if (nameText != null) nameText.text = type + labelSuffix;

        if (move1Button != null) { move1Button.onClick.RemoveAllListeners(); move1Button.onClick.AddListener(() => move1()); }
        if (move5Button != null) { move5Button.onClick.RemoveAllListeners(); move5Button.onClick.AddListener(() => move5()); }
        if (moveAllButton != null) { moveAllButton.onClick.RemoveAllListeners(); moveAllButton.onClick.AddListener(() => moveAll()); }

        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (countText != null && _getCount != null)
            countText.text = _getCount().ToString();
    }
}
