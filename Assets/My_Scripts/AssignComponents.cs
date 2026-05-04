using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

#if UNITY_EDITOR
using UnityEditor;
# endif

public class AssignComponents : MonoBehaviour
{
    [SerializeField] GameObject[] gesturesLeft;
    [SerializeField] GameObject[] gesturesRight;
    [SerializeField] GameObject[] gesturesBoth;
    [SerializeField] Hand leftHand;
    [SerializeField] Hand rightHand;
    [SerializeField] FingerFeatureStateProvider leftFingerFeatureStateProvider;
    [SerializeField] FingerFeatureStateProvider rightFingerFeatureStateProvider;
    [SerializeField] TransformFeatureStateProvider leftTransformFeatureStateProvider;
    [SerializeField] TransformFeatureStateProvider rightTransformFeatureStateProvider;

    [ContextMenu("Set References(custom)")]
    void AssignAll()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (leftHand == null || rightHand == null)
            {
                Debug.LogError("Left or Right Hand is not assigned. Please assign them in the Inspector.");
                return;
            }
            if (leftFingerFeatureStateProvider == null || rightFingerFeatureStateProvider == null)
            {
                Debug.LogError("Left or Right Finger Feature State Provider is not assigned. Please assign them in the Inspector.");
                return;
            }
            if (leftTransformFeatureStateProvider == null || rightTransformFeatureStateProvider == null)
            {
                Debug.LogError("Left or Right Transform Feature State Provider is not assigned. Please assign them in the Inspector.");
                return;
            }
            Debug.Log("Set References in Editor mode..");

            if (gesturesBoth != null)
            {
                try
                {
                    foreach (GameObject gesture in gesturesBoth)
                    {
                        if (gesture == null) throw new System.NullReferenceException("Gesture in gesturesBoth is null");
                        // assign both hands component to each gesture
                        HandRef[] ges_hand = gesture.GetComponents<HandRef>();
                        if (ges_hand.Length < 2)
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have 2 HandRef components");
                            continue;
                        }

                        TransformRecognizerActiveState[] ges_trans = gesture.GetComponents<TransformRecognizerActiveState>();
                        if (ges_trans.Length < 2)
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have 2 TransformRecognizerActiveState components");
                            continue;
                        }

                        // assign and make it dirty
                        ges_hand[0].InjectHand(leftHand);
                        EditorUtility.SetDirty(ges_hand[0]);
                        ges_hand[1].InjectHand(rightHand);
                        EditorUtility.SetDirty(ges_hand[1]);

                        ges_trans[0].InjectTransformFeatureStateProvider(leftTransformFeatureStateProvider);
                        EditorUtility.SetDirty(ges_trans[0]);
                        ges_trans[1].InjectTransformFeatureStateProvider(rightTransformFeatureStateProvider);
                        EditorUtility.SetDirty(ges_trans[1]);

                        if(!gesture.activeSelf)
                        {
                            gesture.SetActive(true);
                        }
                        EditorUtility.SetDirty(gesture); // Mark the gesture as dirty
                        Debug.Log($"Both hands gesture configured: {gesture.name}");
                    }
                    Debug.Log("Gestures for both hands assigned");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in gesturesBoth: {e.Message}");
                }
            }
            if (gesturesLeft != null)
            {
                try
                {
                    foreach (GameObject gesture in gesturesLeft)
                    {
                        if (gesture == null) throw new System.NullReferenceException("Gesture in gesturesLeft is null");

                        // some of the gestures may not have HandRef, ShapeRecognizerActiveState or TransformRecognizerActiveState components
                        // LPose that is a child of the FramePoseTwoHanded, only has ShapeRecognizerActiveState, that's why it is first, so that it can be assigned
                        // while the other can be skipped if not present
                        if (!gesture.TryGetComponent<ShapeRecognizerActiveState>(out var shapeRecognizer))
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a ShapeRecognizerActiveState component");
                            continue;
                        }
                        shapeRecognizer.InjectFingerFeatureStateProvider(leftFingerFeatureStateProvider);
                        EditorUtility.SetDirty(shapeRecognizer);
                        // assign the left hand component to each gesture
                        if (!gesture.TryGetComponent<HandRef>(out var handRef))
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a HandRef component");
                            continue;
                        }
                        handRef.InjectHand(leftHand);
                        EditorUtility.SetDirty(handRef);

                        if (!gesture.TryGetComponent<TransformRecognizerActiveState>(out var transformRecognizer))
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a TransformRecognizerActiveState component");
                            continue;
                        }
                        transformRecognizer.InjectTransformFeatureStateProvider(leftTransformFeatureStateProvider);
                        EditorUtility.SetDirty(transformRecognizer);

                        if (!gesture.activeSelf)
                        {
                            gesture.SetActive(true);
                        }
                        EditorUtility.SetDirty(gesture); // Mark the gesture as dirty
                        Debug.Log($"Left hand gesture configured: {gesture.name}");
                    }
                    Debug.Log("Gestures for left hand assigned");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in gesturesLeft: {e.Message}");
                }
            }
            if (gesturesRight != null)
            {
                try
                {
                    foreach (GameObject gesture in gesturesRight)
                    {
                        if (gesture == null) throw new System.NullReferenceException("Gesture in gesturesRight is null");
                        // assign the right hand component to each gesture
                        HandRef handRef = gesture.GetComponent<HandRef>();
                        if (handRef == null)
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a HandRef component");
                            continue;
                        }
                        handRef.InjectHand(rightHand);
                        EditorUtility.SetDirty(handRef);

                        if (!gesture.TryGetComponent<ShapeRecognizerActiveState>(out var shapeRecognizer))
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a ShapeRecognizerActiveState component");
                            continue;
                        }
                        shapeRecognizer.InjectFingerFeatureStateProvider(rightFingerFeatureStateProvider);
                        EditorUtility.SetDirty(shapeRecognizer);
                        if (!gesture.TryGetComponent<TransformRecognizerActiveState>(out var transformRecognizer))
                        {
                            Debug.LogWarning($"Gesture {gesture.name} doesn't have a TransformRecognizerActiveState component");
                            continue;
                        }
                        transformRecognizer.InjectTransformFeatureStateProvider(rightTransformFeatureStateProvider);
                        EditorUtility.SetDirty(transformRecognizer);

                        gesture.SetActive(true);
                        Debug.Log($"Right hand gesture configured: {gesture.name}");
                    }
                    Debug.Log("Gestures for right hand assigned");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in gesturesRight: {e.Message}");
                }
            }

        }
        // Save the current scene
        bool change = UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
        if (change && saved)
        {
            Debug.Log("Scene saved successfully.");
        }
        else
        {
            Debug.LogWarning("something went wrong while saving the scene.");
        }
#endif
    }
}
