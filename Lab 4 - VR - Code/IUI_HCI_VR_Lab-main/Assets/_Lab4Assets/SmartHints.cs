using UnityEngine;
using TMPro;
using System.Collections;

public class SmartHints : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI hintsText;
    public GameObject hintPanel;
    
    [Header("Hint Settings")]
    public float hintDelay = 3f;          // Seconds before showing hint
    public float hintDuration = 5f;       // How long hint stays active
    public bool adaptiveHints = true;     // Enable smart hinting
    
    [Header("User Behavior Tracking")]
    public float[] taskTimes = new float[5];       // Time spent on each task
    public int[] userErrors = new int[5];         // Errors per task
    public string[] previousSteps = new string[10]; // Last user actions
    
    [Header("Hint Content")]
    public HintScenario[] hintScenarios;
    
    [System.Serializable]
    public class HintScenario
    {
        public string triggerCondition;  // e.g., "drawers_slow", "coffee_angle", "trash_distance"
        public string hintMessage;
        public int priority;              // Higher = more urgent
        public bool showOnceOnly = true;
        public bool alreadyShown = false;
    }
    
    private Coroutine currentHintCoroutine;
    private float[] taskStartTimes = new float[5];
    private bool[] tasksStarted = new bool[5];
    
    void Start()
    {
        InitializeHintScenarios();
        if (hintPanel != null) hintPanel.SetActive(false);
    }
    
    void InitializeHintScenarios()
    {
        if (hintScenarios == null || hintScenarios.Length == 0)
        {
            hintScenarios = new HintScenario[]
            {
                new HintScenario
                {
                    triggerCondition = "drawers_slow",
                    hintMessage = "💡 Hint: Drawer colors match file colors - look at the drawer front colors!",
                    priority = 3
                },
                new HintScenario
                {
                    triggerCondition = "coffee_angle",
                    hintMessage = "💡 Hint: Pour coffee like real life - tip the pot steeply (35-55° angle) toward the cup!",
                    priority = 4
                },
                new HintScenario
                {
                    triggerCondition = "trash_distance",
                    hintMessage = "💡 Hint: Throw items from a distance with quick motion - the system needs speed and distance!",
                    priority = 4
                },
                new HintScenario
                {
                    triggerCondition = "cleaning_location",
                    hintMessage = "💡 Hint: All 5 cleaning zones are marked - the sponge will make a sound when you touch them!",
                    priority = 2
                },
                new HintScenario
                {
                    triggerCondition = "general_struggle",
                    hintMessage = "💡 Hint: Pay attention to visual and audio feedback - they guide you to success!",
                    priority = 1
                }
            };
        }
    }
    
    public void RecordTaskStart(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < 5)
        {
            taskStartTimes[taskIndex] = Time.time;
            tasksStarted[taskIndex] = true;
        }
    }
    
    public void RecordTaskProgress(int taskIndex, bool completed, bool error = false)
    {
        if (!adaptiveHints) return;
        
        if (taskIndex >= 0 && taskIndex < 5 && tasksStarted[taskIndex])
        {
            float timeSpent = Time.time - taskStartTimes[taskIndex];
            taskTimes[taskIndex] = timeSpent;
            
            if (error) userErrors[taskIndex]++;
            
            // Check for hint triggers after a delay
            if (!completed)
            {
                StopCurrentHint();
                currentHintCoroutine = StartCoroutine(CheckHintTriggers(taskIndex, timeSpent));
            }
        }
    }
    
    IEnumerator CheckHintTriggers(int taskIndex, float timeSpent)
    {
        // Wait for user to potentially figure it out themselves
        yield return new WaitForSeconds(hintDelay);
        
        // Analyze user behavior patterns
        string bestHint = AnalyzeAndGetBestHint(taskIndex, timeSpent);
        
        if (!string.IsNullOrEmpty(bestHint))
        {
            ShowHint(bestHint);
        }
    }
    
    string AnalyzeAndGetBestHint(int taskIndex, float timeSpent)
    {
        HintScenario bestHint = null;
        
        foreach (var scenario in hintScenarios)
        {
            // Skip if already shown and it's show-once-only
            if (scenario.alreadyShown && scenario.showOnceOnly) continue;
            
            bool shouldShow = false;
            
            switch (scenario.triggerCondition)
            {
                case "drawers_slow":
                    shouldShow = (taskIndex == 0 || taskIndex == 1) && timeSpent > 30f;
                    break;
                case "coffee_angle":
                    shouldShow = taskIndex == 3 && timeSpent > 45f && userErrors[taskIndex] > 1;
                    break;
                case "trash_distance":
                    shouldShow = taskIndex == 4 && userErrors[taskIndex] > 2;
                    break;
                case "cleaning_location":
                    shouldShow = taskIndex == 2 && timeSpent > 20f && userErrors[taskIndex] > 0;
                    break;
                case "general_struggle":
                    int totalErrors = 0;
                    for (int i = 0; i < 5; i++) totalErrors += userErrors[i];
                    shouldShow = totalErrors > 5;
                    break;
            }
            
            // Select highest priority valid hint
            if (shouldShow && (bestHint == null || scenario.priority > bestHint.priority))
            {
                bestHint = scenario;
            }
        }
        
        if (bestHint != null)
        {
            bestHint.alreadyShown = true;
            return bestHint.hintMessage;
        }
        
        return null;
    }
    
    void ShowHint(string message)
    {
        StopCurrentHint();
        
        if (hintsText != null)
        {
            hintsText.text = message;
        }
        
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            currentHintCoroutine = StartCoroutine(AutoHideHint());
        }
    }
    
    IEnumerator AutoHideHint()
    {
        yield return new WaitForSeconds(hintDuration);
        
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }
    
    void StopCurrentHint()
    {
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
            currentHintCoroutine = null;
        }
    }
    
    public void HideHint()
    {
        StopCurrentHint();
        if (hintPanel != null) hintPanel.SetActive(false);
    }
    
    [ContextMenu("Test Drawer Hint")]
    public void TestDrawerHint()
    {
        ShowHint("💡 TEST: This is how drawer hints appear to users!");
    }
    
    [ContextMenu("Simulate Slow Drawers")]
    public void SimulateSlowDrawers()
    {
        RecordTaskStart(0); // Drawer A
        StartCoroutine(SimulateSlowTask(0, 35f, 2)); // 35 seconds, 2 errors
    }
    
    IEnumerator SimulateSlowTask(int taskIndex, float duration, int errors)
    {
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < errors; i++)
        {
            RecordTaskProgress(taskIndex, false, true);
            yield return new WaitForSeconds(duration / errors);
        }
        
        RecordTaskProgress(taskIndex, false); // Still not completed
    }
}
