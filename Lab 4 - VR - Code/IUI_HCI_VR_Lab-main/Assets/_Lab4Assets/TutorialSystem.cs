using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI tutorialText;
    public GameObject[] highlightTargets;  // Objects to highlight during tutorial
    public GameObject tutorialPanel;
    
    [Header("Tutorial Steps")]
    public TutorialStep[] tutorialSteps;
    
    [Header("Settings")]
    public float highlightDuration = 2f;
    public Color highlightColor = Color.yellow;
    public float textFadeTime = 0.5f;
    
    private int currentStep = 0;
    private Camera vrCamera;
    
    [System.Serializable]
    public class TutorialStep
    {
        public string stepDescription;
        public GameObject targetObject;
        public bool autoAdvance;
        public float duration;
    }
    
    void Start()
    {
        vrCamera = Camera.current ?? FindObjectOfType<Camera>();
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
    
    public void StartTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        currentStep = 0;
        ShowStep(0);
    }
    
    public void ShowStep(int stepIndex)
    {
        if (stepIndex >= tutorialSteps.Length) return;
        
        var step = tutorialSteps[stepIndex];
        if (tutorialText != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeText(step.stepDescription));
        }
        
        if (step.targetObject != null)
        {
            StartCoroutine(HighlightObject(step.targetObject));
        }
        
        if (step.autoAdvance)
        {
            Invoke("NextStep", step.duration);
        }
    }
    
    public void NextStep()
    {
        currentStep++;
        if (currentStep < tutorialSteps.Length)
        {
            ShowStep(currentStep);
        }
        else
        {
            EndTutorial();
        }
    }
    
    public void EndTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        // Hide all highlights
        foreach (var obj in highlightTargets)
        {
            if (obj != null) UnhighlightObject(obj);
        }
    }
    
    IEnumerator FadeText(string newText)
    {
        if (tutorialText == null) yield break;
        
        // Fade out
        Color color = tutorialText.color;
        for (float t = 0; t < textFadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / textFadeTime);
            tutorialText.color = color;
            yield return null;
        }
        
        // Change text
        tutorialText.text = newText;
        
        // Fade in
        for (float t = 0; t < textFadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / textFadeTime);
            tutorialText.color = color;
            yield return null;
        }
        
        tutorialText.color = new Color(color.r, color.g, color.b, 1f);
    }
    
    IEnumerator HighlightObject(GameObject obj)
    {
        if (obj == null) yield break;
        
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            // Look for renderer in children
            renderer = obj.GetComponentInChildren<Renderer>();
        }
        
        if (renderer != null)
        {
            var originalMaterial = renderer.material;
            var highlightMaterial = new Material(originalMaterial);
            highlightMaterial.color = highlightColor;
            renderer.material = highlightMaterial;
            
            yield return new WaitForSeconds(highlightDuration);
            
            renderer.material = originalMaterial;
        }
    }
    
    void UnhighlightObject(GameObject obj)
    {
        // Remove any highlight effects
        if (obj != null)
        {
            var renderer = obj.GetComponent<Renderer>() ?? obj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                // Restore original appearance
                renderer.material.color = Color.white;
            }
        }
    }
}
