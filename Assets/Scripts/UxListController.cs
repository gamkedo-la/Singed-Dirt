using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// ========================================================================
/// <summary>
/// Class to manage the list of spawns
/// </summary>
public class UxListController : MonoBehaviour {
    public int maxEntries = 25;
    public bool purge = true;

    // ------------------------------------------------------
    // STATIC VARIABLES

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public RectTransform uxListContentTransform;

    // ------------------------------------------------------
    // INSTANCE VARIABLES
    protected VerticalLayoutGroup layoutGroup;
    protected List<UxListElement> uxList = new List<UxListElement>();

    public void OnEnable() {
        layoutGroup = uxListContentTransform.GetComponent<VerticalLayoutGroup>();
    }

    void Update() {
        if (layoutGroup != null) {
            layoutGroup.childAlignment = Time.frameCount%2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }
    }

    public void Add(UxListElement listElement) {
        if (uxList.Contains(listElement)) return;

        // if list is full...
        if (uxList.Count >= maxEntries) {
            if (purge) {
                var element = uxList[0];
                uxList.RemoveAt(0);
                Destroy(element.gameObject);
            } else {
                return;
            }
        }

        // add listElement
        uxList.Add(listElement);

        // update parent and adjust add button row
        listElement.transform.SetParent(uxListContentTransform, false);

        // notify listElement of list modification
        UxListModified();

        // register self w/ onDelete event of child
        listElement.onDeleteEvent.AddListener(() => { Remove(listElement); });
    }

    public void Remove(UxListElement listElement) {
        uxList.Remove(listElement);
        UxListModified();
    }

    public void UxListModified() {
        int i = 0;
        foreach (UxListElement listElement in uxList) {
            listElement.OnListChanged(i);
            ++i;
        }
    }

}
