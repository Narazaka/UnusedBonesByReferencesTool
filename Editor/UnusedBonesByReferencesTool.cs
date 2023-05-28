using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Narazaka.Unity.BoneTools
{
    public class UnusedBonesByReferencesTool : EditorWindow
    {
        [MenuItem("Window/UnusedBonesByReferencesTool")]
        public static void Open()
        {
            GetWindow<UnusedBonesByReferencesTool>(nameof(UnusedBonesByReferencesTool));
        }

        public Transform HumanoidRoot;
        public bool PreserveEndBone = true;
        public bool DetectExtraChild;

        void OnGUI()
        {
            HumanoidRoot = EditorGUILayout.ObjectField("アバター", HumanoidRoot, typeof(Transform), true) as Transform;
            EditorGUIUtility.labelWidth = 250;
            PreserveEndBone = EditorGUILayout.Toggle("親がactiveなendボーンを削除しない(ON推奨)", PreserveEndBone);
            DetectExtraChild = EditorGUILayout.Toggle("子にボーン以外を持つボーンを削除しない", DetectExtraChild);
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.HelpBox("EditorOnlyなメッシュのみから参照されているボーンをEditorOnlyにします", MessageType.Info);
            EditorGUI.BeginDisabledGroup(HumanoidRoot == null);
            if (GUILayout.Button("Set EditorOnly!"))
            {
                var unused = UnusedBonesByReferences.Make(BoneReference.Make(HumanoidRoot, DetectExtraChild), PreserveEndBone);
                unused.SetEditorOnlyToBones();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
