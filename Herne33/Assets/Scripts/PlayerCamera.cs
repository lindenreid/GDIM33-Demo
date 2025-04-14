using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Unity.VisualScripting;

namespace GDIM33Demo
{
    [Inspectable]
    public class PlayerCamera : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Variables
        //---------------------------------------------------------------------
        [SerializeField] private float _minFocalDistance = 0.10f;
        [SerializeField] private float _maxFocalDistance = 100.0f;
        // ideally, focal range would be calculated based on camera aperture
        // and focal length... but that's a TODO for once I actually write
        // a custom PP effect for the depth of field
        [SerializeField] private float _focalRange = 2.0f;
        [SerializeField] private Volume _depthOfFieldVolume;
        [SerializeField] private int _numFilm;
        [SerializeField] private Transform _cameraPosition;
        public Texture2D _recentPhoto;

        private Texture2D[] _activePhotos;
        private int _currentPhotoIndex;
        private bool _waitingForPhoto;
        private Quest _activeQuest;
        private DepthOfField _depthOfFieldSettings;

        //---------------------------------------------------------------------
        // Methods
        //---------------------------------------------------------------------
        private void Start ()
        {
            SetupFilm();
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

            _depthOfFieldVolume.profile.TryGet<DepthOfField>(out _depthOfFieldSettings);
            Assert.IsNotNull(_depthOfFieldSettings);
            var focalDistance = _depthOfFieldSettings.focusDistance;
            focalDistance.value = Mathf.SmoothStep(
                _minFocalDistance,
                _maxFocalDistance,
                0.5f
            );
        }

        //---------------------------------------------------------------------
        public void SetFocalDistance (float linearDistance)
        {
            var focalDistance = _depthOfFieldSettings.focusDistance;
            focalDistance.value = Mathf.SmoothStep(
                _minFocalDistance,
                _maxFocalDistance,
                linearDistance
            );
        }

        //---------------------------------------------------------------------
        private void SetupFilm ()
        {
            int w = Screen.width;
            int h = Screen.height;
            _activePhotos = new Texture2D[_numFilm];
            for(int i = 0; i < _numFilm; i++)
            {
                Texture2D photo = new Texture2D(
                    w, h,
                    TextureFormat.RGB24,
                    0, false
                );
                _activePhotos[i] = photo;
            }
            _currentPhotoIndex = 0;
        }

        //---------------------------------------------------------------------
        public void TakePhoto (Quest quest)
        {
            _waitingForPhoto = true;
            _activeQuest = quest;
        }

        //---------------------------------------------------------------------
        private void OnEndCameraRendering(
            ScriptableRenderContext context,
            Camera camera
        ) {
            if(!_waitingForPhoto || camera != Camera.main) 
            {
                return;
            }
            _waitingForPhoto = false;

            if(_currentPhotoIndex >= _activePhotos.Length)
            {
                return;
            }            

            // get current active film
            Texture2D photo = _activePhotos[_currentPhotoIndex];
            
            // take screenshot
            Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height);
            photo.ReadPixels(regionToReadFrom, 0, 0, false);
            photo.Apply(); // send to GPU

            _currentPhotoIndex++;

            int returnValue = 0;

            // check if frame contains subject
            if(_activeQuest != null)
            {
                returnValue = 1; 
                
                // finds all of the Subjects in the Scene of the type we're looking for
                // this is hella inefficient, I know, it's just a prototype :)
                Subject[] allSubjects = FindObjectsByType<Subject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                List<Subject> potentialTargets = allSubjects.Where(s => s.PhotoSubjectType == _activeQuest.Subject).ToList();

                float focalDistance = _depthOfFieldSettings.focusDistance.value;

                // determine if any of the subjects are within the MAIN CAMERAS frustrum AND are in-focus
                // there's multiple ways to evaluate if an object is in-frame,
                // but we have to do it this way bc we don't want to include Scene window or other cameras!
                Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
                foreach(Subject target in potentialTargets)
                {
                    Bounds targetBounds = target.BoundsForPhoto;
                    if(GeometryUtility.TestPlanesAABB(cameraPlanes, targetBounds))
                    {
                        returnValue = 2;
                        float subjectDist = Vector3.Distance(targetBounds.center, _cameraPosition.position);
                        if(Mathf.Abs(subjectDist - focalDistance) <= _focalRange)
                        {
                            returnValue = 3;
                            _activeQuest.MarkSuccessfulPhotoTaken();
                            break;
                        }
                    }
                }
            }

            // tell UI
            _recentPhoto = photo;
            // returnValue => 0 = no subject; 1 = subject not in frame; 2 = subject out of focus; 3 = successful
            EventBus.Trigger(EventNames.PhotoReadyEvent, returnValue);

            // clear current quest
            _activeQuest = null;
        }

        //---------------------------------------------------------------------
        private void OnDestroy ()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }
}