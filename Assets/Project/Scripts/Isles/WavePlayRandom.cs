using System.Collections;
using UnityEngine;

public class WavePlayRandom : MonoBehaviour
{
    public Animator animatorSystem;
    private Coroutine animationWave;

    private void Update()
    {
        if (animationWave == null)
            animationWave = StartCoroutine(StartRoutine(Random.Range(1, 8)));
    }


    private IEnumerator StartRoutine(int second)
    {
        animatorSystem.Play("Start");
        yield return new WaitForSeconds(second);
        animationWave = null;
    }
    private void OnDisable() {
        animationWave = null;
    }
}
