using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SkeletonAnimation))]
public class SpineRandomizer : MonoBehaviour
{
    [SerializeField] private float minDelay = 0f;
    [SerializeField] private float maxDelay = 10f;

    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState animationState;

    private void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        animationState = skeletonAnimation.AnimationState;
        skeletonAnimation.loop = false;

        animationState.Complete += OnSpineAnimationComplete;
    }

    public void OnSpineAnimationComplete(TrackEntry trackEntry)
    {
        float random = Random.Range(minDelay, maxDelay);
        StartCoroutine(StartAnimation_Coroutine(random));
    }

    private IEnumerator StartAnimation_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        animationState.SetAnimation(0, skeletonAnimation.AnimationName, false);
    }
}
