using UnityEngine;
using System.Text; // Required for StringBuilder

public class ModelInspector : MonoBehaviour
{ // Debugging script to inspect blendshapes and bones in a model
    public bool includeHierarchyTraversalForBones = true; // Option to enable/disable hierarchy traversal

    public bool includeBlendshapeInspection = false; // Option to enable/disable blendshape inspection
    void Start()
    {
        StringBuilder output = new StringBuilder();
        output.AppendLine("--- Model Inspection Results ---");
        output.AppendLine($"Model Name: {gameObject.name}");
        output.AppendLine("--------------------------------");

        InspectBlendshapes(output);
        output.AppendLine("--------------------------------");
        InspectBones(output);
        output.AppendLine("--------------------------------");

        if (includeBlendshapeInspection)
        {
            Debug.Log(output.ToString());
        }
    }

    void InspectBlendshapes(StringBuilder output)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>(); // Use GetComponentsInChildren to find any SkinnedMeshRenderer on children

        if (skinnedMeshRenderer != null)
        {
            Mesh mesh = skinnedMeshRenderer.sharedMesh;

            if (mesh != null)
            {
                int blendShapeCount = mesh.blendShapeCount;
                output.AppendLine($"Found SkinnedMeshRenderer: {skinnedMeshRenderer.gameObject.name}");
                output.AppendLine($"Number of Blendshapes: {blendShapeCount}");

                if (blendShapeCount > 0)
                {
                    for (int i = 0; i < blendShapeCount; i++)
                    {
                        string blendShapeName = mesh.GetBlendShapeName(i);
                        output.AppendLine($"  [{i}] {blendShapeName}");
                    }
                }
                else
                {
                    output.AppendLine("  No blendshapes found on this mesh.");
                }
            }
            else
            {
                output.AppendLine("  No Mesh found on SkinnedMeshRenderer.");
            }
        }
        else
        {
            output.AppendLine("No SkinnedMeshRenderer found on this GameObject or its children.");
        }
    }

    void InspectBones(StringBuilder output)
    {
        output.AppendLine("Bones (Rig) Information:");

        // Method 1: Using the Animator component (best for humanoid/generic rigs)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            output.AppendLine($"  Animator found on: {animator.gameObject.name}");
            output.AppendLine($"  Animator Avatar valid: {animator.avatar != null}");
            if (animator.avatar != null && animator.avatar.isHuman)
            {
                output.AppendLine("  Humanoid rig detected. Common bones:");
                // You can iterate through HumanBodyBones enum to get many standard bones
                foreach (HumanBodyBones boneType in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (boneType != HumanBodyBones.LastBone) // Exclude the last enum entry
                    {
                        Transform boneTransform = animator.GetBoneTransform(boneType);
                        if (boneTransform != null)
                        {
                            output.AppendLine($"    - {boneType}: {boneTransform.name} (Path: {GetGameObjectPath(boneTransform)})");
                        }
                    }
                }
            }
            else if (animator.avatar != null) // && animator.avatar.isGeneric)
            {
                output.AppendLine("  Generic rig detected.");
                // For generic rigs, you'd typically have to know the bone names or traverse
                // the hierarchy from the root bone.
                output.AppendLine("  To see generic rig bones, you might need to inspect the Avatar in the Rig tab.");
            }
        }
        else
        {
            output.AppendLine("  No Animator component found on this GameObject.");
        }

        // Method 2: Traversing the hierarchy to find all child transforms (useful for generic rigs or non-animated models)
        if (includeHierarchyTraversalForBones)
        {
            output.AppendLine("\n  --- Hierarchy Traversal for all Child Transforms ---");
            output.AppendLine("  (These are all GameObjects that could potentially be bones or other mesh parts)");
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                // This will list ALL child transforms. You'll need to visually inspect or use naming conventions
                // to distinguish actual bones from other helper GameObjects or mesh parts.
                output.AppendLine($"    - {child.name} (Path: {GetGameObjectPath(child)})");
            }
        }
    }

    // Helper to get the full path of a GameObject
    string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}