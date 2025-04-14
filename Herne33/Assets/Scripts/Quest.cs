using UnityEngine;
using Unity.VisualScripting;

public enum PhotoSubject
{
    Jellyfish, Deer
}

namespace GDIM33Demo
{
    [CreateAssetMenu(fileName = "Quest", menuName = "ScriptableObjects/Quest", order = 1)]
    public class Quest : ScriptableObject
    {
        //---------------------------------------------------------------------
        // Variables
        //---------------------------------------------------------------------
        [Tooltip("The unique name of the quest.")]
        public string Name;

        [Tooltip("A longer description of the quest.")]
        public string Description;

        [Tooltip("The required subject.")]
        public PhotoSubject Subject;

        [Tooltip("The dialogue the NPC plays when you turn in the quest.")]
        public DialogueNode QuestCompletionDialogue;

        //---------------------------------------------------------------------
        // Properties
        // this is dumb that i built it like this. i should have just made them
        //  public vars
        // but i don't want the users to edit them in the SO asset...
        // but at this point i also don't want to refactor bc i don't know how
        //  to find where in the Graphs I used the getter methods...
        //---------------------------------------------------------------------
        private bool _successfulPhotoTaken;
        public bool SuccessfulPhotoTaken => _successfulPhotoTaken;

        private bool _completed;
        public bool Completed => _completed;

        private GameObject _questGiver;
        public GameObject QuestGiver => _questGiver;

        //---------------------------------------------------------------------
        // Methods
        //---------------------------------------------------------------------
        private void OnEnable ()
        {
            // clear runtime only variables of our editor sins
            _completed = false;
            _successfulPhotoTaken = false;
            _questGiver = null;
        }

        //---------------------------------------------------------------------
        public void SetQuestGiverObject (GameObject questGiver)
        {
            _questGiver = questGiver;
        }

        //---------------------------------------------------------------------
        public void MarkSuccessfulPhotoTaken ()
        {
            _successfulPhotoTaken = true;
        }

        //---------------------------------------------------------------------
        public void CompleteQuest ()
        {
            _completed = true;
        }
    }
}