using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/**
 * UI Improvements for better usability
 * Manages contextual menu improvements, progress tracking, and visual feedback
 */
public class UIEnhancements : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI mainText;                    // Main instruction text
    public TextMeshProUGUI progressText;               // Progress counter
    public TextMeshProUGUI taskStatusText;            // Individual task status
    
    [Header("Task References")]
    public DrawerTask drawerA;
    public DrawerTask drawerB;
    public CleaningTask cleaningTask;
    public TrashBinScorer trashTask;
    public CoffeeTask coffeeTask;
    
    [Header("Game Control")]
    public GameRunController runController;
    
    [Header("Visual Feedback")]
    public Image progressBar;                           // Visual progress bar
    public Button[] taskButtons;                        // Task reset buttons
    
    private bool started = false;
    private Coroutine progressUpdateCoroutine;
    
    void Start()
    {
        // Initialize UI state
        UpdateInstructionText();
        if (progressBar != null) progressBar.fillAmount = 0f;
        if (taskStatusText != null) taskStatusText.text = "";
        
        StartCoroutine(UpdateProgressContinuously());
    }
    
    void UpdateInstructionText()
    {
        if (mainText == null) return;
        
        string instructions = @"<b>A TERRIBLE DAY IN THE OFFICE</b>

<b>Task Instructions:</b>
1. <color=green>Drawers:</color> File the correct documents in drawers A & B (4 files each)
2. <color=orange>Cleaning:</color> Clean all 5 marked zones with the sponge
3. <color=blue>Coffee:</color> Pour coffee from the pot into the cup
4. <color=red>Trash:</color> Throw 2 items (can & bottle) into the wastebasket

<b>Trash Items:</b> Green can and blue bottle (throw 2 items into the wastebasket)
<b>Goal:</b> Complete all tasks as quickly as possible!

<b>Click START to begin</b>";

        mainText.text =instructions;
    }
    
    IEnumerator UpdateProgressContinuously()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateProgress();
        }
    }
    
    void UpdateProgress()
    {
        if (!started) 
        {
            if (progressText != null) progressText.text = "Ready to start";
            if (taskStatusText != null) taskStatusText.text = "";
            return;
        }
        
        // Count completed tasks
        int completed = 0;
        int total = 5;
        
        string statusText = "<b>TASK PROGRESS:</b>\n";
        
        if (drawerA && drawerA.IsComplete) { completed++; statusText += "✅ Drawer A\n"; }
        else statusText += "⏳ Drawer A\n";
        
        if (drawerB && drawerB.IsComplete) { completed++; statusText += "✅ Drawer B\n"; }
        else statusText += "⏳ Drawer B\n";
        
        if (cleaningTask && cleaningTask.IsComplete) { completed++; statusText += "✅ Cleaning\n"; }
        else statusText += "⏳ Cleaning\n";
        
        if (coffeeTask && coffeeTask.IsComplete) { completed++; statusText += "✅ Coffee\n"; }
        else statusText += "⏳ Coffee\n";
        
        if (trashTask && trashTask.IsComplete) { completed++; statusText += "✅ Trash\n"; }
        else statusText += "⏳ Trash\n";
        
        // Update progress text
        if (progressText != null) 
            progressText.text = $"Progress: {completed}/{total} Tasks Complete";
            
        if (taskStatusText != null)
            taskStatusText.text = statusText;
            
        // Update progress bar
        if (progressBar != null)
            progressBar.fillAmount = (float)completed / total;
        
        // Update task button colors
        UpdateButtonStates();
    }
    
    void UpdateButtonStates()
    {
        if (taskButtons == null) return;
        
        Color completedColor = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green
        Color pendingColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);    // Gray
        
        // Update drawer buttons
        if (taskButtons.Length > 0 && drawerA != null)
            taskButtons[0].gameObject.GetComponent<Image>().color = drawerA.IsComplete ? completedColor : pendingColor;
            
        if (taskButtons.Length > 1 && drawerB != null)
            taskButtons[1].gameObject.GetComponent<Image>().color = drawerB.IsComplete ? completedColor : pendingColor;
    }
    
    public void OnStartButtonPressed()
    {
        if (runController != null && !runController.Started)
        {
            started = true;
            if (mainText != null)
            {
                mainText.text = "<b>TASKS IN PROGRESS</b>\nCheck progress on your right!\nGood luck! 🎯";
            }
            
            StartCoroutine(ShowBriefInstructions());
        }
    }
    
    IEnumerator ShowBriefInstructions()
    {
        yield return new WaitForSeconds(3f);
        
        string briefGuide = @"<b>Quick Reference:</b>
• <color=green>Drawers:</color> File docs in colored drawers
• <color=orange>Cleaning:</color> Touch zones with sponge  
• <color=blue>Coffee:</color> Pour into cup at steep angle
• <color=red>Trash:</color> Throw from distance with speed

Check your progress panel! 📊";
        
        if (mainText != null) mainText.text = briefGuide;
    }
    
    // Public methods for reset buttons
    public void OnDrawerAReset() { Debug.Log("Drawer A Reset"); }
    public void OnDrawerBReset() { Debug.Log("Drawer B Reset"); }
    public void OnCleaningReset() { Debug.Log("Cleaning Reset"); }
    public void OnCoffeeReset() { Debug.Log("Coffee Reset"); }
    public void OnTrashReset() { Debug.Log("Trash Reset"); }
}
