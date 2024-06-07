using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplicantChatBubble : ReplicantDriver
{
    //Reset and place UI elements (Priority)
    //Set text empty (Priority)
    //Reference by Tree (Priority)

    public TMP_Text MainName;
    public TMP_Text Chat;
    public TMP_Text Time;
    public RawImage Avatar;


    public TreeNode<Message> MessageRef;
    

    internal void SetMessageRef (TreeNode<Message> newmessageref)
    {
        MessageRef = newmessageref;
    }

}
