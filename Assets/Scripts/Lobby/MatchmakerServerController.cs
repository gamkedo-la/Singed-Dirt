using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;

public class MatchmakerServerController : MonoBehaviour {
    public RectTransform serverListRect;
    public GameObject serverEntryPrefab;
    public GameObject noServerFound;

    protected int currentPage = 0;
    protected int previousPage = 0;

    static Color OddServerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    static Color EvenServerColor = new Color(.94f, .94f, .94f, 1.0f);

    void OnEnable() {
        currentPage = 0;
        previousPage = 0;

        // delete any existing entries in the server list
        foreach (Transform t in serverListRect) {
            Destroy(t.gameObject);
        }

        // disable text warning no entries found
        noServerFound.SetActive(false);

        // request first page of servers from matchmaker
        RequestPage(0);
    }

	public void MatchListCallback(
        bool success,
        string extendedInfo,
        List<MatchInfoSnapshot> matches
    ) {
		if (matches.Count == 0) {
            if (currentPage == 0) {
                noServerFound.SetActive(true);
            }

            currentPage = previousPage;
            return;
        }

        // clear any current state for server list
        noServerFound.SetActive(false);
        foreach (Transform t in serverListRect) {
            Destroy(t.gameObject);
        }

        // repopulate
		for (int i = 0; i < matches.Count; ++i) {
            GameObject o = Instantiate(serverEntryPrefab) as GameObject;
			o.GetComponent<LobbyMatchEntry>().Populate(matches[i], (i % 2 == 0) ? OddServerColor : EvenServerColor);
			o.transform.SetParent(serverListRect, false);
        }
    }

    public void ChangePage(
        int dir
    ) {
        int newPage = Mathf.Max(0, currentPage + dir);

        //if we have no server currently displayed, need we need to refresh page0 first instead of trying to fetch any other page
        if (noServerFound.activeSelf) {
            newPage = 0;
        }

        RequestPage(newPage);
    }

    public void RequestPage(
        int page
    ) {
        previousPage = currentPage;
        currentPage = page;
        var lobbyManager = SingedLobbyManager.s_singleton;
		lobbyManager.matchMaker.ListMatches(page, 6, "", true, 0, 0, MatchListCallback);
	}
}
