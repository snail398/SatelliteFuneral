using UnityEngine;

public class UnitView : MonoBehaviour
{
    [SerializeField] 
    private GameObject _ThirtPersonView;
    [SerializeField] 
    private GameObject _FirstPersonView;
    [SerializeField] 
    private Transform _PossessionPoint;

    public Transform FirstPersonView => _FirstPersonView.transform;
    public Transform PossessionPoint => _PossessionPoint.transform;


    public void ChangeView(bool isFirstPerson)
    {
        _FirstPersonView.SetActive(isFirstPerson);
        _ThirtPersonView.SetActive(!isFirstPerson);
    }
}
