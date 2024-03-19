using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public class IdleAnimationController : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;

    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        PlayIdleAnimation();
    }

    void PlayIdleAnimation()
    {
        
        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
    }
}
