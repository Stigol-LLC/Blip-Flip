using UnityEngine;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour {

	private Action _actionGameOver = null;

	[SerializeField]
	public float _speedDown = 1.0f;
	[SerializeField]
	public float _stepUp = 50.0f;
	[SerializeField]
	List<PlayerChild> listChild = new List<PlayerChild>();
	[SerializeField]
	public GameObject playerNode = null;

	private bool _pause = false;
	public void SetActionGameOver(Action act){
		_actionGameOver = act;
	}
	public Color ChildColor{
		set{
			foreach(var go in listChild){
				go.GetComponent<SpriteRenderer>().color = value;
			}
		}
		get{
			return listChild[0].GetComponent<SpriteRenderer>().color;
		}
	}
	void Update(){
		if(_pause)
			return;
		bool haveCollision = false;
		foreach(var go in listChild){
			if(go.IsCollision){
				haveCollision = true;
			}
		}
		if(haveCollision == true){
			_actionGameOver();
		}
		transform.Translate(0,-_speedDown,0);
	}
	void OnTriggerEnter2D(Collider2D other) {
		if(other.GetComponent<Player>() == null){
			_actionGameOver();
		}
	}
	public void Reset(){
		foreach(var go in listChild){
			go.GetComponent<SpriteRenderer>().color = Color.white;
			go.IsCollision = false;
		}
	}
	public bool Pause{
		get{
			return _pause;
		}
		set{
			_pause = value;
		}
	}
	public void Up(){
//		if(gameObject.activeSelf)
			transform.Translate(0,_stepUp,0);
	}
}
