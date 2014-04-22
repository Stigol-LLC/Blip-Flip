#region Usings

#if UNITY_EDITOR
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Social;
using UIEditor.Core;
using UIEditor.Node;
using UIEditor.Util;
using UnityEngine;

#endregion

public class GameScene : MonoBehaviour, ITouchable {
    #region Properties

    private static GameScene _gameScene;
    public static GameScene Active {
        get {
            if ( _gameScene == null ) {
                _gameScene = FindObjectOfType<GameScene>();
            }
            return _gameScene;
        }
    }

    [SerializeField] private StackMoveObject _moveBarrier = null;
    [SerializeField] private AutoMoveObject _moveBackground = null;
    [SerializeField] private Player _player;

    private Label _countlabel;
    private Label _obstaclelabel;
    private int _bestResult;

    [SerializeField] private float _lenghtMoveTouch = 10.0f;
    private Vector2 _touchBegin = Vector2.zero;

    private Animator _playerAnimator;
    [SerializeField, HideInInspector] private float _animationSpeedKoef = 1.0f;
    [SerializeField] private float _speedUpTimeMult = 1.0f;
    [SerializeField] private float _speedUpTimeAdd = 0.0f;


    [SerializeField] private bool _allowCircleSlide = true;

    private string[] _playerSides = {
        "up",
        "down"
    };

    private bool _playerSide;
    private bool _touch = true;
    private int _currentRestart;
    private int _indexSlide = 0;
    private int _slideInCurrentTouch;
    private GameObject _lastCompliteObject;

    [SerializeField] private SettingProject _setting = null;

//    [SerializeField] private GameObject _tutorialSlide;
    private bool _isTutorial = true;
    private float _tutorialBaseSpeed = -1.5f;

    [SerializeField] private float _tutorialSpeedKoef = 16.0f;
    [SerializeField] private float _tutorialMotionDump = 0.97f;

    private int _lastMoveBarrier = 0;
    [SerializeField] private int _startMoveObject = 0;

    private GameObject _tutorialFindObject;
    public int CountScore { get; set; }
    private float _startTime;
    private float _currentTime;
    private float _setTime;
    [SerializeField] private float _baseSpeed = 5.0f;
    [SerializeField] private float _accelerationTimeInterval = 30.0f;
    private bool _newTouch;
    private float _scoreCountTime = 2.0f;
    private bool _pauseUpdate = true;

    #endregion

    #region Constructor

    private GameScene() {
        CountScore = 0;
    }

    #endregion

    #region Methods

    public void SortZorder() {
        List<VisualNode> zOrderList = NodeContainer.SortChildrenList( this.transform );
        Debug.Log( zOrderList.Count );
        int i = 0;
        foreach ( VisualNode vn in zOrderList ) {
            vn.transform.position = new Vector3(
                    Mathf.CeilToInt( vn.transform.position.x ),
                    Mathf.CeilToInt( vn.transform.position.y ),
                    transform.position.z - i * 0.1f );
            i++;
        }
    }

    private void Awake() {
        ViewManager.Active.GameManager = this;
        UnityEngine.Social.localUser.Authenticate( result => { } );
//        if ( _setting != null ) {
//            Chartboost.Instance().Initialize( _setting.CHARTBOOST_APPID, _setting.CHARTBOOST_SIGNATURE );
//            DeviceInfo.Initialize( _setting.STAT_FOLDER_NAME, _setting.STAT_APP_NAME, _setting.STAT_URL );
//            Social.Facebook.Instance().Initialize( _setting.FACEBOOK_APPID, _setting.FACEBOOK_PERMISSIONS );
//            AmazonHelper.Instance().Initialize( _setting.AMAZON_ACCESS_KEY, _setting.AMAZON_SECRET_KEY );
//            AmazonHelper.Instance()
//                        .UploadFiles(
//                                Path.Combine( Finder.SandboxPath, _setting.STAT_FOLDER_NAME ),
//                                _setting.AMAZON_STAT_BUCKET,
//                                new[] {
//                                    "txt"
//                                },
//                                true );
//            DeviceInfo.CollectAndSaveInfo();
//        }
//        initTutorial();
    }

    private void Game() {
        ViewManager.Active.GetViewById( "PauseCounter" ).IsVisible = false;
        if ( _player == null &&
             ! ViewManager.Active.GetViewById( "Tutorial" ).IsVisible ) {
            ViewManager.Active.GetViewById( "Tutorial" ).IsVisible = true;
        } else if ( ! _player.Pause ) {
//            _moveBarrier.Reset();
            _moveBarrier.CurrentMoveObject().Pause = false;
            _currentTime = _startTime = Time.time;
        }
        if ( _player != null ) {
            PauseGame( false );
            return;
        }
//        GameObject go = Instantiate( Resources.Load( "PlayerUnite" ) ) as GameObject;
        _player = FindObjectOfType<Player>();
//        go.name = "PlayerUnite";
//        go.transform.parent = transform;
        _playerSide = false;
        _player.SetActionGameOver( GameOver );
        _playerAnimator = _player.Animator;
        _playerAnimator.Play( "start_up" );
        _touch = true;
        _moveBackground.Pause = false;
//        SoundManager.Active.OnEnterGame();
        _player.GetComponent<VisualNode>().IsVisible = true;
//        go.SetActive( true );
//        _playerAnimator.Play( "start" );
//        StartCoroutine( "StartSoundPlay", 1.1f );
        _player.Pause = false;
        _pauseUpdate = false;
        //if(isSlide){
//        ButtonBase bb = (ButtonBase) ViewManager.Active.GetViewById( "Game" ).GetChildById( "1" );
//        bb.State = ButtonState.Focus;
//        if ( ButtonBase.focusButton != null ) {
//            ButtonBase.focusButton.State = ButtonState.Default;
//        }
//        ButtonBase.focusButton = bb;
        //}
        CountScore = 0;
        _countlabel.MTextMesh.text = CountScore.ToString();
        _animationSpeedKoef = Mathf.Abs( _moveBarrier.CurrentMoveObject().speed.x ) / _baseSpeed;
    }

    private string GetAnimationName( string stateName ) {
        return stateName + "_" + _playerSides[ _playerSide ? 1 : 0 ];
    }

    private void InstantiatePlayer() {
//        GameObject go = Instantiate( Resources.Load( "PlayerUnite" ) ) as GameObject;
        _player = FindObjectOfType<Player>(); //go.GetComponent<Player>();
//        go.name = "PlayerUnite";
//        go.transform.parent = transform;
        _player.gameObject.SetActive( true );
    }

    private void Menu() {
        ViewManager.Active.GetViewById( "Game" ).IsVisible = false;
        _pauseUpdate = true;
        if ( _moveBarrier != null ) {
            _moveBarrier.Reset();
//            Destroy( _tutorialSlide );
        }
        if ( _player != null ) {
            if ( _playerAnimator != null ) {
                _playerAnimator.Play( "appear" );
            }
            _player.Pause = true;
            _player.Reset();
            _player = null;
//            SoundManager.Active.OnEnterMenu();
        }
        CountScore = 0;
        _countlabel.MTextMesh.text = CountScore.ToString();
    }

    private void OnApplicationPause( bool pauseStatus ) {
        if ( _setting != null ) {
            AmazonHelper.Instance()
                        .UploadFiles(
                                Path.Combine( Finder.SandboxPath, _setting.STAT_FOLDER_NAME ),
                                _setting.AMAZON_STAT_BUCKET,
                                new[] {
                                    "txt"
                                },
                                true );
        }
    }

    // Use this for initialization

    private void OnDestroy() {
        //Debug.Log("Destroy");
    }

    //TODO Death pause lock
    private void Over() {
    }

    private void SetSide( string stateName ) {
        _playerSide = stateName == "transform" ? ! _playerSide : _playerSide;
    }

    private void PauseGame( bool key ) {
//        moveBackground.Pause = key;
        if ( _moveBarrier.CurrentMoveObject() != null ) {
            _moveBarrier.CurrentMoveObject().Pause = key;
        }
        _pauseUpdate = key;
        _player.Pause = key;
        _touch = ! key;
    }

//    private void ShowPlayer( int num, bool isSlide = false, string stateName = null ) {
//        if ( _playerAnimator != null/* && _playerAnimator.GetCurrentAnimatorStateInfo( 0 ).nameHash ==
//             Animator.StringToHash( "Base Layer.idle_" + _playerSides[ _playerSide ? 1 : 0 ] ) */) {
//            string playState = stateName != null ? GetAnimationName( stateName ) : "slide" + currentShow + "_" + num;
////            if ( isSlide ) {
////                ButtonBase bb = (ButtonBase) ViewManager.Active.GetViewById( "Game" ).GetChildById( num.ToString() );
////                bb.State = ButtonState.Focus;
////                ButtonBase.focusButton.State = ButtonState.Default;
////                ButtonBase.focusButton = bb;
////            }
////            if ( _playerAnimator != null ) {
////                _playerAnimator.speed = Mathf.Abs( moveBarrier.CurrentMoveObject().speed.x ) * animationSpeedKoef;
////            }
////            if ( musicPlay ) {
////                AudioSource.PlayClipAtPoint( clipSwapPlayer, Vector3.zero );
////            }
////            StopCoroutine( "StartAnimationPlay" );
////            StartCoroutine( "StartAnimationPlay", 0.25f / _playerAnimator.speed );
//            _playerAnimator.Play( playState );
//            SetSide( stateName );
//            currentShow = num;
//        }
//    }

    private void ShowPlayer( string stateName ) {
        if ( _playerAnimator == null ) {
            return;
        }
        int currentStateHash = _playerAnimator.GetCurrentAnimatorStateInfo( 0 ).nameHash;
        int idleStateHash = Animator.StringToHash( "Base Layer.idle_" + _playerSides[ _playerSide ? 1 : 0 ] );
        int slideStateHash = Animator.StringToHash( "Base Layer.slide_" + _playerSides[ _playerSide ? 1 : 0 ] );
        if ( currentStateHash == idleStateHash ||
             ( currentStateHash == slideStateHash /* && stateName == "jump" ) ||
             ( currentStateHash == SlideStateHash && stateName == "transform"*/ ) ) {
            _playerAnimator.speed = /*Mathf.Abs( moveBarrier.CurrentMoveObject().speed.x ) **/ _animationSpeedKoef;
            if ( _newTouch &&
                 currentStateHash == slideStateHash &&
                 stateName == "slide" ) {
                _playerAnimator.Play( slideStateHash, -1, 0.17f );
                return;
            }
            string playState = GetAnimationName( stateName );
            _playerAnimator.Play( playState );
            SetSide( stateName );
        }
    }

    private void GameOver() {
        _playerAnimator.speed = 1.0f;
        _playerAnimator.Play( "death_" + _playerSides[ _playerSide ? 1 : 0 ] );
//        moveBackground.Pause = true;
        if ( _moveBarrier.CurrentMoveObject() != null ) {
            _moveBarrier.CurrentMoveObject().Pause = true;
        }
        _player.Pause = true;
        _player.Reset();
//        Camera.main.animation.Play();
        PlayerPrefs.SetInt( "MoveBarrier", _moveBarrier.CurrentIndex );
        if ( ( _currentRestart % 5 ) == 0 ) {
            //Debug.Log("Show Chartboost");
//            Chartboost.Instance().CacheMoreApps( null );
        }
        _currentTime = _startTime = 0.0f;
        StartCoroutine( "ShowGameOverView", 1.0f );
    }

    private void ShowGameOverView( float time ) {
        _player = null;
        _moveBarrier.Reset();
//        yield return new WaitForSeconds( time );
        StartCoroutine( "ShowScore", _scoreCountTime / CountScore );
//        int bestResult = Mathf.Max( CountScore, PlayerPrefs.GetInt( "bestResult" ) );
//        UnityEngine.Social.ReportScore(
//                bestResult,
//                "com.oleh.gates",
//                result => { Debug.Log( ( result ) ? "Complite send score" : "failed send score" ); } );
//        PlayerPrefs.SetInt( "bestResult", bestResult );
//        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetInt("bestResult").ToString());
//        Button button = new Button();
//        button.ActionName = "BTN_END";
//        ViewManager.Active.GetViewById( "Game" ).RunAction( button );
//        ViewManager.Active.GetViewById( "Over" ).IsVisible = true;
//        VisualNode group = ViewManager.Active.GetViewById( "Over" ).GetChildById( "group" );
//        if ( group.GetChildById( "result" ) is Label ) {
//            ( group.GetChildById( "result" ) as Label ).MTextMesh.text = CountScore.ToString();
//            ( group.GetChildById( "bestResult" ) as Label ).MTextMesh.text = bestResult.ToString();
//        }
//        Destroy( _tutorialSlide );
//        Destroy( _player.gameObject );
    }

    private IEnumerator ShowScore( float time ) {
        yield return new WaitForSeconds( 1.0f );
        int current = 0;
        VisualNode group = ViewManager.Active.GetViewById( "Over" ).GetChildById( "group" );
        if ( CountScore > _bestResult ) {
            _bestResult = CountScore;
        }
        int countStep = Mathf.CeilToInt( 0.0166f / time );
        while ( ( current + countStep ) <= CountScore ) {
            current += countStep;
            if ( group.GetChildById( "result" ) is Label ) {
                ( group.GetChildById( "result" ) as Label ).MTextMesh.text = current.ToString();
                ( group.GetChildById( "bestResult" ) as Label ).MTextMesh.text = _bestResult.ToString();
                SoundManager.Active.PlayClip( "score" );
            }
            yield return new WaitForSeconds( time );
        }
        if ( CountScore > _bestResult ) {
            ( group.GetChildById( "result" ) as Label ).MTextMesh.text = _bestResult.ToString();
        }
    }

    private void SkipIdleSpeed() {
        if ( _playerAnimator != null &&
             _playerAnimator.speed > 1.0f &&
             _playerAnimator.GetCurrentAnimatorStateInfo( 0 ).nameHash ==
             Animator.StringToHash( "Base Layer.idle_" + _playerSides[ _playerSide ? 1 : 0 ] ) ) {
            _playerAnimator.speed = 1.0f;
        }
    }

    private void Start() {
//        musicPlay = ( PlayerPrefs.GetInt( "music" ) != 0 );
//        if ( musicPlay ) {
//            ViewManager.Active.GetViewById( "ViewStart" ).GetChildById( "musicOff" ).IsVisible = false;
//            ViewManager.Active.GetViewById( "ViewStart" ).GetChildById( "musicOn" ).IsVisible = true;
//            musicMenu.Play();
//        } else {
//            ViewManager.Active.GetViewById( "ViewStart" ).GetChildById( "musicOff" ).IsVisible = true;
//            ViewManager.Active.GetViewById( "ViewStart" ).GetChildById( "musicOn" ).IsVisible = false;
//        }
        Application.targetFrameRate = 60;
        TouchProcessor.Instance.AddListener( this, -1 );
//        ViewManager.Active.GetViewById( "Over" ).SetDelegate( "BTN_RESTART", Restart );
//        ViewManager.Active.GetViewById( "Over" ).SetDelegate( "Home", GoHome );
//        ViewManager.Active.GetViewById( "Over" ).SetDelegate( "GameCentr", GameCentr );
//        ViewManager.Active.GetViewById( "Over" ).SetDelegate( "BTN_TWITTER", Twitter );
//        ViewManager.Active.GetViewById( "Over" ).SetDelegate( "BTN_FACEBOOK", Facebook );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "BTN_INFO", ShowInfo );
//        ViewManager.Active.GetViewById( "Info" ).SetDelegate( "BTN_BACK", ShowGame );
//        ViewManager.Active.GetViewById( "Game" ).SetDelegate( "ShowPlayer", ShowPlayer );
//        ViewManager.Active.GetViewById( "Game" ).SetDelegate( "BTN_PAUSE", ShowPause );
//        ViewManager.Active.GetViewById( "Pause" ).SetDelegate( "BTN_PLAY", ResumeGame );
        _countlabel = (Label) ViewManager.Active.GetViewById( "Game" ).GetChildById( "count" );
        _obstaclelabel = (Label) ViewManager.Active.GetViewById( "Game" ).GetChildById( "obstacle" );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "BTN_PLAY", StartGame );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "GameCentr", GameCentr );
//        _moveBackground.Pause = false;
        ViewManager.Active.GetViewById( "SplashScreen" ).IsVisible = false;
        InstantiatePlayer();
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = true;
//        ViewManager.Active.GetViewById( "Start" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Over" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Game" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Info" ).SetSingleAction( ButtonClick );
    }

    private void Update() {
        if ( _pauseUpdate ) {
            return;
        }
        SkipIdleSpeed();
//        AutoMoveObject currMove = moveBarrier.CurrentMoveObject();
//        List<GameObject> listGo = currMove.ListActiveObject;
//        GameObject go = null;
//        int indexLeft = -1;
//        for ( int i = listGo.Count - 1; i >= 0; --i ) {
//            if ( listGo[ i ].transform.position.x < _player.playerNode.transform.position.x ) {
//                go = listGo[ i ];
//                indexLeft = i;
//                break;
//            }
//        }
//        if ( isTutorial ) {
//            bool setFast = false;
//            int needShow = -1;
//            List<GameObject> listTutorial = moveBarrier.ListMoveObject[ 0 ].ListActiveObject;
//            for ( int i = 0; i < listTutorial.Count; ++i ) {
//                if ( listTutorial[ i ].transform.position.x > _player.playerNode.transform.position.x ) {
//                    VisualNode vn = listTutorial[ i ].GetComponent<VisualNode>();
//                    if ( vn != null ) {
//                        needShow = int.Parse( listTutorial[ i ].GetComponent<VisualNode>().Id );
//                        if ( needShow == currentShow ) {
//                            setFast = true;
//                            tutorialFindObject = listTutorial[ i ];
//                            break;
//                        }
//                    }
//                }
//            }
//            if ( setFast ) {
//                if ( !animatorPlay &&
//                     Mathf.Abs( currMove.speed.x ) < Mathf.Abs( tutorialBaseSpeed * tutorialSpeedKoef ) ) {
//                    moveBarrier.ListMoveObject[ 0 ].speed.x = tutorialBaseSpeed * tutorialSpeedKoef;
//                }
//            } else {
//                if ( tutorialFindObject != null ) {
//                    if (
//                            Mathf.Abs(
//                                    tutorialFindObject.transform.position.x - _player.playerNode.transform.position.x ) >
//                            10.0f ) {
//                        moveBarrier.ListMoveObject[ 0 ].speed.x *= tutorialMotionDump;
//                        if ( Mathf.Abs( currMove.speed.x ) < Mathf.Abs( tutorialBaseSpeed ) ) {
//                            moveBarrier.ListMoveObject[ 0 ].speed.x = tutorialBaseSpeed;
//                        }
//                    }
//                } else {
//                    moveBarrier.ListMoveObject[ 0 ].speed.x = tutorialBaseSpeed;
//                }
//            }
//            if ( tutorialSlide != null ) {
//                if ( CountScore >= 0 &&
//                     needShow != -1 &&
//                     needShow != currentShow ) {
//                    tutorialSlide.SetActive( true );
//                    if ( !tutorialSlide.animation.isPlaying ) {
//                        //Debug.Log(needShow + " " + currentShow);
//                        int delt = ( needShow - currentShow );
//                        if ( delt == 1 ||
//                             delt == -3 ||
//                             delt == 2 ) {
//                            tutorialSlide.animation.Play( "TutorialSlideRight" );
//                        } else if ( delt == -1 ||
//                                    delt == 3 ||
//                                    delt == -2 ) {
//                            tutorialSlide.animation.Play( "TutorialSlideLeft" );
//                        }
//                    }
//                }
//            }
//            if ( moveBarrier.CurrentIndex > 0 ) {
//                Debug.Log( "Set false" );
//                isTutorial = false;
//                moveBarrier.ListMoveObject[ 0 ].Clear();
//                tutorialSlide.SetActive( false );
//                PlayerPrefs.SetInt( "MoveBarrier", moveBarrier.CurrentIndex );
//                PlayerPrefs.SetInt( "ShowTutorial", 0 );
//                PlayerPrefs.Save();
//                GameObject GreatJob = Instantiate( Resources.Load( "Text_GreatJob" ) ) as GameObject;
//                GreatJob.transform.parent = transform;
//            }
//        }
//        if ( go != lastCompliteObject ) {
//            lastCompliteObject = go;
////            CountScore++;
////            count_label.MTextMesh.text = CountScore.ToString();
////            moveBarrier.CurrentMoveObject().speed.x *= speedUpTimeMult;
////            moveBarrier.CurrentMoveObject().speed.x += speedUpTimeAdd;
////            animationSpeedKoef = moveBarrier.CurrentMoveObject().speed.x / 5.0f;
//        }
        UpdateSpeed();
        UpdateScore();
    }
    //TODO Score principle
    private void UpdateScore() {
        AutoMoveObject currentMove = _moveBarrier.CurrentMoveObject();
        if ( currentMove != null &&
             currentMove.CurrentObstacle != null ) {
            _obstaclelabel.MTextMesh.text = _moveBarrier.CurrentMoveObject().CurrentObstacle.name;
        }
        if ( _startTime > 0 ) {
            CountScore = (int) ( ( _currentTime - _startTime ) * _animationSpeedKoef );
            _countlabel.MTextMesh.text = CountScore.ToString();
            _currentTime = Time.time;
        }
    }

    private void UpdateSpeed() {
        if ( _startTime <= 0 ) {
            return;
        }
        float time = Time.time;
        if ( !( time > 0 ) ||
             !( ( time - _setTime ) >= _accelerationTimeInterval ) ) {
            return;
        }
        AutoMoveObject currentMove = _moveBarrier.CurrentMoveObject();
        if ( currentMove == null ) {
            return;
        }
        _setTime = time;
        currentMove.speed.x *= _speedUpTimeMult;
        currentMove.speed.x += _speedUpTimeAdd;
        _animationSpeedKoef = Math.Abs( currentMove.speed.x / _baseSpeed );
    }

    #endregion

    #region Action

//    private void ButtonClick( ICall bb ) {
//        Debug.Log( "bb" + bb.ActionIdWithStore );
//        if ( musicPlay ) {
//            VisualNode vn = bb as VisualNode;
//            if ( vn != null &&
//                 vn.Id.CompareTo( "View" ) == 0 ) {
//                if ( clipChangeView != null ) {
//                    AudioSource.PlayClipAtPoint( clipChangeView, Vector3.zero );
//                }
//            } else {
//                if ( clipButtonClick != null ) {
//                    AudioSource.PlayClipAtPoint( clipButtonClick, Vector3.zero );
//                }
//            }
//        }
//    }


//    private IEnumerator ShowGame() {
//        yield return new WaitForSeconds( 1.0f );
//        ViewManager.Active.GetViewById( "PauseCounter" ).IsVisible = false;
//        yield return new WaitForSeconds( 1.7f );
//        PauseGame( false );
//    }
//
//    private void ShowGame( ICall iCall ) {
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = true;
//        ViewManager.Active.GetViewById( "Info" ).IsVisible = false;
//        ViewManager.Active.GetViewById( "Pause" ).IsVisible = false;
//    }


    private void Facebook( ICall bb ) {
        if ( !Social.Facebook.Instance().IsOpenSession ) {
            Social.Facebook.Instance().Login(
                    result => {
                        if ( !string.IsNullOrEmpty( result ) ) {
                            Social.Facebook.Instance()
                                  .GetUserDetails(
                                          SettingProject.Instance.FACEBOOK_PERMISSIONS.ToString(),
                                          r => { SaveFBUserDetail( r ); } );
                        }
                    } );
        } else {
            Social.Facebook.Instance()
                  .GetUserDetails(
                          SettingProject.Instance.FACEBOOK_PERMISSIONS.ToString(),
                          result => { SaveFBUserDetail( result ); } );
            Social.Facebook.Instance().GoToPage( _setting.FACEBOOK_APPID );
        }
    }

    private void FacebookCall( ICall bb ) {
        if ( Social.Facebook.Instance().IsOpenSession ) {
            FacebookGetUserData();
            Social.Facebook.Instance().GoToPage( _setting.STIGOL_FACEBOOK_ID );
        } else {
            Social.Facebook.Instance().Login(
                    s => {
                        if ( !string.IsNullOrEmpty( s ) ) {
                            FacebookGetUserData();
                        } else {
                            Social.Facebook.Instance().GoToPage( _setting.STIGOL_FACEBOOK_ID );
                        }
                        Social.Facebook.Instance().GoToPage( _setting.STIGOL_FACEBOOK_ID );
                    } );
        }
        ;
    }

    private void FacebookGetUserData() {
        Social.Facebook.Instance().GetUserDetails(
                string.Join( ",", _setting.FACEBOOK_PERMISSIONS ),
                res => {
                    if ( res != null ) {
                        SaveFBUserDetail( res );
                    }
                } );
    }

    private void GameCentr( ICall bb ) {
        UnityEngine.Social.ShowLeaderboardUI();
    }

    private void GoHome( ICall bb ) {
        _moveBackground.Pause = false;
    }

    private void Info() {
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = false;
//        ViewManager.Active.GetViewById( "Info" ).IsVisible = true;
    }

    private void Pause() {
        ViewManager.Active.GetViewById( "Tutorial" ).IsVisible = false;
        PauseGame( true );
//        ViewManager.Active.GetViewById( "Pause" ).IsVisible = true;
    }

    private void PauseCounter() {
//        PauseGame( false );
    }

    private void Restart( ICall bb ) {
//        ViewManager.Active.GetViewById( "Over" ).IsVisible = false;
        Game();
        _moveBackground.Pause = false;
        _currentRestart++;
        if ( PlayerPrefs.HasKey( "MoveBarrier" ) ) {
            _moveBarrier.CurrentIndex = Mathf.Min( _startMoveObject, PlayerPrefs.GetInt( "MoveBarrier" ) );
        }
        if ( _moveBarrier.CurrentIndex == 0 ) {
//            initTutorial();
        }
    }

    private void SaveFBUserDetail( JSONObject result ) {
        JSONObject anyData = new JSONObject();
        anyData.AddField( "Facebook", result );
        Debug.Log( "Facebook = " + anyData );
        DeviceInfo.CollectAndSaveInfo( anyData );
    }

    private void SaveFBUserDetail( string result ) {
        if ( result != null ) {
            JSONObject anyData = new JSONObject();
            JSONObject facebookDetail = new JSONObject( result );
            anyData.AddField( "Facebook", facebookDetail );
            DeviceInfo.CollectAndSaveInfo( anyData );
        }
    }

//    private void ShowPlayer( ICall bb ) {
//        if ( musicPlay && clipButtonClick != null ) {
//            AudioSource.PlayClipAtPoint( clipButtonClick, Vector3.zero );
//        }
//        int num = int.Parse( bb.ActionValue );
//        ShowPlayer( num );
//    }

//    private void StartGame( ICall bb ) {
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = false;
//        ViewManager.Active.GetViewById( "Game" ).IsVisible = true;
//        Game();
//    }

    private void Twitter( ICall bb ) {
        Social.Twitter.Instance().Login();
        Social.Twitter.Instance().GoToPage( _setting.TWEET_FOLLOW );
        if ( string.IsNullOrEmpty( Social.Twitter.Instance().UserId ) ) {
            JSONObject anyData = new JSONObject();
            anyData.AddField( "user_twitter_id", Social.Twitter.Instance().UserId );
            DeviceInfo.CollectAndSaveInfo( anyData );
        }
    }

    #endregion

    #region Touch

    public Rect GetTouchableBound() {
        return new Rect( 0, 0, 0, 0 );
    }

    public bool IsPointInBound( Vector2 point ) {
        return true;
    }

    public bool IsTouchable {
        set { }
        get { return true; }
    }

    public bool TouchBegan( Vector2 touchPoint ) {
        if ( !_touch ) {
            return false;
        }
        _touchBegin = touchPoint;
        _newTouch = true;
//        _player.Up();
        return true;
    }

    public bool TouchMove( Vector2 touchPoint ) {
        if ( !_touch ) {
            return false;
        }
        float verticalSlideLenght = _touchBegin.y - touchPoint.y;
        float horizontalSlideLenght = _touchBegin.x - touchPoint.x;
        bool vertical = Mathf.Abs( verticalSlideLenght ) > Mathf.Abs( horizontalSlideLenght );
        if ( ! vertical &&
             ( horizontalSlideLenght < -_lenghtMoveTouch || horizontalSlideLenght > _lenghtMoveTouch ) ) {
            ShowPlayer( "slide" ); //Slide
            _newTouch = false;
        } else if ( verticalSlideLenght > _lenghtMoveTouch /* && slideInCurrentTouch != 1*/ ) {
//			indexSlide--;
//			if(indexSlide < 0){
//				indexSlide = (allowCircleSlide)?arraySlideObject.Length - 1:0;
//			}
//			slideInCurrentTouch = 1;
//            if ( indexSlide >= 0 &&
//                 arraySlideObject.Length > indexSlide ) {
            ShowPlayer( _playerSide ? "jump" : "transform" ); //Down
//            }
        } else if ( verticalSlideLenght < -_lenghtMoveTouch /* && slideInCurrentTouch != -1*/ ) {
//			indexSlide++;
//			if(indexSlide >= arraySlideObject.Length){
//				indexSlide = (allowCircleSlide)?0:arraySlideObject.Length - 1;
//			}
//			slideInCurrentTouch = -1;
//            if ( indexSlide >= 0 &&
//                 arraySlideObject.Length > indexSlide ) {
            ShowPlayer( ! _playerSide ? "jump" : "transform" ); //Up
//            }
        }
//        if ( ( slideInCurrentTouch == 1 && verticalSlideLenght > 0 ) ||
//             ( slideInCurrentTouch == -1 && verticalSlideLenght < 0 ) ) {
//            touchBegin = touchPoint;
//        }
        return false;
    }

    public void TouchEnd( Vector2 touchPoint ) {
//        float shift = Vector2.Distance( touchPoint, touchBegin );
//        if ( shift <= _minEpsilon ) {
//            ShowPlayer( "jump" ); //Jump
//        }
//        slideInCurrentTouch = 0;
    }

    public void TouchCancel( Vector2 touchPoint ) {
//        slideInCurrentTouch = 0;
    }

    #endregion

//    private void initTutorial() {
//        AudioClip ac = null;
//        if ( PlayerPrefs.HasKey( "MoveBarrier" ) ) {
//            _moveBarrier.CurrentIndex = Mathf.Min( _startMoveObject, PlayerPrefs.GetInt( "MoveBarrier" ) );
//        }
//        _isTutorial = true;
//        if ( PlayerPrefs.HasKey( "ShowTutorial" ) ) {
//            int showTutorial = PlayerPrefs.GetInt( "ShowTutorial" );
//            if ( showTutorial == 0 ) {
//                _isTutorial = false;
//            }
//        } else {
//            PlayerPrefs.SetInt( "ShowTutorial", 1 );
//        }
//        if ( _isTutorial ) {
//            _tutorialSlide = Instantiate( Resources.Load( "TutorialSlide" ) ) as GameObject;
//            _tutorialSlide.SetActive( false );
//            _tutorialBaseSpeed = _moveBarrier.CurrentMoveObject().speed.x;
//            Debug.Log( _tutorialBaseSpeed.ToString() );
//        }
//    }
}