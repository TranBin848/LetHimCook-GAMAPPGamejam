using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "ScriptableObjects/NPCDialogue")]
public class ChefDialogue : ScriptableObject
{
    public string chefName;
    public Sprite ncpPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 2f; // Thời gian tự động chuyển sang dòng tiếp theo
    public float typingSpeed = 0.05f; // Thời gian gõ mỗi ký tự
    public float voicePitch = 1f;
}
