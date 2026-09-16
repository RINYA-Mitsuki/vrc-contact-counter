#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;

namespace MitsuboshiStudio
{
    public class VRCContactCounter_v1 : EditorWindow
    {
        private GameObject targetRoot;
        private Vector2 scroll;

        private int senderCount;
        private int receiverCount;
        private int totalCount;

        private int enabledSenderCount;
        private int enabledReceiverCount;

        private int editorOnlySenderCount;
        private int editorOnlyReceiverCount;

        [MenuItem("Tools/Mitsuboshi_Studio/VRC Contact Counter")]
        private static void OpenWindow()
        {
            var window = GetWindow<VRCContactCounter_v1>("VRC Contact Counter");
            window.minSize = new Vector2(420f, 300f);
            window.SyncFromSelection();
        }

        private void OnEnable()
        {
            SyncFromSelection();
        }

        private void OnSelectionChange()
        {
            SyncFromSelection();
            Repaint();
        }

        private void SyncFromSelection()
        {
            if (Selection.activeGameObject != null)
            {
                targetRoot = Selection.activeGameObject;
                CountContacts();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("VRC Contact Counter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "選択したGameObjectをルートとして、そのオブジェクト自身と全ての子階層に存在する " +
                "VRC Contact Sender / Receiver を集計します。非アクティブなGameObject上のContactも含みます。",
                MessageType.Info);

            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            targetRoot = (GameObject)EditorGUILayout.ObjectField(
                "Target Root",
                targetRoot,
                typeof(GameObject),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                CountContacts();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Object"))
                {
                    targetRoot = Selection.activeGameObject;
                    CountContacts();
                }

                using (new EditorGUI.DisabledScope(targetRoot == null))
                {
                    if (GUILayout.Button("Recount"))
                    {
                        CountContacts();
                    }
                }
            }

            EditorGUILayout.Space(10);

            if (targetRoot == null)
            {
                EditorGUILayout.HelpBox("Hierarchy上のGameObjectを選択してください。", MessageType.Warning);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Count Result", EditorStyles.boldLabel);

            DrawCountRow("VRC Contact Sender", senderCount);
            DrawCountRow("VRC Contact Receiver", receiverCount);

            EditorGUILayout.Space(4);

            GUIStyle totalStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            EditorGUILayout.LabelField($"Total: {totalCount} / 256", totalStyle);

            if (totalCount > 256)
            {
                EditorGUILayout.HelpBox(
                    "256を超えています。VRChatのVRC Contactハードリミットを超過しています。",
                    MessageType.Error);
            }
            else if (totalCount > 220)
            {
                EditorGUILayout.HelpBox(
                    "256個の上限にかなり近づいています。",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(12);

            EditorGUILayout.LabelField("Additional Info", EditorStyles.boldLabel);
            DrawCountRow("Enabled Sender Components", enabledSenderCount);
            DrawCountRow("Enabled Receiver Components", enabledReceiverCount);
            DrawCountRow("Disabled Sender Components", senderCount - enabledSenderCount);
            DrawCountRow("Disabled Receiver Components", receiverCount - enabledReceiverCount);

            EditorGUILayout.Space(6);

            EditorGUILayout.LabelField("Under EditorOnly", EditorStyles.boldLabel);
            DrawCountRow("Sender", editorOnlySenderCount);
            DrawCountRow("Receiver", editorOnlyReceiverCount);
            DrawCountRow("Total", editorOnlySenderCount + editorOnlyReceiverCount);

            EditorGUILayout.HelpBox(
                "EditorOnly配下の値は、選択ルートからそのContactまでの親階層に " +
                "EditorOnlyタグのGameObjectが存在するものを数えています。",
                MessageType.None);

            EditorGUILayout.EndScrollView();
        }

        private void DrawCountRow(string label, int count)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label);
                EditorGUILayout.LabelField(count.ToString(), GUILayout.Width(70f));
            }
        }

        private void CountContacts()
        {
            senderCount = 0;
            receiverCount = 0;
            totalCount = 0;

            enabledSenderCount = 0;
            enabledReceiverCount = 0;

            editorOnlySenderCount = 0;
            editorOnlyReceiverCount = 0;

            if (targetRoot == null)
                return;

            var senders = targetRoot.GetComponentsInChildren<VRCContactSender>(true);
            var receivers = targetRoot.GetComponentsInChildren<VRCContactReceiver>(true);

            senderCount = senders.Length;
            receiverCount = receivers.Length;
            totalCount = senderCount + receiverCount;

            foreach (var sender in senders)
            {
                if (sender != null && sender.enabled)
                    enabledSenderCount++;

                if (sender != null && IsUnderEditorOnly(sender.transform))
                    editorOnlySenderCount++;
            }

            foreach (var receiver in receivers)
            {
                if (receiver != null && receiver.enabled)
                    enabledReceiverCount++;

                if (receiver != null && IsUnderEditorOnly(receiver.transform))
                    editorOnlyReceiverCount++;
            }

            Repaint();
        }

        private bool IsUnderEditorOnly(Transform current)
        {
            Transform root = targetRoot != null ? targetRoot.transform : null;

            while (current != null)
            {
                if (current.gameObject.tag == "EditorOnly")
                    return true;

                if (current == root)
                    break;

                current = current.parent;
            }

            return false;
        }
    }
}
#endif
