using System;
using System.Text;
using AnyConnection;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEditor;

public class HostConnectionExample : EditorWindow
{
    public static readonly Guid kMsgSendEditorToPlayer = new Guid("01E82D47-267D-48F7-AAE3-D5AD4B2F22D6");
    public static readonly Guid kMsgSendPlayerToEditor = new Guid("4A7CF7BB-362D-4045-BB10-7A72D710DCB3");

    [MenuItem("Test/HostConnectionExample")]
    static void Init()
    {
        HostConnectionExample window = (HostConnectionExample)EditorWindow.GetWindow(typeof(HostConnectionExample));
        window.Show();
        window.titleContent = new GUIContent("HostConnectionExample");
    }

    void OnEnable()
    {
        HostConnection.Instance.Initialize();
        HostConnection.Instance.Register(kMsgSendPlayerToEditor, OnMessageEvent);
    }

    void OnDisable()
    {
        HostConnection.Instance.Unregister(kMsgSendPlayerToEditor, OnMessageEvent);
        HostConnection.Instance.DisconnectAll();
    }

    private void OnMessageEvent(MessageEventArgs args)
    {
        var text = Encoding.ASCII.GetString(args.data);
        Debug.Log("Message from player: " + text);
    }

    void OnGUI()
    {
        var playerCount = HostConnection.Instance.GetConnectedPlayersCount();
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(string.Format("{0} players connected.", playerCount));
        int i = 0;
        foreach (var p in HostConnection.Instance.EnumerateConnectedPlayers())
        {
            builder.AppendLine(string.Format("[{0}] - {1} {2}", i++, p.name, p.playerId));
        }
        EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);

        if (GUILayout.Button("Send message to player"))
        {
            HostConnection.Instance.Send(kMsgSendEditorToPlayer, Encoding.ASCII.GetBytes("Hello from Editor"));
        }
    }
}