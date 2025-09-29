using UnityEngine;
using UnityEngine.UI;

/**
 * Component to manage UI button spacing and layout improvements
 * Add this to UI Root or Button container to fix overlapping issues
 */
public class UIButtonSpacer : MonoBehaviour
{
    [Header("Button References")]
    public Button[] resetButtons = new Button[4]; // DrawerA, DrawerB, Cleaning, Coffee, Trash
    
    [Header("Layout Settings")]
    public float buttonSpacing = 50f;      // Space between buttons
    public Vector2 buttonSize = new Vector2(120f, 40f);  // Button dimensions
    public string[] buttonLabels = new string[] { "Drawer A", "Drawer B", "Cleaning", "Coffee", "Trash" };
    
    [Header("Positioning")]
    public Vector3 startPosition = new Vector3(-150f, 100f, 0f);  // Starting position for first button
    public bool arrangeHorizontally = true;     // true = horizontal layout, false = vertical
    
    void Start()
    {
        ArrangeButtons();
    }
    
    [ContextMenu("Arrange Buttons")]
    public void ArrangeButtons()
    {
        if (resetButtons == null || resetButtons.Length == 0)
        {
            Debug.LogWarning("UIButtonSpacer: No buttons assigned!");
            return;
        }
        
        Vector3 currentPos = startPosition;
        Vector3 offset = arrangeHorizontally ? new Vector3(buttonSpacing, 0, 0) : new Vector3(0, -buttonSpacing, 0);
        
        for (int i = 0; i < resetButtons.Length; i++)
        {
            if (resetButtons[i] != null)
            {
                // Set position
                var rectTransform = resetButtons[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = currentPos;
                    rectTransform.sizeDelta = buttonSize;
                }
                
                // Set label text if available
                var text = resetButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null && i < buttonLabels.Length)
                {
                    text.text = buttonLabels[i];
                }
                
                // Visual improvements
                var image = resetButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    // Make buttons more visible
                    var colors = resetButtons[i].colors;
                    colors.normalColor = new Color(0.8f, 0.8f, 0.8f, 0.9f);
                    colors.normalColor = Color.white;
                    resetButtons[i].colors = colors;
                }
                
                currentPos += offset;
            }
        }
        
        Debug.Log($"UIButtonSpacer: Arranged {resetButtons.Length} buttons");
    }
    
    // Method to reset button positions to original
    [ContextMenu("Reset Button Positions")]
    public void ResetPositions()
    {
        // This would restore original positions if saved
        Debug.Log("UIButtonSpacer: Button positions reset");
    }
}
