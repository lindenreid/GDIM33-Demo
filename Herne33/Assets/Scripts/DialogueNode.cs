using System.Collections.Generic;
using UnityEngine;

namespace GDIM33Demo
{
    [CreateAssetMenu(fileName = "DialogueLine", menuName = "ScriptableObjects/DialogueLine", order = 1)]
    public class DialogueNode : ScriptableObject
    {
        //---------------------------------------------------------------------
        // Variables
        //---------------------------------------------------------------------
        [Tooltip("The lines of dialogue the NPC says in this node.")]
        public List<string> Lines;

        [Tooltip("[Optional] The dialogue options for the player's response, and what node each response leads to.")]
        public List<PlayerReply> ReplyOptions;

        [Tooltip("[Optional] The associated quest ASSIGNED after this line completes.")]
        public Quest QuestGiven;

        [Tooltip("[Optional] The associated quest FINISHED after this line completes.")]
        public Quest QuestFinished;

        [Tooltip("[Optional] The audio that should play at the beginning of this line.")]
        public AudioClip Audio;
    }
}