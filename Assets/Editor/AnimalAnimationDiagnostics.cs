#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimalAnimationDiagnostics
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [MenuItem("Tools/Animals/Diagnose Animation Setup")]
    public static void Diagnose()
    {
        AnimalLibrary library = Object.FindFirstObjectByType<AnimalLibrary>();

        if (library == null)
        {
            Debug.LogError("No AnimalLibrary was found in the open scene.");
            return;
        }

        AnimalDefinition[] animals = library.GetAnimals();
        StringBuilder report = new StringBuilder();
        report.AppendLine("========== ANIMAL ANIMATION DIAGNOSTICS ==========");

        for (int index = 0; index < animals.Length; index++)
        {
            AnimalDefinition definition = animals[index];
            string animalName = !string.IsNullOrWhiteSpace(definition.displayName)
                ? definition.displayName
                : definition.modelPrefab != null
                    ? definition.modelPrefab.name
                    : $"Animal {index}";

            report.AppendLine();
            report.AppendLine($"[{index}] {animalName}");

            if (definition.modelPrefab == null)
            {
                report.AppendLine("ERROR: Model prefab is missing.");
                continue;
            }

            if (definition.animatorController == null)
            {
                report.AppendLine("ERROR: Animator controller is missing.");
                continue;
            }

            Animator[] animators = definition.modelPrefab.GetComponentsInChildren<Animator>(true);
            report.AppendLine($"Animator count in prefab: {animators.Length}");

            if (animators.Length == 0)
            {
                report.AppendLine("ERROR: No Animator exists in this prefab.");
                continue;
            }

            foreach (Animator animator in animators)
            {
                report.AppendLine(
                    $"Animator: {GetPath(definition.modelPrefab.transform, animator.transform)}, " +
                    $"enabled={animator.enabled}, avatar={animator.avatar?.name ?? "None"}, " +
                    $"avatarValid={animator.avatar?.isValid.ToString() ?? "N/A"}, " +
                    $"skinnedMeshes={animator.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length}");
            }

            Animator selected = animators[0];
            report.AppendLine(
                $"CURRENT CODE SELECTS FIRST ANIMATOR: " +
                GetPath(definition.modelPrefab.transform, selected.transform));

            AnimatorController baseController = GetBaseController(definition.animatorController);
            bool hasSpeed = false;

            if (baseController != null)
            {
                foreach (AnimatorControllerParameter parameter in baseController.parameters)
                {
                    if (parameter.nameHash == SpeedHash &&
                        parameter.type == AnimatorControllerParameterType.Float)
                    {
                        hasSpeed = true;
                        break;
                    }
                }
            }

            report.AppendLine($"Controller: {definition.animatorController.name}");
            report.AppendLine($"Float Speed parameter: {hasSpeed}");

            AnimationClip[] clips = definition.animatorController.animationClips;
            report.AppendLine($"Animation clip count: {clips.Length}");

            HashSet<string> transformPaths = BuildTransformPaths(selected.transform);

            foreach (AnimationClip clip in clips)
            {
                EditorCurveBinding[] floatBindings = AnimationUtility.GetCurveBindings(clip);
                EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

                int total = floatBindings.Length + objectBindings.Length;
                int matched = 0;

                foreach (EditorCurveBinding binding in floatBindings)
                {
                    if (transformPaths.Contains(binding.path)) matched++;
                }

                foreach (EditorCurveBinding binding in objectBindings)
                {
                    if (transformPaths.Contains(binding.path)) matched++;
                }

                float ratio = total > 0 ? (float)matched / total : 0f;

                report.AppendLine(
                    $"Clip '{clip.name}': legacy={clip.legacy}, empty={clip.empty}, " +
                    $"bindings={total}, matchedToSelectedAnimator={matched} ({ratio:P0})");

                if (clip.legacy)
                {
                    report.AppendLine("  ERROR: Legacy clip cannot be driven normally by Animator Controller/Mecanim.");
                }

                if (total > 0 && ratio < 0.5f)
                {
                    report.AppendLine(
                        "  ERROR: Most animation curve paths do not exist under the Animator selected by the current code. " +
                        "This usually means the wrong Animator/root was selected or the clip belongs to a different skeleton.");
                }
            }
        }

        report.AppendLine("==================================================");
        Debug.Log(report.ToString(), library);
    }

    private static AnimatorController GetBaseController(RuntimeAnimatorController controller)
    {
        if (controller is AnimatorController animatorController)
        {
            return animatorController;
        }

        if (controller is AnimatorOverrideController overrideController)
        {
            return GetBaseController(overrideController.runtimeAnimatorController);
        }

        return null;
    }

    private static HashSet<string> BuildTransformPaths(Transform root)
    {
        HashSet<string> paths = new HashSet<string> { string.Empty };
        AddChildren(root, root, paths);
        return paths;
    }

    private static void AddChildren(Transform root, Transform current, HashSet<string> paths)
    {
        foreach (Transform child in current)
        {
            paths.Add(AnimationUtility.CalculateTransformPath(child, root));
            AddChildren(root, child, paths);
        }
    }

    private static string GetPath(Transform root, Transform target)
    {
        return target == root
            ? root.name
            : root.name + "/" + AnimationUtility.CalculateTransformPath(target, root);
    }
}
#endif
