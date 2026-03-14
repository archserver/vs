using UnityEngine;

public class AnimationObjectDestroy : MonoBehaviour
{
    // define variables for components
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        // Destroy the game object after it runs
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }

    
}
