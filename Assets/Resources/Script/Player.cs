#region Usings

using System;
using System.Collections.Generic;
using UIEditor.Core;
using UIEditor.Node;
using UIEditor.Util;
using UnityEngine;

#endregion

[RequireComponent( typeof (Animator) )]
public class Player : MonoBehaviour {
    #region Properties

    private Action _actionGameOver;

    [SerializeField] public float _speedDown = 1.0f;
    [SerializeField] public float _stepUp = 50.0f;
    [SerializeField] private List<PlayerChild> listChild = new List<PlayerChild>();
    [SerializeField] public GameObject playerNode = null;
    private Animator _animator;
    public Animator Animator {
        get { return _animator == null ? _animator = GetComponent<Animator>() : _animator; }
    }

    private bool _pause;

    public Color ChildColor {
        set {
            foreach ( var go in listChild ) {
                go.GetComponent<SpriteRenderer>().color = value;
            }
        }
        get { return listChild[ 0 ].GetComponent<SpriteRenderer>().color; }
    }

    public string RunNextState {
        set {
            Animator.Play( value );
            Button button = new Button();
            button.ActionName = "BTN_END";
            ViewManager.Active.GetViewById( "Game" ).RunAction( button );
        }
    }

    public bool Pause {
        get { return _pause; }
        set { _pause = value; }
    }

    #endregion

    #region Methods

    public void Reset() {
        foreach ( var go in listChild ) {
            go.GetComponent<SpriteRenderer>().color = Color.white;
            go.IsCollision = false;
        }
    }

    public void SetActionGameOver( Action act ) {
        _actionGameOver = act;
    }

    public void Up() {
//		if(gameObject.activeSelf)
        transform.Translate( 0, _stepUp, 0 );
    }

    private void OnTriggerEnter2D( Collider2D other ) {
        if ( other.GetComponent<Player>() == null ) {
            _actionGameOver();
        }
    }

    private void Start() {
        gameObject.SetLayer( 0 );
    }

    private void Update() {
        if ( _pause ) {
            return;
        }
        bool haveCollision = false;
        foreach ( var go in listChild ) {
            if ( go != null &&
                 go.IsCollision ) {
                haveCollision = true;
            }
        }
        if ( haveCollision ) {
            _actionGameOver();
        }
        transform.Translate( 0, -_speedDown, 0 );
    }

    #endregion
}