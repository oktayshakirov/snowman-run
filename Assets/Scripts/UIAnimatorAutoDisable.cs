using System.Collections;
using UnityEngine;

// A UI Animator left enabled after its one-shot clip finishes keeps writing the
// same values every frame, dirtying the canvas and forcing constant rebuilds.
// GameManager attaches this to every canvas Animator whose clips are all
// non-looping; looping animators (pulsing buttons etc.) are left alone.
public class UIAnimatorAutoDisable : MonoBehaviour
{
    private Animator _animator;
    private float _clipLength = 0.5f;

    public static void AttachToFinishedUIAnimators()
    {
        foreach (Animator animator in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (animator.GetComponent<UIAnimatorAutoDisable>() != null)
                continue;
            if (animator.GetComponentInParent<Canvas>(true) == null)
                continue;

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
                continue;

            bool hasLoopingClip = false;
            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip.isLooping)
                {
                    hasLoopingClip = true;
                    break;
                }
            }

            if (!hasLoopingClip)
                animator.gameObject.AddComponent<UIAnimatorAutoDisable>();
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
                _clipLength = Mathf.Max(_clipLength, clip.length);
        }
    }

    private void OnEnable()
    {
        if (_animator == null)
            return;

        _animator.enabled = true;
        StartCoroutine(DisableWhenFinished());
    }

    private IEnumerator DisableWhenFinished()
    {
        yield return new WaitForSecondsRealtime(_clipLength + 0.1f);
        if (_animator != null)
            _animator.enabled = false;
    }
}
