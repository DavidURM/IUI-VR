using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashBinScorer : MonoBehaviour
{
    [Header("Setup")]
    public Transform rimCenter;     // point at the center/top of the bin opening
    [Tooltip("This object should have a Trigger collider (e.g., tall cylinder above bin).")]
    public Collider scoreZone;      // auto-filled by Reset if on same object
    
    [Header("Visual Feedback")]
    public TextMeshProUGUI scoreText;  // UI text to show current score
    public Renderer binRenderer;       // Bin renderer for visual feedback
    
    [Header("UI Enhancement")]
    public GameObject scoreIndicator;  // Optional score indicator panel
    public AudioClip successAudioClip; // Audio for successful scores
    public AudioClip failureAudioClip; // Audio for failed attempts

    [Header("Rules")]
    public float minReleaseSpeed = 1.2f;      // m/s; prevents “drop-ins”
    public float minReleaseDistance = 1.0f;   // meters from rim at release
    public float maxSecondsSinceRelease = 5f; // throw must be recent
    public bool requireDownwardEntry = true;  // must be moving downward when entering

    [Header("State")]
    public int score;
    public bool IsComplete { get; private set; }  // <-- new property

    private HashSet<TrashItemThrowData> counted = new();

    // DO NOT CHANGE
    void Reset() { scoreZone = GetComponent<Collider>(); }

    // called when something enters the scorecollider
    void OnTriggerEnter(Collider other)
    {   
        Debug.Log("Something entered the bin");
        // check what the other rigidbody is
        var rb = other.attachedRigidbody;
        if (!rb) return;

        var data = rb.GetComponent<TrashItemThrowData>();
        if (!data || counted.Contains(data)) return;
        
        float since = Time.time - data.releaseTime;
        float speed = data.releaseVel.magnitude;
        var center = rimCenter ? rimCenter.position : transform.position;
        float dist = Vector3.Distance(data.releasePos, center);
        bool downward = !requireDownwardEntry || Vector3.Dot(rb.linearVelocity.normalized, Vector3.down) > 0.2f;

        if (since <= maxSecondsSinceRelease && speed >= minReleaseSpeed && dist >= minReleaseDistance && downward)
        {
            score++;
            counted.Add(data);
            Debug.Log($"Trash: SCORE #{score} (speed {speed:F1}, dist {dist:F2}, t {since:F1}s)");
            UpdateScoreDisplay();
            StartCoroutine(FlashBinSuccess());
            PlaySuccessAudio();
        }
        else
        {
            Debug.Log($"Trash: rejected (speed {speed:F1}, dist {dist:F2}, t {since:F1}s, down {downward})");
            StartCoroutine(FlashBinFailure());
            PlayFailureAudio();
        }

        UpdateCompletion();
    }

    // check if the task is completed
    // Enhanced with visual feedback
    private void UpdateCompletion()
    {
        // True only if at least 2 valid scores AND nothing removed (still 2+ inside)
        bool wasComplete = IsComplete;
        IsComplete = score >= 2 && counted.Count > 1;
        
        if (IsComplete && !wasComplete)
        {
            Debug.Log("Trash task COMPLETE!");
            StartCoroutine(FlashBinComplete());
        }
        
        UpdateScoreDisplay();
    }
    
    // Update score display text
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Trash Score: {score}/2";
        }
    }
    
    // Visual feedback for successful throw
    private System.Collections.IEnumerator FlashBinSuccess()
    {
        if (binRenderer == null) yield break;
        
        var material = binRenderer.material;
        var originalColor = material.color;
        var successColor = Color.green;
        
        material.color = successColor;
        yield return new WaitForSeconds(0.3f);
        material.color = originalColor;
    }
    
    // Visual feedback for wrong throw
    private System.Collections.IEnumerator FlashBinFailure()
    {
        if (binRenderer == null) yield break;
        
        var material = binRenderer.material;
        var originalColor = material.color;
        var failColor = Color.red;
        
        for (int i = 0; i < 2; i++)
        {
            material.color = failColor;
            yield return new WaitForSeconds(0.1f);
            material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Visual feedback for task completion
    private System.Collections.IEnumerator FlashBinComplete()
    {
        if (binRenderer == null) yield break;
        
        var material = binRenderer.material;
        var originalColor = material.color;
        var completeColor = Color.yellow;
        
        for (int i = 0; i < 5; i++)
        {
            material.color = completeColor;
            yield return new WaitForSeconds(0.2f);
            material.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    // Audio feedback methods
    private void PlaySuccessAudio()
    {
        if (successAudioClip != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(successAudioClip);
            }
            else
            {
                // Fallback: create temporary audio source
                AudioSource.PlayClipAtPoint(successAudioClip, transform.position);
            }
        }
    }
    
    private void PlayFailureAudio()
    {
        if (failureAudioClip != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(failureAudioClip);
            }
            else
            {
                // Fallback: create temporary audio source
                AudioSource.PlayClipAtPoint(failureAudioClip, transform.position);
            }
        }
    }
}
