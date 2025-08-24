using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyView _view;
    [SerializeField] private RagdollHandler _ragdollHandler;

    private void Awake()
    {
        _view.Initialize();
        _ragdollHandler.Initialize();
    }

    /// <summary>
    /// ����� ����� ? ��������� �������� � �������� �������
    /// </summary>
    public void Kill()
    {
        _view.DisableAnimator();
        _ragdollHandler.Enable();
    }
}
