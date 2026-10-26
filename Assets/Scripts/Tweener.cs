using System;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private Tween activeTween;
    private Action onComplete;

    public bool AddTween(
        Transform target,
        Vector3 startPos,
        Vector3 endPos,
        float duration,
        Action onComplete)
    {
        if (activeTween != null)
        {
            return false;
        }

        activeTween = new Tween(target, startPos, endPos, Time.time, duration);

        this.onComplete = onComplete;

        return true;
    }

    private void Update()
    {
        if (activeTween == null)
        {
            return;
        }

        float elapsedTime = Time.time - activeTween.StartTime;
        float progress = elapsedTime / activeTween.Duration;
        progress = Mathf.Clamp01(progress);

        activeTween.Target.position = Vector3.Lerp(
            activeTween.StartPos,
            activeTween.EndPos,
            progress
        );

        if (progress >= 1f)
        {
            activeTween.Target.position = activeTween.EndPos;
            Action completedCallback = onComplete;

            activeTween = null;

            completedCallback?.Invoke();
        }
    }
}
