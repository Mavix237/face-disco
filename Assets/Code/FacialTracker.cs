// AR Kit Blend Shapes
// https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/mouthlowerdownleft

using UnityEngine;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;

public class FacialTracker : MonoBehaviour
{
    ARKitFaceSubsystem faceSubsystem;
    ARFace face;
    UIManager uiManager;
    BeatSource beatSource;
    void Start()
    {
        face = GetComponent<ARFace>();
        ARFaceManager faceManager = FindAnyObjectByType<ARFaceManager>();
        if (faceManager != null)
        {
            faceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
        //update the text
        uiManager = FindAnyObjectByType<UIManager>();
        beatSource = FindAnyObjectByType<BeatSource>();
    }

    void Update()
    {
        if (faceSubsystem != null)
        {
            using (var blendShapes = faceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp))
            {
                //smile tracker
                float smileMuch = 0;
                foreach (var featureCoefficient in blendShapes)
                {
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthSmileRight)
                    {smileMuch += featureCoefficient.coefficient;}
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthSmileLeft)
                    {smileMuch += featureCoefficient.coefficient;}
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthFrownRight)
                    {smileMuch -= featureCoefficient.coefficient;}
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthFrownLeft)
                    {smileMuch -= featureCoefficient.coefficient;}
                }

                //eye blink tracker
                float eyeBlink = 0;
                bool eyesBlinking = false;
                foreach (var featureCoefficient in blendShapes)
                {
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.EyeBlinkRight)
                    {eyeBlink += featureCoefficient.coefficient;}
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.EyeBlinkLeft)
                    {eyeBlink += featureCoefficient.coefficient;}
                    
                    if(eyeBlink > 0.4f){
                        eyesBlinking = true;
                    } else {
                        eyesBlinking = false;
                    }
                }

                //mouth open tracker
                float jawOpen = 0;
                bool isJawOpen = false;
                foreach (var featureCoefficient in blendShapes)
                {
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.JawOpen)
                    {
                        jawOpen += featureCoefficient.coefficient;
                    }
                    if (jawOpen > 0.3f)
                    {
                        isJawOpen = true;
                    }
                    else
                    {
                        isJawOpen = false;
                    }
                }


                float browRaise = 0;
                bool isBrowRaise = false;
                foreach (var featureCoefficient in blendShapes)
                {
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowInnerUp)
                    {
                        browRaise += featureCoefficient.coefficient;
                    }
                    if (browRaise > 0.3f)
                    {
                        isBrowRaise = true;
                    }
                    else
                    {
                        isBrowRaise = false;
                    }
                }

                //play beats - call methods in BeatSource.cs
                if (beatSource != null)
                {
                    beatSource.TriggerSmileAudio(smileMuch);
                    beatSource.TriggerEyeBlinkAudio(eyesBlinking);
                    beatSource.TriggerJawOpenAudio(isJawOpen);
                    beatSource.TriggerBrowRaiseAudio(isBrowRaise);
                }

                uiManager.UpdateText("Smile: " + smileMuch + "\nEyes Blink:" + eyesBlinking + "\nMouth Open:" + isJawOpen);

            }
        }
    }
}