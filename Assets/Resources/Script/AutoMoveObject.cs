#region Usings

using System.Collections.Generic;
using UnityEngine;

#endregion

public class AutoMoveObject : MonoBehaviour {
    #region Delegates

    public delegate void callbackCount( int count );

    #endregion

    #region Properties

    [SerializeField] public Vector2 speed = Vector2.zero;

    private Vector2 startSpeed = Vector2.zero;

    [SerializeField] private float currentPosition;
    [SerializeField] private Vector2 newObjectCreationBorders = new Vector2( 500.0f, 1000.0f );
    [SerializeField, HideInInspector] private float createNewObject = 500.0f;
    [SerializeField] private Vector2 objectCreatePostion = Vector2.zero;
    [SerializeField] private bool useOriginalYPosition = true;
    [SerializeField] private Rect randomOffset = new Rect( 0, 0, 0, 0 );
    [SerializeField] private int countCreateObject = -1;

    private bool isDone;
    [SerializeField] private ObjectPool _objectPool = null;

    [SerializeField] private bool isLimitedPosition;
    [SerializeField] private Vector2 limitPosition = new Vector2( 4000, 4000 );

    [HideInInspector, SerializeField] private MapStringAnimationClip animationEventClips = new MapStringAnimationClip();

    [SerializeField] private bool _pause = true;

    private List<GameObject> listGo = new List<GameObject>();
    private int _countGen;

    private callbackCount _callbackCount;

    // Use this for initialization

    public int CountActiveObject {
        get { return listGo.Count; }
    }
    public Vector2 LimitPosition {
        set { limitPosition = value; }
        get { return limitPosition; }
    }
    public bool IsLimitedPosition {
        get { return isLimitedPosition; }
        set { isLimitedPosition = value; }
    }
    public MapStringAnimationClip AnimationEventClips {
        get { return animationEventClips; }
        set { animationEventClips = value; }
    }
    public bool Pause {
        set { _pause = value; }
        get { return _pause; }
    }
    public List<GameObject> ListActiveObject {
        get { return listGo; }
    }

    public bool IsDone {
        get { return isDone; }
    }
    public int CountGeneratedObject {
        get { return _countGen; }
        set {
            _countGen = value;
            if ( _callbackCount != null ) {
                _callbackCount( _countGen );
            }
        }
    }

    #endregion

    #region Methods

    public void Clear() {
        foreach ( var go in listGo ) {
            Destroy( go );
        }
        listGo.Clear();
    }

    public void CreateObject() {
        if ( countCreateObject != -1 &&
             _countGen >= countCreateObject ) {
            isDone = true;
            return;
        }
        GameObject ret = _objectPool.GetObject();
        if ( ret != null ) {
            CountGeneratedObject++;
            GameObject go = Instantiate( ret ) as GameObject;
            float z = go.transform.localPosition.z;
            go.transform.position = new Vector3( objectCreatePostion.x, useOriginalYPosition ? go.transform.position.y : objectCreatePostion.y, go.transform.position.z );
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3( go.transform.localPosition.x, go.transform.localPosition.y, z );
            go.transform.Translate(
                    Random.Range( randomOffset.x, randomOffset.width ),
                    Random.Range( randomOffset.y, randomOffset.height ),
                    0 );
            go.transform.localScale = ret.transform.localScale;
            go.name = ret.name;
            listGo.Add( go );
        }
    }

    public void Reset() {
        speed = startSpeed;
        Clear();
        transform.position = new Vector3( 0, 0, transform.position.z );
        isDone = false;
        _countGen = 0;
        _objectPool.Reset();
    }

    public void SetCallBackCount( callbackCount _delegate ) {
        _callbackCount = _delegate;
    }

    private void RemoveBorder() {
        foreach ( var go in listGo ) {
            if ( Mathf.Abs( go.transform.position.x ) > limitPosition.x ||
                 Mathf.Abs( go.transform.position.y ) > limitPosition.y ) {
                listGo.Remove( go );
                Destroy( go );
                break;
            }
        }
    }

    private void Start() {
        startSpeed = speed;
        //listGo = UIEditor.Node.NodeContainer.GetAllChildren(transform);
    }

    private void UpdateNewObjectAppear() {
        currentPosition -= createNewObject;
        createNewObject = Random.Range( newObjectCreationBorders.x, newObjectCreationBorders.y );
    }

    private void Update() {
        if ( _pause ) {
            return;
        }
        transform.Translate( speed.x, speed.y, 0, Space.Self );
        currentPosition += Mathf.Abs( speed.x ) + Mathf.Abs( speed.y );
        if ( currentPosition >= createNewObject ) {
            UpdateNewObjectAppear();
            CreateObject();
        }
        if ( isLimitedPosition ) {
            RemoveBorder();
        }
        if ( isDone ) {
            Debug.Log( listGo.Count );
            _pause = true;
        }
    }

    #endregion
}
