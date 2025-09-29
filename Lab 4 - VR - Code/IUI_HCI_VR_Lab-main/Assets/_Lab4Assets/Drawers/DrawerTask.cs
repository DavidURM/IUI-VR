using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

// You can change this file
public class DrawerTask : MonoBehaviour
{ 
    [Header(" DO NOT CHANGE ANY PARAMETERS HERE ")]
    [Header("Setup")]
    public string taskName = "Drawers";
    public FileType expectedType = FileType.Green;
    public XRSocketInteractor[] sockets; // drag from inspector
    public int requiredCount = 4; // should normally be equal to sockets.Length
    
    [Header("Visual Feedback")]
    public Renderer drawerFrontRenderer; // Reference to drawer front for color indication
    
    [Header("Materials")]
    public Material greenFileMaterial;     // Green file material
    public Material lightGreenFileMaterial; // Light green file material

    [Header("Metrics (persist across resets)")]
    [SerializeField] private int totalInserts;   // every time something is placed in any socket
    [SerializeField] private int wrongInserts;   // placed item != expectedType
    public float ErrorRate => totalInserts > 0 ? (float)wrongInserts / totalInserts : 0f; //note: not used in final version

    public bool IsComplete { get; private set; }

    // DO NOT CHANGE THIS METHOD
    void OnEnable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.AddListener(OnSocketEntered);
            s.selectExited.AddListener(OnSocketExited);
        }

        Recompute();
        SetupDrawerAppearance();
    }

    // DO NOT CHANGE THIS METHOD
    void OnDisable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.RemoveListener(OnSocketEntered);
            s.selectExited.RemoveListener(OnSocketExited);
        }
    }
    
    //called when something entered a socket
    void OnSocketEntered(SelectEnterEventArgs args)
    {
        // Count attempt
        totalInserts++;

        // we get the the selected item and check whether its what we expected
        var selected = args.interactableObject?.transform;
        if (selected)
        {
            var fi = selected.GetComponent<FileItem>();
            if (!fi || fi.fileType != expectedType)
            {
                wrongInserts++;
                // Visual feedback for wrong insertion
                StartCoroutine(FlashDrawerWrong());
            }
            else
            {
                // Visual feedback for correct insertion
                StartCoroutine(FlashDrawerCorrect());
            }
        }

        Recompute();
    }

    // called when something exited a socket
    void OnSocketExited(SelectExitEventArgs _) => Recompute();

    // this method is called when a new item has entered or exited a socket
    void Recompute()
    {
        int matched = 0;

        // for each socket, check if we currently have a correct item in it
        foreach (var s in sockets)
        {   
            // always check whether the socket is initialized
            if (s == null) continue;
            var selected = s.firstInteractableSelected;
            if (selected == null) continue;

            // check if the item in the socket is of the expected filetype
            var fi = selected.transform.GetComponent<FileItem>();
            if (fi && fi.fileType == expectedType)
                matched++;
        }

        bool nowComplete = matched >= requiredCount;

        // If the task was not yet complete, print message
        if (nowComplete && !IsComplete)
        {
            IsComplete = true;
            Debug.Log($"{taskName}: COMPLETED ({matched}/{requiredCount}) | errorRate={ErrorRate:P1}");
        }
        // If task was complete before, and not anymore
        else if (!nowComplete && IsComplete)
        {
            IsComplete = false;
            Debug.Log($"{taskName}: no longer complete ({matched}/{requiredCount})");
        }
    }

    // --- Reset pattern ---
    // Called by DrawerResetController. Clears progress, NOT metrics.
    // If you change this behaviour, ensure to keep the current lines
    public void ResetState()
    {
        IsComplete = false;
        Recompute(); // will recompute from empty sockets after we clear them in the controller
    }

    // This is called when finishing the task to export numbers (not used in final version)
    public (int total, int wrong, float rate) GetErrorMetrics()
        => (totalInserts, wrongInserts, ErrorRate);
        
    // Setup drawer appearance to indicate expected file type
    private void SetupDrawerAppearance()
    {
        if (drawerFrontRenderer == null) return;
        
        Material materialToApply = null;
        
        switch (expectedType)
        {
            case FileType.Green:
                materialToApply = greenFileMaterial;
                break;
            case FileType.LightGreen:
                materialToApply = lightGreenFileMaterial;
                break;
            default:
                return; // Don't change if no specific type
        }
        
        if (materialToApply != null)
        {
            drawerFrontRenderer.material = materialToApply;
        }
        else
        {
            // Fallback to color tinting if materials not assigned
            var material = drawerFrontRenderer.material;
            Color drawerColor;
            
            switch (expectedType)
            {
                case FileType.Green:
                    drawerColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
                    break;
                case FileType.LightGreen:
                    drawerColor = new Color(0.4f, 0.9f, 0.4f, 0.7f);
                    break;
                default:
                    drawerColor = Color.white;
                    break;
            }
            
            material.color = drawerColor;
        }
    }
    
    // Visual feedback for correct insertion
    private IEnumerator FlashDrawerCorrect()
    {
        if (drawerFrontRenderer == null) yield break;
        
        var renderer = drawerFrontRenderer;
        var originalColor = renderer.material.color;
        var flashColor = Color.green;
        
        renderer.material.color = flashColor;
        yield return new WaitForSeconds(0.2f);
        renderer.material.color = originalColor;
    }
    
    // Visual feedback for wrong insertion
    private IEnumerator FlashDrawerWrong()
    {
        if (drawerFrontRenderer == null) yield break;
        
        var renderer = drawerFrontRenderer;
        var originalColor = renderer.material.color;
        var flashColor = Color.red;
        
        for (int i = 0; i < 3; i++)
        {
            renderer.material.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
