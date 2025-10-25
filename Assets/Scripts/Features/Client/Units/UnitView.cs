using UnityEngine;

public class UnitView : MonoBehaviour
{
    [SerializeField] 
    private GameObject _ThirtPersonView;
    [SerializeField] 
    private GameObject _FirstPersonView;
    [SerializeField] 
    private Transform _RotationRoot;
    [SerializeField] 
    private GameObject _CameraGo;
    [SerializeField] 
    private Transform _PossessionPoint;
    [SerializeField] 
    private Rigidbody _Rigidbody;
    [SerializeField] 
    private float _MoveSpeed = 5f;
    [SerializeField] 
    private float _Acceleration = 5f; 
    [SerializeField] 
    private float _JumpForce = 5f; 

    public Transform FirstPersonView => _FirstPersonView.transform;
    public Transform PossessionPoint => _PossessionPoint.transform;
    public Transform CameraGo => _CameraGo.transform;
    public Rigidbody Rigidbody => _Rigidbody; 
    public Transform RotationRoot => _RotationRoot; 
    public float MoveSpeed => _MoveSpeed; 
    public float Acceleration => _Acceleration;
    public float JumpForce => _JumpForce;


    public void ChangeView(bool isFirstPerson)
    {
        _FirstPersonView.SetActive(isFirstPerson);
        _ThirtPersonView.SetActive(!isFirstPerson);
    }
}
