using UnityEngine;
using System.Collections.Generic;



public class AutoMoveObject : MonoBehaviour {
	
	[SerializeField]
	public Vector2 speed = Vector2.zero;

	private Vector2 startSpeed = Vector2.zero;

	[SerializeField]
	private float currentPosition = 0;
	[SerializeField]
	private float createNewObject = 10;
	[SerializeField]
	private Vector2 objectCreatePostion = Vector2.zero;
	[SerializeField]
	private Rect randomOffset = new Rect(0,0,0,0);
	[SerializeField]
	private int countCreateObject = -1;

	private bool isDone = false;
	[SerializeField]
	private ObjectPool _objectPool = null;

	[SerializeField]
	private bool isLimitedPosition = false;
	[SerializeField]
	private Vector2 limitPosition = new Vector2(4000,4000);

	[HideInInspector,SerializeField]
	private MapStringAnimationClip animationEventClips = new MapStringAnimationClip();

	[SerializeField]
	private bool _pause = true;

	private List<GameObject> listGo = new List<GameObject>();
	private int _countGen = 0;

	public delegate void callbackCount(int count);
	callbackCount _callbackCount = null;
	
	// Use this for initialization
	void Start () {
		startSpeed = speed;
		//listGo = UIEditor.Node.NodeContainer.GetAllChildren(transform);
	}
	public int CountActiveObject{
		get{
			return listGo.Count;
		}
	}
	public Vector2 LimitPosition{
		set{
			limitPosition = value;
		}
		get{
			return limitPosition;
		}
	}
	public bool IsLimitedPosition{
		get{
			return isLimitedPosition;
		}
		set{
			isLimitedPosition = value;
		}
	}
	public MapStringAnimationClip AnimationEventClips{
		get{
			return animationEventClips;
		}
		set{
			animationEventClips = value;
		}
	}
	public bool Pause{
		set{
			_pause = value;
		}
		get{
			return _pause;
		}
	}
	public List<GameObject> ListActiveObject{
		get{
			return listGo;
		}
	}
	public void SetCallBackCount(callbackCount _delegate){
		_callbackCount = _delegate;
	}
	public void Clear(){
		foreach(var go in listGo){
			Destroy(go);
		}
		listGo.Clear();
	}
	public void Reset(){
		speed = startSpeed;
		Clear();
		transform.position = new Vector3(0,0,transform.position.z);
		isDone = false;
		_countGen = 0;
		_objectPool.Reset();
	}
	// Update is called once per frame
	void Update () {
		if(_pause)
			return;
		transform.Translate(speed.x,speed.y,0,Space.Self);
		currentPosition += Mathf.Abs(speed.x) + Mathf.Abs(speed.y);
		if(currentPosition >= createNewObject){
			currentPosition -= createNewObject;
			CreateObject();
		}
		if(isLimitedPosition)
			RemoveBorder();
		if(isDone){
			Debug.Log(listGo.Count);
			_pause = true;
		}
	}
	void RemoveBorder(){
		foreach(var go in listGo){
			if(Mathf.Abs(go.transform.position.x) > limitPosition.x || Mathf.Abs(go.transform.position.y) > limitPosition.y){
				listGo.Remove(go);
				Destroy(go);
				break;
			}

		}
	}
	public bool IsDone{
		get{
			return isDone;
		}
	}
	public int CountGeneratedObject{
		get{
			return _countGen;
		}
		set{
			_countGen = value;
			if(_callbackCount != null){
				_callbackCount(_countGen);
			}
		}
	}
	public void CreateObject(){
		if(countCreateObject != -1 && _countGen >= countCreateObject){
			isDone = true;
			return;
		}
		GameObject ret  = _objectPool.GetObject();
		if(ret != null){
			CountGeneratedObject++;
			GameObject go = GameObject.Instantiate(ret) as GameObject;
			float z = go.transform.localPosition.z;
			go.transform.position = new Vector3(objectCreatePostion.x,objectCreatePostion.y,go.transform.position.z);
			go.transform.parent = transform;
			go.transform.localPosition = new Vector3(go.transform.localPosition.x,go.transform.localPosition.y,z);
			go.transform.Translate(Random.Range(randomOffset.x,randomOffset.width),Random.Range(randomOffset.y,randomOffset.height),0);
			go.transform.localScale = ret.transform.localScale;
			go.name = ret.name;
			listGo.Add(go);

		}

	}
}
