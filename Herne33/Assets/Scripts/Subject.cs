using UnityEngine;

namespace GDIM33Demo
{
    public class Subject : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        [SerializeField] private PhotoSubject _photoSubjectType;
        public PhotoSubject PhotoSubjectType => _photoSubjectType;

        [SerializeField] private Collider _boundsForPhoto;
        public Bounds BoundsForPhoto => _boundsForPhoto.bounds;
    }
}