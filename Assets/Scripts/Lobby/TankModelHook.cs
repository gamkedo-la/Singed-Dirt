using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Abstract class allowing the passing of state from lobby player to game player.
/// Subclass this and redefine the function you want then add it to the lobby prefab
/// </summary>
public abstract class TankModelHook : MonoBehaviour {

    public virtual void SetupPlayer(
        NetworkManager manager,
        GameObject lobbyPlayer,
        GameObject gamePlayer
    ) {
    }
    
}
