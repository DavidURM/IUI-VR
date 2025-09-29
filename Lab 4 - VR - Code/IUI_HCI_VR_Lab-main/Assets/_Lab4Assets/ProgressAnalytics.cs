using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ProgressAnalytics : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI detailedProgressText;
    public Slider overallProgressSlider;
    public TextMeshProUGUI timerDisplay;
    
    [Header("Task Tracking")]
    public TaskAnalytics[] taskAnalytics;
    
    [Header("Settings")]
    public bool showDetailedMetrics = true;
    public bool savePerformanceData = true;
    
    [System.Serializable]
    public class TaskAnalytics
    {
        public string taskName;
        public DateTime startTime;
        public DateTime completionTime;
        public float duration;
        public int attempts;
        public int errors;
        public bool isCompleted;
        public bool isInProgress;
        
        public TaskAnalytics(string name)
        {
            taskName = name;
            duration = 0f;
            attempts = 0;
            errors = 0;
            isCompleted = false;
            isInProgress = false;
        }
    }
    
    private DateTime sessionStartTime;
    private bool sessionActive = false;
    
    void Start()
    {
        sessionStartTime = DateTime.Now;
        InitializeTaskAnalytics();
    }
    
    void InitializeTaskAnalytics()
    {
        var taskNames = new string[] { "Drawer A", "Drawer B", "Cleaning", "Coffee", "Trash" };
        taskAnalytics = new TaskAnalytics[taskNames.Length];
        
        for (int i = 0; i < taskNames.Length; i++)
        {
            taskAnalytics[i] = new TaskAnalytics(taskNames[i]);
        }
    }
    
    public void StartSession()
    {
        sessionStartTime = DateTime.Now;
        sessionActive = true;
        Debug.Log($"Analytics session started at {sessionStartTime:HH:mm:ss}");
    }
    
    public void StartTask(string taskName)
    {
        var task = FindTask(taskName);
        if (task != null && !task.isInProgress && !task.isCompleted)
        {
            task.startTime = DateTime.Now;
            task.isInProgress = true;
            task.attempts++;
            Debug.Log($"Task '{taskName}' started at {task.startTime:HH:mm:ss}");
        }
    }
    
    public void CompleteTask(string taskName)
    {
        var task = FindTask(taskName);
        if (task != null && task.isInProgress)
        {
            task.completionTime = DateTime.Now;
            task.duration = (float)(task.completionTime - task.startTime).TotalSeconds;
            task.isCompleted = true;
            task.isInProgress = false;
            
            Debug.Log($"Task '{taskName}' completed in {task.duration:F2} seconds");
            UpdateUI();
        }
    }
    
    public void RecordError(string taskName, string errorMessage = "")
    {
        var task = FindTask(taskName);
        if (task != null)
        {
            task.errors++;
            Debug.LogWarning($"Error recorded for '{taskName}': {errorMessage}");
            UpdateUI();
        }
    }
    
    TaskAnalytics FindTask(string taskName)
    {
        foreach (var task in taskAnalytics)
        {
            if (task.taskName.Equals(taskName, StringComparison.OrdinalIgnoreCase))
                return task;
        }
    
        return null;
    }
    
    void UpdateUI()
    {
        if (!showDetailedMetrics) return;
        
        UpdateDetailedProgress();
        UpdateOverallProgress();
        UpdateTimerDisplay();
    }
    
    void UpdateDetailedProgress()
    {
        if (detailedProgressText == null) return;
        
        string progressDetails = "<b>📊 DETAILED PROGRESS:</b>\n\n";
        
        foreach (var task in taskAnalytics)
        {
            string status = "";
            if (task.isCompleted)
            {
                status = $"✅ {task.duration:F1}s ({task.attempts} attempts, {task.errors} errors)";
            }
            else if (task.isInProgress)
            {
                float currentDuration = (float)(DateTime.Now - task.startTime).TotalSeconds;
                status = $"🔄 {currentDuration:F1}s... ({task.errors} errors)";
            }
            else
            {
                status = "⏳ Not started";
            }
            
            progressDetails += $"<color=#777>{task.taskName}:</color> {status}\n";
        }
        
        // Add session stats
        int completedTasks = 0;
        int totalAttempts = 0;
        int totalErrors = 0;
        
        foreach (var task in taskAnalytics)
        {
            if (task.isCompleted) completedTasks++;
            totalAttempts += task.attempts;
            totalErrors += task.errors;
        }
        
        progressDetails += $"\n<b>Session Total:</b> {completedTasks}/5 tasks, {totalAttempts} attempts, {totalErrors} errors";
        
        detailedProgressText.text = progressDetails;
    }
    
    void UpdateOverallProgress()
    {
        if (overallProgressSlider == null) return;
        
        int completedTasks = 0;
        foreach (var task in taskAnalytics)
        {
            if (task.isCompleted) completedTasks++;
        }
        
        overallProgressSlider.value = (float)completedTasks / taskAnalytics.Length;
    }
    
    void UpdateTimerDisplay()
    {
        if (timerDisplay == null) return;
        
        if (sessionActive)
        {
            var elapsed = DateTime.Now - sessionStartTime;
            timerDisplay.text = $"⏱️ Session: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }
    }
    
    [ContextMenu("Generate Summary Report")]
    public void GenerateSummaryReport()
    {
        string report = "📈 PERFORMANCE SUMMARY REPORT\n\n";
        
        foreach (var task in taskAnalytics)
        {
            report += $"🗂️ {task.taskName}:\n";
            report += $"   Status: {(task.isCompleted ? "COMPLETED" : task.isInProgress ? "IN PROGRESS" : "NOT STARTED")}\n";
            
            if (task.isCompleted)
            {
                report += $"   Duration: {task.duration:F2} seconds\n";
                report += $"   Rating: {GetTaskRating(task.duration, task.errors)}\n";
            }
            
            report += $"   Attempts: {task.attempts}\n";
            report += $"   Errors: {task.errors}\n\n";
        }
        
        Debug.Log(report);
        
        if (savePerformanceData)
        {
            SavePerformanceData();
        }
    }
    
    string GetTaskRating(float duration, int errors)
    {
        if (errors == 0)
        {
            if (duration < 30f) return "⭐⭐⭐⭐⭐ Excellent";
            if (duration < 60f) return "⭐⭐⭐⭐ Very Good";
            if (duration < 120f) return "⭐⭐⭐ Good";
            return "⭐⭐ Fair";
        }
        
        if (errors <= 2) return "⭐⭐⭐ Good";
        if (errors <= 5) return "⭐⭐ Fair";
        return "⭐ Needs Practice";
    }
    
    void SavePerformanceData()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new { taskAnalytics, sessionStartTime, sessionActive }, true);
            string path = System.IO.Path.Combine(Application.persistentDataPath, "performance_data.json");
            System.IO.File.WriteAllText(path, jsonData);
            Debug.Log($"Performance data saved to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save performance data: {e.Message}");
        }
    }
    
    void Update()
    {
        if (sessionActive)
        {
            UpdateTimerDisplay();
        }
    }
}
