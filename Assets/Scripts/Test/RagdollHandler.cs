using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    private List<Rigidbody> _rigidbodies;

    /// <summary>
    /// Инициализация списка рэгдолл-ригидбоди
    /// </summary>
    public void Initialize()
    {
        _rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        Disable(); // по умолчанию отключаем
    }

    /// <summary>
    /// Включить рэгдолл (сделать физику активной)
    /// </summary>
    public void Enable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }

    /// <summary>
    /// Выключить рэгдолл (сделать физику неактивной)
    /// </summary>
    public void Disable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }
}
