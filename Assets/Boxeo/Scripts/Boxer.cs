using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Boxer : MonoBehaviour
{

    [SerializeField] Animator animator;
    [SerializeField] VisualEffect greenSmoke;
    [SerializeField] VisualEffect redSmoke;

    private BoxGameManager gameManager;
    private int lastStateHash = 0;
    private bool hasScoredInCurrentAnim = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<BoxGameManager>();
        greenSmoke.SendEvent("OnExit");
        redSmoke.SendEvent("OnExitRed");
    }

    // Update is called once per frame
    void Update()
    {
        if (animator == null) return;

        // Detecta cambio de animación para resetear el flag
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.fullPathHash != lastStateHash)
        {
            lastStateHash = currentState.fullPathHash;
            hasScoredInCurrentAnim = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!hasScoredInCurrentAnim)
        {
            if (collision.gameObject.CompareTag("Player") && stateInfo.IsName("Idle") && gameManager.getActiveGame())
            {
                gameManager.IncrementarPuntuacion(1);
                PlaySmoke(greenSmoke, "OnEnter", "OnExit");
                animator.SetTrigger("GetHit");
                hasScoredInCurrentAnim = true;
            }

            if (collision.gameObject.CompareTag("Player") && stateInfo.IsName("Punch") && gameManager.getActiveGame())
            {
                gameManager.IncrementarPuntuacion(-1);
                PlaySmoke(redSmoke, "OnEnterRed", "OnExitRed");
                hasScoredInCurrentAnim = true;
            }
        }
    }

    void PlaySmoke(VisualEffect smoke, string enterEventName, string exitEventName)
    {
        smoke.SendEvent(enterEventName);
        StartCoroutine(StopSmokeAfterDelay(smoke, exitEventName, 0.5f));
    }

    IEnumerator StopSmokeAfterDelay(VisualEffect smoke, string exitEventName, float delay)
    {
        yield return new WaitForSeconds(delay);
        smoke.SendEvent(exitEventName);
    }
}
