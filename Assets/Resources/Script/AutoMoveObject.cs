#region Usings

using System.Collections.Generic;
using System.Linq;
using UIEditor.Util;
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
    [SerializeField] private float _obstacleGap = 100.0f;

    private bool isDone;
    [SerializeField] private ObjectPool _objectPool = null;

    [SerializeField] private bool isLimitedPosition;
    [SerializeField] private Vector2 limitPosition = new Vector2( 4000, 4000 );

    [HideInInspector, SerializeField] private MapStringAnimationClip animationEventClips = new MapStringAnimationClip();

    [SerializeField] private bool _pause = true;

    private Dictionary<GameObject, Vector2> _obstacles = new Dictionary<GameObject, Vector2>();
    [SerializeField] private int countToCreateObstacles = -1;
    private int _countOfCreatedObstacles;
    public int CountOfCreatedObstacles {
        get { return _countOfCreatedObstacles; }
        set {
            _countOfCreatedObstacles = value;
            if ( _callbackCount != null ) {
                _callbackCount( _countOfCreatedObstacles );
            }
        }
    }

    private callbackCount _callbackCount;

    public int CountActiveObject {
        get { return _obstacles.Count; }
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
    public List<GameObject> IdleObstacles {
        get { return ( from Transform obstacle in ObstaclesPool.transform select obstacle.gameObject ).ToList(); }
    }

    public bool IsDone {
        get { return isDone; }
    }

    private GameObject _currentObstacle;
    public GameObject CurrentObstacle {
        get { return _currentObstacle; }
    }
    private GameObject _obstaclesPool;
    public GameObject ObstaclesPool {
        get { return _obstaclesPool; }
    }
    private Bounds _currentObstacleBounds;
    public Bounds CurrentObstacleBounds {
        get {
            if ( _currentObstacle != null ) {
                return GetChildrenBounds( _currentObstacle );
            }
            return new Bounds();
        }
    }

    #endregion

    #region Methods

    public void Reset() {
        Debug.Log( "Reset : " + name );
        speed = startSpeed;
        Vector2 obstaclesPosition = ObstaclesPool.transform.position;
        transform.position = new Vector3( obstaclesPosition.x, obstaclesPosition.y, transform.position.z );
        foreach ( Transform obstacle in transform ) {
            obstacle.parent = ObstaclesPool.transform;
            obstacle.gameObject.SetActive( false );
        }
        GetNewObstacle();
        isDone = false;
        Pause = true;
        _countOfCreatedObstacles = 0;
        _objectPool.Reset();
    }

    public void SetCallBackCount( callbackCount _delegate ) {
        _callbackCount = _delegate;
    }

    protected void OnDrawGizmosSelected() {
        if ( CurrentObstacle != null ) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube( CurrentObstacleBounds.center, CurrentObstacleBounds.size );
        }
    }

    private Bounds GetChildrenBounds( GameObject parent ) {
        Vector3 center = Vector3.zero;
        Renderer renderer;
        foreach ( Transform child in parent.transform ) {
            renderer = child.GetComponent<Renderer>();
            if ( renderer != null ) {
                center += renderer.bounds.center;
            }
        }
        center /= parent.transform.childCount;
        Bounds bounds = new Bounds( center, Vector3.zero );
        foreach ( Transform child in parent.transform ) {
            renderer = child.GetComponent<Renderer>();
            if ( renderer != null ) {
                bounds.Encapsulate( renderer.bounds );
            }
        }
        return bounds;
    }

    private void GetNewObstacle() {
        if ( countToCreateObstacles != -1 &&
             CountOfCreatedObstacles >= countToCreateObstacles ) {
            isDone = true;
            return;
        }
        _currentObstacle = IdleObstacles[ Random.Range( 0, _obstaclesPool.transform.childCount ) ];
        CurrentObstacle.transform.parent = transform;
        CurrentObstacle.transform.localPosition = new Vector3(
                CurrentObstacle.transform.localPosition.x,
                CurrentObstacle.transform.localPosition.y,
                0.0f );
        CurrentObstacle.Enable( true );
        CountOfCreatedObstacles++;
//        if ( name == "MoveBlipFlip" ) {
//            Debug.Log( _currentObstacle + " : " + _currentObstacle.transform.position );
//        }
    }

    private void InstantiateAllObstacles() {
        if ( _objectPool == null ) {
            return;
        }
        _obstaclesPool = new GameObject {
            name = "ObstaclesPool_" + name,
        };
        ObstaclesPool.transform.parent = GameScene.Active.transform;
        ObstaclesPool.gameObject.SetActive( false );
        Vector2 obstaclesPosition = ObstaclesPool.transform.position;
        transform.position = new Vector3( obstaclesPosition.x, obstaclesPosition.y, transform.position.z );
        foreach ( GameObject obstacle in _objectPool.genListObject ) {
            InstantiateObstacle( obstacle, ObstaclesPool.transform );
        }
    }

    private void InstantiateObstacle( GameObject obstacle, Transform parent ) {
        if ( obstacle == null ) {
            return;
        }
        GameObject go = Instantiate( obstacle ) as GameObject;
        float z = go.transform.localPosition.z;
        Bounds bounds = GetChildrenBounds( go );
        float centerDiff = ( go.transform.position - bounds.center ).x;
        float headPosition = objectCreatePostion.x + bounds.extents.x + centerDiff;
        go.transform.parent = parent;
        go.transform.position = new Vector3(
                headPosition,
                useOriginalYPosition ? go.transform.position.y : objectCreatePostion.y,
                go.transform.position.z );
//        go.transform.localPosition = new Vector3( go.transform.localPosition.x, go.transform.localPosition.y, z );
        go.transform.Translate(
                Random.Range( randomOffset.x, randomOffset.width ),
                Random.Range( randomOffset.y, randomOffset.height ),
                0 );
        go.transform.localScale = obstacle.transform.localScale;
        go.name = obstacle.name;
        go.SetActive( false );
        _obstacles[ go ] = go.transform.position;
    }

    private void Start() {
        startSpeed = speed;
        InstantiateAllObstacles();
        GetNewObstacle();
        //listGo = UIEditor.Node.NodeContainer.GetAllChildren(transform);
    }

    private void TakeObstacleToOrigin() {
        foreach ( Transform obstacle in transform ) {
            foreach ( Transform child in
                    obstacle.transform.Cast<Transform>()
                            .Where(
                                    child =>
                                    child.gameObject.activeInHierarchy && child.renderer.bounds.max.x < limitPosition.x )
                    ) {
                child.gameObject.SetActive( false );
            }
            if ( ! obstacle.gameObject.HasActiveChilds() ) {
                obstacle.transform.parent = ObstaclesPool.transform;
                obstacle.transform.position = _obstacles[ obstacle.gameObject ];
                obstacle.gameObject.SetActive( false );
//                if ( isDone ) {
////                    Debug.Log( "Done : " + name );
//                    _pause = true;
//                }
            }
        }
    }

    private void Update() {
        if ( _pause || CurrentObstacle == null ) {
            return;
        }
        transform.Translate( speed.x, speed.y, 0, Space.Self );
        if ( CurrentObstacleBounds.max.x + _obstacleGap < objectCreatePostion.x ) {
            GetNewObstacle();
        }
        if ( isLimitedPosition ) {
            TakeObstacleToOrigin();
        }
    }

    #endregion
}