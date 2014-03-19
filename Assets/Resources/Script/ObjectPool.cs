using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {
//	[SerializeField]
//	public int loop = 1;
	[SerializeField]
	public List<GameObject> listObject = new List<GameObject>();

	[SerializeField]
	public List<GameObject> genListObject = new List<GameObject>();
	[SerializeField]
	public bool onlyOneGen = false;

	[SerializeField]
	public bool disableTwoEqual = true;

	[SerializeField]
	public int currentPosition = 0;

	private GameObject lastGen = null;

	public void Reset(){
		lastGen = null;
		currentPosition = 0;
	}
	public GameObject GetObject(){
		GameObject mRef = null;
		if(listObject.Count == 0){
			return mRef;
		}

		if(currentPosition >= listObject.Count){
			currentPosition = 0;
		}
		mRef = listObject[currentPosition];
		if(genListObject.Count != 0 && mRef == null){
			mRef = genListObject[UnityEngine.Random.Range(0,genListObject.Count)];

			if(disableTwoEqual){
				while(mRef == lastGen){
					mRef = genListObject[UnityEngine.Random.Range(0,genListObject.Count)];
				}
				lastGen = mRef;
			}
			if(onlyOneGen){
				listObject[currentPosition] = mRef;
			}
		}
		currentPosition++;
		return mRef;
	}
}
