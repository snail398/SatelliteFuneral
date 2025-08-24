using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    private List<Rigidbody> _rigidbodies;

    /// <summary>
    /// ������������� ������ �������-���������
    /// </summary>
    public void Initialize()
    {
        _rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        Disable(); // �� ��������� ���������
    }

    /// <summary>
    /// �������� ������� (������� ������ ��������)
    /// </summary>
    public void Enable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }

    /// <summary>
    /// ��������� ������� (������� ������ ����������)
    /// </summary>
    public void Disable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }
}
