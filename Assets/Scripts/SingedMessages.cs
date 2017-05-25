using UnityEngine;
using UnityEngine.Networking;

public class SingedMessages {
    public enum SingedMessageKind {
        playAudioClip = 1000,
        playAudioLoop
    }

    public class PlayAudioClipMessage : MessageBase {
        public string resourcePath;
        public bool pitchModulation;
        public float delay;
    }

    public class PlayAudioLoopMessage : MessageBase {
        public GameObject parent;
        public string name;
        public string resourcePath;
        public bool start;
    }

    public static void SendPlayAudioClip(string resourcePath, bool pitchModulation=false, float delay=0f) {
        if (NetworkManager.singleton != null) {
            var msg = new SingedMessages.PlayAudioClipMessage();
            msg.resourcePath = resourcePath;
            msg.pitchModulation = pitchModulation;
            msg.delay = delay;
            NetworkManager.singleton.client.Send((short) SingedMessages.SingedMessageKind.playAudioClip, msg);
        }
    }
    public static void SendPlayAudioClip(TankSoundKind tankSoundKind) {
        SendPlayAudioClip(PrefabRegistry.GetResourceName<TankSoundKind>(tankSoundKind));
    }

    public static void SendStartAudioLoop(GameObject parent, string name, TankSoundKind tankSoundKind) {
        if (NetworkManager.singleton != null) {
            var msg = new SingedMessages.PlayAudioLoopMessage();
            msg.parent = parent;
            msg.resourcePath = PrefabRegistry.GetResourceName<TankSoundKind>(tankSoundKind);
            msg.name = name;
            msg.start = true;
            NetworkManager.singleton.client.Send((short) SingedMessages.SingedMessageKind.playAudioLoop, msg);
        }
    }

    public static void SendStopAudioLoop(GameObject parent, string name) {
        if (NetworkManager.singleton != null) {
            var msg = new SingedMessages.PlayAudioLoopMessage();
            msg.parent = parent;
            msg.name = name;
            msg.start = false;
            NetworkManager.singleton.client.Send((short) SingedMessages.SingedMessageKind.playAudioLoop, msg);
        }
    }

    public static void ServerRegisterMessageHandlers() {
        NetworkServer.RegisterHandler(
            (short)SingedMessages.SingedMessageKind.playAudioClip,
            OnServerPlayAudioClipMessage
        );
        NetworkServer.RegisterHandler(
            (short)SingedMessages.SingedMessageKind.playAudioLoop,
            OnServerPlayAudioLoopMessage
        );
    }

    public static void ClientRegisterMessageHandlers(NetworkClient networtClient) {
        networtClient.RegisterHandler(
            (short)SingedMessages.SingedMessageKind.playAudioClip,
            OnClientPlayAudioClipMessage
        );
        networtClient.RegisterHandler(
            (short)SingedMessages.SingedMessageKind.playAudioLoop,
            OnClientPlayAudioLoopMessage
        );
    }

    // ------------------------------------------------------
    // PlayAudioClipMessage
    private static void OnServerPlayAudioClipMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SingedMessages.PlayAudioClipMessage>();
        //Debug.Log("New network message on server: " + msg);
        // server replays original message to all clients
        NetworkServer.SendToAll((short)SingedMessages.SingedMessageKind.playAudioClip, msg);
    }

    private static void OnClientPlayAudioClipMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SingedMessages.PlayAudioClipMessage>();
        //Debug.Log("New network message on client: " + msg);
        if (SoundManager.instance != null) {
            SoundManager.instance.PlayAudioClip(msg);
        }
    }

    // ------------------------------------------------------
    // PlayAudioLoopMessage
    private static void OnServerPlayAudioLoopMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SingedMessages.PlayAudioLoopMessage>();
        //Debug.Log("New network message on server: " + msg);
        // server replays original message to all clients
        NetworkServer.SendToAll((short)SingedMessages.SingedMessageKind.playAudioLoop, msg);
    }

    private static void OnClientPlayAudioLoopMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SingedMessages.PlayAudioLoopMessage>();
        //Debug.Log("New network message on client: " + msg);
        if (SoundManager.instance != null) {
            if (msg.start) {
                SoundManager.instance.StartAudioLoop(msg.parent, msg.name, msg.resourcePath);
            } else {
                SoundManager.instance.StopAudioLoop(msg.parent, msg.name);
            }
        }
    }


}
