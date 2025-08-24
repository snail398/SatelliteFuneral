using UnityEngine;

[RequireComponent (typeof(Animator))]
public class EnemyView : MonoBehaviour
{
    private const string InRunningKey = "IsRunning";

    private Animator _animator;

    public void Initialize() => _animator = GetComponent<Animator>();

    public void StartRunning() => _animator.SetBool (InRunningKey, true);

    public void StopRunning() => _animator.SetBool(InRunningKey, false);

    public void DisableAnimator() => _animator.enabled = false;

    public void EnebledAnimator() => _animator.enabled = true;

}
