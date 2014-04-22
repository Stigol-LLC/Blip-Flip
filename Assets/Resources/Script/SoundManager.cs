#region Usings

using System;
using System.Linq;
using System.Reflection;
using UIEditor.Act;
using UIEditor.Core;
using UIEditor.Node;
using UIEditor.Util;
using UnityEngine;

#endregion

internal class SoundManager : MonoBehaviour, IActionHandler {
    #region Properties

    private static SoundManager _instance;
    public static SoundManager Active {
        get {
            if ( _instance == null ) {
                _instance = FindObjectOfType<SoundManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private bool _musicPlay = true;
    [SerializeField] private AudioSource _musicMenu = null;
    [SerializeField] private AudioSource _musicGame = null;

    [SerializeField] private AudioClip _clipStart = null;
    [SerializeField] private AudioClip _clipDeath = null;
    [SerializeField] private AudioClip _clipJumpPlayer = null;
    [SerializeField] private AudioClip _clipSlidePlayer = null;
    [SerializeField] private AudioClip _clipTransformPlayer = null;

    [SerializeField] private AudioClip _clipButtonClick = null;
    [SerializeField] private AudioClip _clipScore = null;

    private AudioSource[] _audioSources;
    private AudioClip[] _audioClips;

    #endregion

    #region Constructor

    private SoundManager() {
    }

    #endregion

    #region IActionHandler Members

    public void RunAction( ICall ic ) {
        PlayClip( "button" );
    }

    #endregion

//    public void OnEnterGame() {
//        if ( !_musicPlay ) {
//            return;
//        }
//        PlaySource( _musicGame );
//    }
//
//    public void OnEnterMenu() {
//        if ( !_musicPlay ) {
//            return;
//        }
//        PlaySource( _musicMenu );
//    }

    //TODO Audio & slide skip speed koef

    #region Methods

    public void PlayClip( AudioClip audioClip ) {
        if ( _musicPlay && audioClip != null ) {
            AudioSource.PlayClipAtPoint( audioClip, Vector3.zero );
        }
    }

    public void PlayClip( string stateName ) {
        PlayClip( _clipButtonClick );
    }

    public void PlaySource( AudioSource audioSource ) {
        if ( !_musicPlay ||
             _audioSources == null ||
             audioSource.isPlaying ) {
            return;
        }
        foreach ( AudioSource source in _audioSources.Where( source => source != null ) ) {
            source.Stop();
        }
        audioSource.Play();
    }

    private void Awake() {
        ViewManager.Active.SoundManager = this;
    }

    private void ChangeMusic( ICall iCall ) {
        _musicPlay = !_musicPlay;
        if ( _musicPlay ) {
            if ( iCall.ActionName == "BTN_MENU_MUSIC" ) {
                _musicMenu.Play();
            } else if ( iCall.ActionName == "BTN_GAME_MUSIC" ) {
                _musicGame.Play();
            }
        } else {
            if ( iCall.ActionName == "BTN_MENU_MUSIC" ) {
                _musicMenu.Pause();
            } else if ( iCall.ActionName == "BTN_GAME_MUSIC" ) {
                _musicGame.Pause();
            }
        }
        PlayerPrefs.SetInt( "music", ( _musicPlay ) ? 1 : 0 );
    }

    //TODO Load from resources
    private AudioClip[] GetAudioClips() {
        return
                GetType()
                        .GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
                        .Where( field => field.FieldType == typeof (AudioClip) )
                        .Select( field => field.GetValue( this ) as AudioClip )
                        .ToArray();
    }

    private AudioSource[] GetAudioSources() {
        return
                GetType()
                        .GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
                        .Where( field => field.FieldType == typeof (AudioSource) )
                        .Select( field => field.GetValue( this ) as AudioSource )
                        .ToArray();
    }

    private AudioClip GetClipName( string stateName ) {
        if ( ! string.IsNullOrEmpty( stateName ) ) {
            return _audioClips.First(
                    clip => clip.ToString().Contains( stateName, StringComparison.OrdinalIgnoreCase ) );
        }
        return null;
    }

    private void Start() {
        _audioSources = GetAudioSources();
        _audioClips = GetAudioClips();
        ViewManager.Active.GetViewById( "Menu" ).SetDelegate( "BTN_MENU_MUSIC", ChangeMusic );
        ViewManager.Active.GetViewById( "Pause" ).SetDelegate( "BTN_GAME_MUSIC", ChangeMusic );
    }

    private void Menu() {
        PlaySource( _musicMenu );
    }

    private void Info() {
    }

    private void Game() {
        PlaySource( _musicGame );
    }

    private void Pause() {
    }

    private void PauseCounter() {
    }

    private void Over() {
    }

    #endregion
}