using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Narazaka.Unity.BoneTools
{
    public class UnusedBonesByReferences
    {
        public IList<BoneReference> BoneReferences { get; set; }
        public bool PreserveEndBone { get; set; }

        public HashSet<Transform> UnusedBones { get; private set; }
        public HashSet<Transform> DisabledBones { get; private set; }
        public HashSet<Transform> ForceEnabledBones { get; private set; }

        public static UnusedBonesByReferences Make(IList<BoneReference> boneReferences, bool preserveEndBone = false)
        {
            var info = new UnusedBonesByReferences { BoneReferences = boneReferences, PreserveEndBone = preserveEndBone };
            info.DetectUnusedBones();
            return info;
        }

        void DetectUnusedBones()
        {
            DisabledBones = new HashSet<Transform>();

            // 全ての参照オブジェクトがEditorOnlyならボーンもEditorOnly
            foreach (var boneReference in BoneReferences)
            {
                if (boneReference.ReferencesAllEditorOnly)
                {
                    DisabledBones.Add(boneReference.Bone);
                }
            }

            ForceEnabledBones = new HashSet<Transform>();
            foreach (var boneReference in BoneReferences)
            {
                var active = !DisabledBones.Contains(boneReference.Bone);
                if (!active && boneReference.HasExtraChild) // ボーン以外の子があったらactive
                {
                    ForceEnabledBones.Add(boneReference.Bone);
                    active = true;
                }
                if (active) // 親は全てactive
                {
                    ForceEnabledBones.UnionWith(boneReference.Parents);
                }
            }

            UnusedBones = new HashSet<Transform>(DisabledBones);

            UnusedBones.ExceptWith(ForceEnabledBones);

            if (PreserveEndBone)
            {
                // 親がactiveなEndボーンはactive
                foreach (var boneReference in BoneReferences)
                {
                    if (boneReference.IsEnd && !UnusedBones.Contains(boneReference.Parents.LastOrDefault()))
                    {
                        ForceEnabledBones.Add(boneReference.Bone);
                    }
                }
            }

            UnusedBones.ExceptWith(ForceEnabledBones);
        }

        public void SetEditorOnlyToBones()
        {

            foreach (var boneReference in BoneReferences)
            {
                var disabled = UnusedBones.Contains(boneReference.Bone);
                var currentDisabled = boneReference.Bone.CompareTag("EditorOnly");
                if (currentDisabled != disabled)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(boneReference.Bone);
#endif
                    boneReference.Bone.tag = disabled ? "EditorOnly" : "Untagged";
                }
            }
        }

        public void PrintDebug()
        {
            foreach (var boneReference in BoneReferences.OrderBy(p => p.BonePath))
            {
                var d = DisabledBones.Contains(boneReference.Bone);
                var f = ForceEnabledBones.Contains(boneReference.Bone);
                Debug.Log($"[{(UnusedBones.Contains(boneReference.Bone) ? "OFF" : "ON")}] [{boneReference.Bone.name}] [{boneReference.BonePath}] D={d} F={f} || {string.Join(",", boneReference.References.Select(t => t.name))}");
            }
        }
    }
}
