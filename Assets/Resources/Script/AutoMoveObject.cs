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
    [SerializeField] private Bounds _forbiddenBounds = new Bounds(
            new Vector3( -1024.0f, 0.0f, 0.0f ),
            new Vector3( 1024.0f, 768.0f, 1024.0f ) );

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
            return _currentObstacle != null
                           ? _currentObstacle.GetChildrenBounds()
                           : new Bounds( Vector3.zero, Vector3.zero );
        }
    }

    private bool CanCreate {
        get { return ( countToCreateObstacles == -1 || CountOfCreatedObstacles < countToCreateObstacles ); }
    }

    #endregion

    #region Methods

    public void Reset() {
        if ( ! gameObject.activeSelf ) {
            return;
        }
        speed = startSpeed;
        Vector2 obstaclesPosition = ObstaclesPool.transform.position;
        transform.position = new Vector3( obstaclesPosition.x, obstaclesPosition.y, transform.position.z );
        foreach ( Transform obstacle in transform.Cast<Transform>().ToList() ) {
            obstacle.parent = ObstaclesPool.transform;
            obstacle.position = _obstacles[ obstacle.gameObject ];
            obstacle.gameObject.SetActive( false );
        }
        Pause = true;
        _countOfCreatedObstacles = 0;
        _objectPool.Reset();
        GetNewObstacle();
    }

    public void SetCallBackCount( callbackCount _delegate ) {
        _callbackCount = _delegate;
    }

    protected void OnDrawGizmosSelected() {
        if ( CurrentObstacle == null ) {
            return;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube( CurrentObstacleBounds.center, CurrentObstacleBounds.size );
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube( _forbiddenBounds.center, _forbiddenBounds.size );
    }

    private void ApplyCollider( Transform obstacle ) {
        if ( name != "MoveBlipFlip" ) {
            return;
        }
//        foreach ( BoxCollider2D boxCollider in
//                obstacle.GetComponentsInChildren<Transform>()
//                        .Select( child => child.GetComponent<BoxCollider2D>() )
//                        .Where( boxCollider => boxCollider != null ) ) {
//            Destroy( boxCollider );
//        }
        foreach ( Transform child in obstacle.transform ) {
            BoxCollider2D obstacleCollider = child.gameObject.AddComponent<BoxCollider2D>();
            Bounds childBounds = child.gameObject.GetChildrenBounds( true );
            obstacleCollider.isTrigger = true;
            obstacleCollider.center = child.InverseTransformPoint( childBounds.center );
            obstacleCollider.size = childBounds.size;
        }
    }

    private void GetNewObstacle() {
        if ( ! CanCreate ) {
            isDone = true;
            return;
        }
        if ( _obstaclesPool.transform.childCount > 0 ) {
            _currentObstacle = IdleObstacles[ Random.Range( 0, _obstaclesPool.transform.childCount ) ];
            CurrentObstacle.transform.parent = transform;
            CurrentObstacle.transform.localPosition = new Vector3(
                    CurrentObstacle.transform.localPosition.x,
                    CurrentObstacle.transform.localPosition.y,
                    0.0f );
            CurrentObstacle.Enable( true );
        }
        CountOfCreatedObstacles++;
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
        Physics.IgnoreLayerCollision( 1, 1, true );
    }

    private void InstantiateObstacle( GameObject obstacle, Transform parent ) {
        if ( obstacle == null ) {
            return;
        }
        GameObject go = Instantiate( obstacle ) as GameObject;
        ApplyCollider( go.transform );
        Bounds bounds = go.GetChildrenBounds( true );
        float centerDiff = ( go.transform.position - bounds.center ).x;
        float headPosition = objectCreatePostion.x + bounds.extents.x + centerDiff;
        go.transform.parent = parent;
        go.transform.position = new Vector3(
                headPosition,
                useOriginalYPosition ? go.transform.position.y : objectCreatePostion.y,
                go.transform.position.z );
//        float z = go.transform.localPosition.z;
//        go.transform.localPosition = new Vector3( go.transform.localPosition.x, go.transform.localPosition.y, z );
//        go.transform.Translate(
//                Random.Range( randomOffset.x, randomOffset.width ),
//                Random.Range( randomOffset.y, randomOffset.height ),
//                0 );
//        go.transform.localScale = obstacle.transform.localScale;
        go.SetLayer( 1 );
        go.name = obstacle.name;
        go.SetActive( false );
        _obstacles[ go ] = go.transform.position;
    }


    private void Start() {
        startSpeed = speed;
        InstantiateAllObstacles();
        GetNewObstacle();
    }
    //TODO Pause acceleration 
    private void TakeObstacleToOrigin() {
        foreach ( Transform obstacle in transform ) {
            obstacle.gameObject.EnableInBounds( _forbiddenBounds, false );
            if ( obstacle.gameObject.HasActiveChilds() ) {
                continue;
            }
            obstacle.parent = ObstaclesPool.transform;
            obstacle.position = _obstacles[ obstacle.gameObject ];
            obstacle.gameObject.SetActive( false );
            if ( CanCreate || transform.childCount > 0 ) {
                continue;
            }
            gameObject.SetActive( false );
            return;
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