#region Usings

using System.Collections.Generic;
using UnityEngine;

#endregion

public class StackMoveObject : MonoBehaviour {
    #region Properties

    [SerializeField] private List<AutoMoveObject> listMoveObject = new List<AutoMoveObject>();
    private int currentIndex;
    private int countCreate;

    public List<AutoMoveObject> ListMoveObject {
        get { return listMoveObject; }
    }

    public int CurrentIndex {
        set { currentIndex = value; }
        get { return currentIndex; }
    }

    public int CountCreateInStack {
        get { return countCreate; }
    }

    #endregion

    #region Methods

    public AutoMoveObject CurrentMoveObject() {
        if ( listMoveObject.Count > currentIndex ) {
            return listMoveObject[ currentIndex ];
        }
        return null;
    }

    public void Reset() {
        countCreate = 0;
        foreach ( var o in listMoveObject ) {
            o.Reset();
        }
    }

    private void CountDelegate( int c ) {
        countCreate++;
    }

    private void Start() {
        foreach ( var l in listMoveObject ) {
            l.SetCallBackCount( CountDelegate );
        }
    }

    private void Update() {
        if ( listMoveObject.Count > currentIndex &&
             listMoveObject[ currentIndex ].IsDone ) {
            currentIndex++;
            listMoveObject[ currentIndex ].Pause = false;
        }
    }

    #endregion
}