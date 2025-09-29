using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRGrabInteractable
using System.Collections;

public class CoffeeTask : MonoBehaviour
{
    [Header("XR Objects")]
    public XRGrabInteractable mokaPot;
    public XRGrabInteractable cup;

    [Header("Spawn Points, assign in editor")]
    public Transform potSpawn;
    public Transform cupSpawn;

    [Header("Pour Setup")]
    public Transform spoutTip;            // child at the nozzle (blue arrow = flow dir)
    public ParticleSystem pourParticles;  // particle system under spoutTip
    public Collider cupMouth;             // trigger collider at cup opening
    public float rayDistance = 0.35f;     // stream length

    [Header("Angles")]
    public float angleOnDeg = 35;        // start pouring when <= this to DOWN (more realistic)
    public float angleOffDeg = 55;       // keep pouring when <= this

    [Header("Completion")]
    public float requiredSeconds = 0.3f;    // time hitting cup to complete
    public bool IsComplete { get; private set; }
    
    [Header("Visual Feedback")]
    public Renderer cupRenderer;             // For visual feedback on the cup
    
    [Header("Cup Materials")]
    public Material emptyCupMaterial;        // Material for empty cup
    public Material filledCupMaterial;       // Material for filled cup

    // State
    float pouringSeconds;
    bool pouring;

    void Update()
    {
        if (IsComplete || !mokaPot || !spoutTip || !pourParticles) return;

        bool held = mokaPot.isSelected;

        // Decide if we should pour (held + angle with hysteresis)
        bool targetPour = false;
        if (held)
        {
            float angleToDown = Vector3.Angle(StreamDir(), Vector3.down);
            targetPour = !pouring ? angleToDown <= angleOnDeg   // start
                                  : angleToDown <= angleOffDeg; // keep
        }

        if (targetPour != pouring)
        {
            pouring = targetPour;
            SetParticles(pouring); 
        }

        // Count time only while pouring AND ray hits the cup mouth
        if (pouring && RayHitsCupMouth())
        {
            pouringSeconds += Time.deltaTime;
            // Visual feedback: cup fills with coffee color
            UpdateCupVisualization(pouringSeconds / requiredSeconds);
            
            if (pouringSeconds >= requiredSeconds)
            {
                IsComplete = true;
                SetParticles(false);
                StartCoroutine(CompleteCoffeeFlash());
                Debug.Log("Coffee task COMPLETE");
            }
        }
        else
        {
            pouringSeconds = 0f;
            UpdateCupVisualization(0f);
        }
    }

    // Coffee Task reset
    // DO NOT CHANGE
    public void ResetTask()
    {
        // 1) Drop if held
        ForceRelease(mokaPot);
        ForceRelease(cup);

        // 2) Stop & clear particles
        if (pourParticles)
            pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 3) Reset state
        pouring = false;
        pouringSeconds = 0f;
        IsComplete = false;

        // 4) Respawn to spawn points
        if (mokaPot && potSpawn)
            mokaPot.transform.SetPositionAndRotation(potSpawn.position, potSpawn.rotation);
        if (cup && cupSpawn)
            cup.transform.SetPositionAndRotation(cupSpawn.position, cupSpawn.rotation);

        // 5) Zero physics
        ZeroBody(mokaPot ? mokaPot.GetComponent<Rigidbody>() : null);
        ZeroBody(cup     ? cup.GetComponent<Rigidbody>()     : null);

        Debug.Log("Coffee task RESET");
    }

    // ===== Helpers =====

    // DO NOT CHANGE
    Vector3 StreamDir() => spoutTip.forward; // blue axis

    // check whether the ray from the sprouttip hits the triggercollider
    bool RayHitsCupMouth()
    {
        if (!cupMouth) return false;
        Vector3 dir = StreamDir();
        Vector3 origin = spoutTip.position + dir * 0.01f; // avoid hitting our own pot
        return Physics.Raycast(origin, dir, out var hit, rayDistance, ~0, QueryTriggerInteraction.Collide)
               && hit.collider == cupMouth;
    }

    // turn particles on/off
    void SetParticles(bool play)
    {
        if (!pourParticles) return;
        if (play && !pourParticles.isPlaying) pourParticles.Play(true);
        if (!play && pourParticles.isPlaying)  pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // force the release of the coffee cup
    void ForceRelease(XRGrabInteractable grab)
    {
        if (!grab || !grab.isSelected) return;
        var im = grab.interactionManager;
        // Cleanly end all selections (drop)
        for (int i = grab.interactorsSelecting.Count - 1; i >= 0; i--)
        {
            var interactor = grab.interactorsSelecting[i];
            im?.SelectExit(interactor, grab);
        }
    }

    // stop movement
    // DO NOT CHANGE
    void ZeroBody(Rigidbody rb)
    {
        if (!rb) return;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }
    
    // Visual feedback for cup progress
    private void UpdateCupVisualization(float fillLevel)
    {
        if (cupRenderer == null) return;
        
        // Use materials if available, otherwise fallback to color interpolation
        if (emptyCupMaterial != null && filledCupMaterial != null)
        {
            // Smoothly blend between materials
            var lerpedMaterial = new Material(emptyCupMaterial);
            var emptyColor = emptyCupMaterial.color;
            var filledColor = filledCupMaterial.color;
            lerpedMaterial.color = Color.Lerp(emptyColor, filledColor, fillLevel);
            cupRenderer.material = lerpedMaterial;
        }
        else
        {
            // Fallback to color interpolation
            var material = cupRenderer.material;
            var coffeeColor = new Color(0.4f, 0.2f, 0.1f, fillLevel * 0.8f); // Coffee brown
            
            if (material.HasProperty("_Color"))
            {
                var baseColor = new Color(0.8f, 0.8f, 1f, 0.3f); // Light blue empty cup
                material.color = Color.Lerp(baseColor, coffeeColor, fillLevel);
            }
        }
        
        // Additional visual feedback: scale or glow effect
        if (fillLevel > 0.8f)
        {
            cupRenderer.transform.localScale = Vector3.one * 1.05f; // Slight scale when nearly full
        }
        else
        {
            cupRenderer.transform.localScale = Vector3.one;
        }
    }
    
    // Completion feedback animation
    private IEnumerator CompleteCoffeeFlash()
    {
        if (cupRenderer == null) yield break;
        
        var material = cupRenderer.material;
        var originalColor = material.color;
        var completeColor = new Color(1f, 1f, 0f, 0.8f); // Yellow success color
        
        for (int i = 0; i < 3; i++)
        {
            material.color = completeColor;
            yield return new WaitForSeconds(0.2f);
            material.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
