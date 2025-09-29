using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/**
 * Система помощи и восстановления после ошибок пользователя
 */
public class HelpSystem : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject helpPanel;
    public TextMeshProUGUI helpText;
    public Button[] helpButtons;
    
    [Header("Task Help Content")]
    public string[] helpTopics = {
        "Drawers Help": "File documents by dragging to correct color-coded drawers. Green files → Green drawer, Light Green files → Light Green drawer",
        "Cleaning Help": "Use sponge to clean all 5 marked zones. Zones will glow green when cleaned successfully.",
        "Coffee Help": "Pour coffee by tilting the pot at realistic angle (35-55 degrees) towards the cup until filled.",
        "Trash Help": "Throw green can and blue bottle into wastebasket from distance (1+ meter) with good speed (1.2+ m/s)."
    };
    
    [Header("Error Recovery")]
    public AudioClip errorSound;
    public AudioClip helpSound;
    public float errorIndicatorDuration = 3f;
    
    private bool isHelpActive = false;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (helpPanel != null) helpPanel.SetActive(false);
        SetupHelpButtons();
    }
    
    void SetupHelpButtons()
    {
        if (helpButtons != null && helpTopics.Length > 0)
        {
            for (int i = 0; i < helpButtons.Length && i < helpTopics.Length; i++)
            {
                int topicIndex = i;
                helpButtons[i].onClick.AddListener(() => ShowHelpTopic(helpTopics[topicIndex]));
            }
        }
    }
    
    public void ToggleHelp()
    {
        isHelpActive = !isHelpActive;
        if (helpPanel != null)
        {
            helpPanel.SetActive(isHelpActive);
        }
        
        if (isHelpActive && audioSource && helpSound)
        {
            audioSource.PlayOneShot(helpSound);
        }
    }
    
    public void ShowHelpTopic(string topicInfo)
    {
        if (helpText != null)
        {
            var parts = topicInfo.Split(new char[] { ':' }, 2);
            if (parts.Length == 2)
            {
                helpText.text = $"<b>{parts[0]}:</b>\n{parts[1]}";
            }
            else
            {
                helpText.text = topicInfo;
            }
        }
    }
    
    public void ShowErrorRecovery(string errorMessage, string solution)
    {
        StartCoroutine(ErrorRecoverySequence(errorMessage, solution));
    }
    
    IEnumerator ErrorRecoverySequence(string error, string solution)
    {
        // Play error sound
        if (audioSource && errorSound)
        {
            audioSource.PlayOneShot(errorSound);
        }
        
        // Show error and solution
        if (helpPanel != null) helpPanel.SetActive(true);
        if (helpText != null)
        {
            helpText.text = $"<color=red>⚠️ Error:</color> {error}\n\n<color=green>💡 Solution:</color> {solution}";
        }
        
        yield return new WaitForSeconds(errorIndicatorDuration);
        
        // Auto-hide after duration
        if (helpPanel != null && !isHelpActive)
        {
            helpPanel.SetActive(false);
        }
    }
    
    [ContextMenu("Test Error Recovery")]
    public void TestErrorRecovery()
    {
        ShowErrorRecovery("Wrong file type inserted", "File types must match drawer color: Green files in green drawer, Light Green files in light green drawer");
    }
    
    [ContextMenu("Test Quick Help")]
    public void TestQuickHelp()
    {
        ShowHelpTopic("Quick Tips: Each task has visual feedback. Look for colored indicators and sound effects that guide you through the process.");
    }
}
