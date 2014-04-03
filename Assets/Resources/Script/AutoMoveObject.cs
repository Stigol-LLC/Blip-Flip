#region Usings

using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float _obstacleGap = 100.0f;

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

    private Bounds _currentObstacleBounds;
    public Vector2 CurrentObstacleBounds {
        get {
            if ( listGo.Count > 0 ) {
                GameObject _currentObstacle = listGo.Last();
                if ( _currentObstacle != null ) {
                    Bounds bounds = GetChildrenBounds( _currentObstacle );
                    return new Vector2( bounds.min.x, bounds.max.x );
                }
            }
            return Vector2.one;
        }
    }

    private Bounds GetChildrenBounds( GameObject parent ) {
        Vector3 center = Vector3.zero;
        Renderer renderer;
        foreach (Transform child in parent.transform) {
            renderer = child.GetComponent<Renderer>();
            if ( renderer != null ) {
                center += renderer.bounds.center;
            }
        }
        center /= parent.transform.childCount;
        Bounds bounds = new Bounds( center,Vector3.zero ); 
        foreach (Transform child in parent.transform) {
            renderer = child.GetComponent<Renderer>();
            if ( renderer != null ) {
                bounds.Encapsulate( renderer.bounds );
            }
        }
        return bounds;
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
        GameObject ret = null;
        if ( _objectPool != null ) {
         ret = _objectPool.GetObject();   
        }
        if ( ret != null ) {
            CountGeneratedObject++;
            GameObject go = Instantiate( ret ) as GameObject;
            float z = go.transform.localPosition.z;
            float headPosition = objectCreatePostion.x + GetChildrenBounds( go ).extents.x;
            go.transform.position = new Vector3( headPosition, useOriginalYPosition ? go.transform.position.y : objectCreatePostion.y, go.transform.position.z );
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3( go.transform.localPosition.x, go.transform.localPosition.y, z );
            go.transform.Translate(
                    Random.Range( randomOffset.x, randomOffset.width ),
                    Random.Range( randomOffset.y, randomOffset.height ),
                    0 );
            go.transform.localScale = ret.transform.localScale;
            go.name = ret.name;
            listGo.Add( go );
//            Debug.Log( CurrentObstacleBounds );
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
            int childCount = 0;
            foreach ( Transform child in go.transform ) {
                if ( child.renderer.bounds.max.x < limitPosition.x ) {
                    Destroy( child.gameObject );
                    return;
                }
                childCount++;
            }
            if ( childCount == 0 ) {
                Destroy( go );
                listGo.Remove( go );
                return;
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
//        currentPosition += Mathf.Abs( speed.x ) + Mathf.Abs( speed.y );
        if ( CurrentObstacleBounds.y + _obstacleGap <= objectCreatePostion.x ) {
            CreateObject();
        }

//        if ( currentPosition >= createNewObject ) {
//            UpdateNewObjectAppear();
//            CreateObject();
//        }
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
