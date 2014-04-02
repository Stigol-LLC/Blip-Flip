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
    static public GameScene Active{
			get{
				if( _gameScene == null ){
					_gameScene = FindObjectOfType<GameScene>();
				}
				return _gameScene;
			}
		}

    [SerializeField] private StackMoveObject moveBarrier = null;
    [SerializeField] private AutoMoveObject moveBackground = null;
    [SerializeField] private Player _player;

    private Label count_label;
    private int currentShow = 1;
    [SerializeField] private AudioSource musicMenu = null;
    [SerializeField] private AudioSource musicGame = null;
    [SerializeField] private AudioClip clipDestroy = null;

    [SerializeField] private AudioClip clipStart = null;
    [SerializeField] private AudioClip clipButtonClick = null;
    [SerializeField] private AudioClip clipSwapPlayer = null;

    [SerializeField] private AudioClip clipChangeView = null;
    [SerializeField] private AudioClip clipScore = null;

    [SerializeField] private float lenghtMoveTouch = 10.0f;
    private Vector2 touchBegin = Vector2.zero;

    private Animator _playerAnimator;
    [SerializeField] private float animationSpeedKoef = 1.0f;
    [SerializeField] private float speedUpTimeMult = 1.0f;
    [SerializeField] private float speedUpTimeAdd = 0.0f;

    [SerializeField] private bool musicPlay = true;
    [SerializeField] private int[] arraySlideObject = {
        1,
        2,
        3,
        4
    };
    [SerializeField] private bool allowCircleSlide = true;

    private string[] _playerSides = {
        "up",
        "down"
    };

    private bool _playerSide;
    private bool touch = true;
    private int currentRestart;
    private int indexSlide = 0;
    private int slideInCurrentTouch;
    private GameObject lastCompliteObject;

    [SerializeField] private SettingProject _setting = null;

    [SerializeField] private GameObject tutorialSlide;
    private bool isTutorial = true;
    private float tutorialBaseSpeed = -1.5f;

    [SerializeField] private float tutorialSpeedKoef = 16.0f;
    [SerializeField] private float tutorialMotionDump = 0.97f;

    private int lastMoveBarrier = 0;
    [SerializeField] private int startMoveObject = 0;

    private bool animatorPlay;

    private GameObject tutorialFindObject;
    public int CountScore { get; set; }
    private const float _minEpsilon = 5.0f;

    #endregion

    #region Constructor

    public GameScene() {
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
        UnityEngine.Social.localUser.Authenticate( result => { } );
        if ( _setting != null ) {
            Chartboost.Instance().Initialize( _setting.CHARTBOOST_APPID, _setting.CHARTBOOST_SIGNATURE );
            DeviceInfo.Initialize( _setting.STAT_FOLDER_NAME, _setting.STAT_APP_NAME, _setting.STAT_URL );
            Social.Facebook.Instance().Initialize( _setting.FACEBOOK_APPID, _setting.FACEBOOK_PERMISSIONS );
            AmazonHelper.Instance().Initialize( _setting.AMAZON_ACCESS_KEY, _setting.AMAZON_SECRET_KEY );
            AmazonHelper.Instance()
                  .UploadFiles(
                          Path.Combine( Finder.SandboxPath, _setting.STAT_FOLDER_NAME ),
                          _setting.AMAZON_STAT_BUCKET,
                          new[] {
                              "txt"
                          },
                          true );
            DeviceInfo.CollectAndSaveInfo();
        }
//        initTutorial();
    }

    private void GameOver() {
        if ( musicPlay && clipDestroy != null ) {
            AudioSource.PlayClipAtPoint( clipDestroy, Vector3.zero );
        }
        moveBackground.Pause = true;
        if ( moveBarrier.CurrentMoveObject() != null ) {
            moveBarrier.CurrentMoveObject().Pause = true;
        }
        _player.Pause = true;
        Camera.main.animation.Play();
        _playerAnimator.speed = 1.0f;
        _playerAnimator.Play( "death_" + _playerSides[ _playerSide ? 1 : 0 ] );
        PlayerPrefs.SetInt( "MoveBarrier", moveBarrier.CurrentIndex );
        if ( ( currentRestart % 5 ) == 0 ) {
            //Debug.Log("Show Chartboost");
            Chartboost.Instance().CacheMoreApps( null );
        }
        StartCoroutine( "ShowGameOverView" );
    }

    private void PauseGame( bool key ) {
        moveBackground.Pause = key;
        if ( moveBarrier.CurrentMoveObject() != null ) {
            moveBarrier.CurrentMoveObject().Pause = key;
        }
        _player.Pause = key;
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

    private void Over() {
        
    }

    private void Game() {
        ViewManager.Active.GetViewById( "PauseCounter" ).IsVisible = false;
        if ( _player != null ) {
            PauseGame( false );
            return;
        }
        GameObject go = Instantiate( Resources.Load( "PlayerUnite" ) ) as GameObject;
        _player = go.GetComponent<Player>();
        go.name = "PlayerUnite";
        go.transform.parent = transform;
        _playerSide = false;
        _player.SetActionGameOver( GameOver );
        _playerAnimator = _player.GetComponent<Animator>();
        touch = true;
        moveBackground.Pause = false;
        moveBarrier.Reset();
//        Debug.Log(moveBarrier.ListMoveObject.Count);
//        Debug.Log(moveBarrier.CurrentIndex);
        moveBarrier.CurrentMoveObject().Pause = false;
        if ( musicPlay ) {
            musicMenu.Stop();
            musicGame.Play();
        }
        _player.GetComponent<VisualNode>().IsVisible = true;
        go.SetActive( true );
//        _playerAnimator.Play( "start" );
        StartCoroutine( "StartSoundPlay", 1.1f );
        _player.Pause = false;
        //if(isSlide){
//        ButtonBase bb = (ButtonBase) ViewManager.Active.GetViewById( "Game" ).GetChildById( "1" );
//        bb.State = ButtonState.Focus;
        if ( ButtonBase.focusButton != null ) {
            ButtonBase.focusButton.State = ButtonState.Default;
        }
//        ButtonBase.focusButton = bb;
        //}
        currentShow = 1;
        CountScore = 0;
        count_label.MTextMesh.text = CountScore.ToString();
    }

    private void ShowGameOverView() {
//        yield return new WaitForSeconds( 1.7f );
        StartCoroutine( "ShowScore", 0.02f );
//        int bestResult = Mathf.Max( CountScore, PlayerPrefs.GetInt( "bestResult" ) );
//        UnityEngine.Social.ReportScore(
//                bestResult,
//                "com.oleh.gates",
//                result => { Debug.Log( ( result ) ? "Complite send score" : "failed send score" ); } );
//        PlayerPrefs.SetInt( "bestResult", bestResult );
//        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetInt("bestResult").ToString());
        Button button = new Button();
        button.ActionName = "BTN_END";
        ViewManager.Active.GetViewById( "Game" ).RunAction( button );
//        ViewManager.Active.GetViewById( "Over" ).IsVisible = true;
//        VisualNode group = ViewManager.Active.GetViewById( "Over" ).GetChildById( "group" );
//        if ( group.GetChildById( "result" ) is Label ) {
//            ( group.GetChildById( "result" ) as Label ).MTextMesh.text = CountScore.ToString();
//            ( group.GetChildById( "bestResult" ) as Label ).MTextMesh.text = bestResult.ToString();
//        }
        if ( musicPlay ) {
            musicMenu.Play();
            musicGame.Stop();
        }
        moveBarrier.Reset();
        Destroy( tutorialSlide );
        Destroy( _player.gameObject );
        _player = null;
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
        if ( _playerAnimator != null &&
             _playerAnimator.GetCurrentAnimatorStateInfo( 0 ).nameHash ==
             Animator.StringToHash( "Base Layer.idle_" + _playerSides[ _playerSide ? 1 : 0 ] ) ) {
            string playState = GetAnimationName( stateName );
            _playerAnimator.Play( playState );
            SetSide( stateName );
        }
    }

    private string GetAnimationName( string stateName ) {
        return stateName + "_" + _playerSides[ _playerSide ? 1 : 0 ];
    }

    private void SetSide( string stateName ) {
        _playerSide = stateName == "transform" ? ! _playerSide : _playerSide;
    }

    private IEnumerator ShowScore( float time ) {
//        if ( musicPlay ) {
//            AudioSource.PlayClipAtPoint( clipScore, Vector3.zero );
//        }
        int current = 0;
        VisualNode group = ViewManager.Active.GetViewById( "Over" ).GetChildById( "group" );
        while ( current <= CountScore ) {
            current++;
            if ( group.GetChildById( "result" ) is Label ) {
                ( group.GetChildById( "result" ) as Label ).MTextMesh.text = current.ToString();
                //(group.GetChildById("bestResult") as Label).MTextMesh.text = bestResult.ToString();
            }
            yield return new WaitForSeconds( time );
        }
    }

    private void Menu() {
        ViewManager.Active.GetViewById( "Game" ).IsVisible = false;
        if ( moveBarrier != null ) {
            moveBarrier.Reset();
            Destroy( tutorialSlide );
        }
        if ( _player != null ) {
            Destroy( _player.gameObject );
            _player = null;
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
        count_label = (Label) ViewManager.Active.GetViewById( "Game" ).GetChildById( "count" );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "BTN_PLAY", StartGame );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "GameCentr", GameCentr );
//        ViewManager.Active.GetViewById( "Start" ).SetDelegate( "BTN_MUSIC", ChangeMusic );
        moveBackground.Pause = false;
        ViewManager.Active.GetViewById( "SplashScreen" ).IsVisible = false;
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = true;
//        ViewManager.Active.GetViewById( "Start" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Over" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Game" ).SetSingleAction( ButtonClick );
//        ViewManager.Active.GetViewById( "Info" ).SetSingleAction( ButtonClick );
    }

    private IEnumerator StartAnimationPlay( float time ) {
        animatorPlay = true;
        yield return new WaitForSeconds( time );
        animatorPlay = false;
    }

    private IEnumerator StartSoundPlay( float time ) {
        yield return new WaitForSeconds( time );
        if ( musicPlay && clipStart != null ) {
            AudioSource.PlayClipAtPoint( clipStart, Vector3.zero );
        }
    }

    private void Update() {
        AutoMoveObject currMove = moveBarrier.CurrentMoveObject();
        List<GameObject> listGo = currMove.ListActiveObject;
        GameObject go = null;
        int indexLeft = -1;
        for ( int i = listGo.Count - 1; i >= 0; --i ) {
            if ( listGo[ i ].transform.position.x < _player.playerNode.transform.position.x ) {
                go = listGo[ i ];
                indexLeft = i;
                break;
            }
        }
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
        if ( go != lastCompliteObject ) {
            lastCompliteObject = go;
            CountScore++;
            count_label.MTextMesh.text = CountScore.ToString();
            moveBarrier.CurrentMoveObject().speed.x *= speedUpTimeMult;
            moveBarrier.CurrentMoveObject().speed.x += speedUpTimeAdd;
        }
    }

    private void initTutorial() {
        AudioClip ac = null;
        if ( PlayerPrefs.HasKey( "MoveBarrier" ) ) {
            moveBarrier.CurrentIndex = Mathf.Min( startMoveObject, PlayerPrefs.GetInt( "MoveBarrier" ) );
        }
        isTutorial = true;
        if ( PlayerPrefs.HasKey( "ShowTutorial" ) ) {
            int showTutorial = PlayerPrefs.GetInt( "ShowTutorial" );
            if ( showTutorial == 0 ) {
                isTutorial = false;
            }
        } else {
            PlayerPrefs.SetInt( "ShowTutorial", 1 );
        }
        if ( isTutorial ) {
            tutorialSlide = Instantiate( Resources.Load( "TutorialSlide" ) ) as GameObject;
            tutorialSlide.SetActive( false );
            tutorialBaseSpeed = moveBarrier.CurrentMoveObject().speed.x;
            Debug.Log( tutorialBaseSpeed.ToString() );
        }
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

    private void PauseCounter() {
//        PauseGame( false );
    }

    private void Pause() {
//        ViewManager.Active.GetViewById( "ViewStart" ).IsVisible = false;
        PauseGame( true );
//        ViewManager.Active.GetViewById( "Pause" ).IsVisible = true;
    }

    private void Info() {
//        ViewManager.Active.GetViewById( "Start" ).IsVisible = false;
//        ViewManager.Active.GetViewById( "Info" ).IsVisible = true;
    }

    private void ChangeMusic( ICall bb ) {
        musicPlay = !musicPlay;
        if ( musicPlay ) {
            musicMenu.Play();
        } else {
            musicMenu.Stop();
        }
        PlayerPrefs.SetInt( "music", ( musicPlay ) ? 1 : 0 );
    }

    private void Facebook( ICall bb ) {
        if ( !Social.Facebook.Instance().IsOpenSession ) {
            Social.Facebook.Instance().Login(
                    result => {
                        if ( !string.IsNullOrEmpty( result ) ) {
                            Social.Facebook.Instance().GetUserDetails( r => { SaveFBUserDetail( r ); } );
                        }
                    } );
        } else {
            Social.Facebook.Instance().GetUserDetails( result => { SaveFBUserDetail( result ); } );
            Social.Facebook.Instance().GoToPage( _setting.FACEBOOK_APPID );
        }
    }

    private void GameCentr( ICall bb ) {
        UnityEngine.Social.ShowLeaderboardUI();
    }

    private void GoHome( ICall bb ) {
        moveBackground.Pause = false;
    }

    private void Restart( ICall bb ) {
//        ViewManager.Active.GetViewById( "Over" ).IsVisible = false;
        Game();
        moveBackground.Pause = false;
        currentRestart++;
        if ( PlayerPrefs.HasKey( "MoveBarrier" ) ) {
            moveBarrier.CurrentIndex = Mathf.Min( startMoveObject, PlayerPrefs.GetInt( "MoveBarrier" ) );
        }
        if ( moveBarrier.CurrentIndex == 0 ) {
//            initTutorial();
        }
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
        if ( !touch ) {
            return false;
        }
        touchBegin = touchPoint;
//        _player.Up();
        return true;
    }

    public bool TouchMove( Vector2 touchPoint ) {
        if ( !touch ) {
            return false;
        }
        float verticalSlideLenght = touchBegin.y - touchPoint.y;
        float horizontalSlideLenght = touchBegin.x - touchPoint.x;
        bool vertical = Mathf.Abs( verticalSlideLenght ) > Mathf.Abs( horizontalSlideLenght );
        if ( ! vertical &&
             ( horizontalSlideLenght < -lenghtMoveTouch || horizontalSlideLenght > lenghtMoveTouch) ) {
            ShowPlayer( "slide" ); //Slide
        } else if ( verticalSlideLenght > lenghtMoveTouch /* && slideInCurrentTouch != 1*/ ) {
//			indexSlide--;
//			if(indexSlide < 0){
//				indexSlide = (allowCircleSlide)?arraySlideObject.Length - 1:0;
//			}
//			slideInCurrentTouch = 1;
//            if ( indexSlide >= 0 &&
//                 arraySlideObject.Length > indexSlide ) {
            ShowPlayer( _playerSide ? "jump" : "transform" ); //Down
//            }
        } else if ( verticalSlideLenght < -lenghtMoveTouch /* && slideInCurrentTouch != -1*/ ) {
//			indexSlide++;
//			if(indexSlide >= arraySlideObject.Length){
//				indexSlide = (allowCircleSlide)?0:arraySlideObject.Length - 1;
//			}
//			slideInCurrentTouch = -1;
//            if ( indexSlide >= 0 &&
//                 arraySlideObject.Length > indexSlide ) {
            ShowPlayer( ! _playerSide ? "jump"  : "transform" ); //Up
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
}
