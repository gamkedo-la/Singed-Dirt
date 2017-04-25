using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UxListElement : MonoBehaviour {
    public UnityEvent onDeleteEvent;

    public virtual void OnListChanged(int index) {
    }

    void Awake() {
        onDeleteEvent = new UnityEvent();
    }

    public void OnDestroy() {
        onDeleteEvent.Invoke();
    }
}
