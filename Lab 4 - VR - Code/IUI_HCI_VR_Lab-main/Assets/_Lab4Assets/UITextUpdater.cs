using UnityEngine;
using TMPro;

/**
 * Simple component to update UI text with better trash task instructions
 * Add this to MainText object to override its text
 */
public class UITextUpdater : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI mainText;
    
    [Header("Trash Instructions")]
    public string trashInstructionsOverride = @"TRASH TASK INDICATOR

Throw these items into the wastebasket:
• Green can (Container_Can_Green)
• Blue bottle (Container_Bottle_Blue)

Minimum requirements:
• Throw from distance (1+ meter)
• Use sufficient speed (1.2+ m/s)
• Aim for downward trajectory

Score: Throw successfully to count!";

    void Start()
    {
        if (mainText != null)
        {
            UpdateText();
        }
    }
    
    public void UpdateText()
    {
        if (mainText != null)
        {
            // Append trash instructions to existing text
            string currentText = mainText.text;
            string newText = currentText + "\n\n" + trashInstructionsOverride;
            mainText.text = newText;
        }
    }
    
    // Call this to show trash instructions separately
    public void ShowTrashInstructions()
    {
        if (mainText != null)
        {
            mainText.text = trashInstructionsOverride;
        }
    }
}
