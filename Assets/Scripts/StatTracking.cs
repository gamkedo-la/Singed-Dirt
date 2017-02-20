using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatTracking {
	public static float roundSeconds;
	public static float currentRound;

	public static void Reset(){
		roundSeconds = 0;
		currentRound = 0;
	}
}
