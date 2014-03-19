using UnityEngine;
using System.Collections.Generic;

public class StackMoveObject : MonoBehaviour {

	[SerializeField]
	List<AutoMoveObject> listMoveObject = new List<AutoMoveObject>();
	int currentIndex = 0;
	int countCreate = 0;

	public List<AutoMoveObject> ListMoveObject{
		get{
			return listMoveObject;
		}
	}
	public void Reset(){
		countCreate  = 0;
		foreach(var o in listMoveObject){
			o.Reset();
		}
	}
	public int CurrentIndex{
		set{
			currentIndex = value;
		}
		get{
			return currentIndex;
		}
	}
	void Start() {
		foreach(var l in listMoveObject){
			l.SetCallBackCount(CountDelegate);
		}
	}
	public int CountCreateInStack{
		get{
			return countCreate;
		}
	}
	void CountDelegate(int c){
		countCreate++;
	}
	// Update is called once per frame
	void Update () {
		if(listMoveObject.Count > currentIndex && listMoveObject[currentIndex].IsDone){
			currentIndex++;
			listMoveObject[currentIndex].Pause = false;
		}
	}
	public AutoMoveObject CurrentMoveObject(){
		if(listMoveObject.Count > currentIndex)
			return listMoveObject[currentIndex];
		else
			return null;
	}

}
