/*
 *  This is a template verison of the VIDEUIManager1.cs script. Check that script out and the "Player Interaction" demo for more reference.
 *  This one doesn't include an item popup as that demo was mostly hard coded.
 *  Doesn't include reference to a player script or gameobject. How you handle that is up to you.
 *  Doesn't save dialogue and VA state.
 *  Player choices are not instantiated. You need to set the references manually.
    
 *  You are NOT limited to what this script can do. This script is only for convenience. You are completely free to write your own manager or build from this one.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using VIDE_Data; //<--- Import to use VD class

public class Test_UIManager : MonoBehaviour
{
    #region VARS

    //These are the references to UI components and containers in the scene
    [Header("References")]
    public GameObject dialogueContainer;
    public GameObject NPC_Container;
    public GameObject playerContainer;

    public Text NPC_Text;
    public Text NPC_label;
    public Image NPCSprite;
    public Image playerSprite;
    public Text playerLabel;

    public List<Button> maxPlayerChoices = new List<Button>();

    [Tooltip("Attach an Audio Source and reference it if you want to play audios")]
    public AudioSource audioSource;

    [Header("Options")]
    public KeyCode interactionKey;
    public bool NPC_animateText;
    public bool player_animateText;
    public float NPC_secsPerLetter;
    public float player_secsPerLetter;
    public float choiceInterval;
    [Tooltip("Tick this if using Navigation. Will prevent mixed input.")]
    public bool useNavigation;

    // Typing SFX
    [Header("SFX Settings")]
    public AudioClip typingSFX;     // 한 글자 출력 시 재생할 효과음
    [Range(1f, 2f)] public float sfxPitchMax = 1.05f;
    [Range(0.5f, 1f)] public float sfxPitchMin = 0.95f;
    [Min(1)] public int sfxEveryNChars = 1; // N글자마다 1회 재생 (중첩 완화)

    bool dialoguePaused = false; //Custom variable to prevent the manager from calling VD.Next
    bool animatingText = false; //Will help us know when text is currently being animated
    int availableChoices = 0;

    [Header("Name Fallbacks")]
    public string defaultPlayerName = "다람쥐";
    public string defaultNPCName = "NPC";

    // ★ 추가: Sprite Fallback
    [Header("Sprite Fallbacks")]
    public Sprite defaultNPCSprite;


    IEnumerator TextAnimator;
    int _typedCharCount = 0; // 타이핑 SFX 카운터

    #endregion

    #region MAIN

    void Awake()
    {
        // VD.LoadDialogues(); //Load all dialogues to memory so that we dont spend time doing so later
        //An alternative to this can be preloading dialogues from the VIDE_Assign component!
    }

    //Call this to begin the dialogue and advance through it
    public void Interact(VIDE_Assign dialogue)
    {
        var doNotInteract = PreConditions(dialogue);
        if (doNotInteract) return;

        if (!VD.isActive)
        {
            Begin(dialogue);
        }
        else
        {
            CallNext();
        }
    }

    //This begins the conversation. 
    void Begin(VIDE_Assign dialogue)
    {
        //Let's reset the NPC text variables
        NPC_Text.text = "";
        NPC_label.text = "";
        playerLabel.text = "";

        //Subscribe to events
        VD.OnActionNode += ActionHandler;
        VD.OnNodeChange += UpdateUI;
        VD.OnEnd += EndDialogue;

        VD.BeginDialogue(dialogue); //Begins dialogue, will call the first OnNodeChange

        dialogueContainer.SetActive(true); //Let's make our dialogue container visible
    }

    //Calls next node in the dialogue
    public void CallNext()
    {
        //Let's not go forward if text is currently being animated, but let's speed it up.
        if (animatingText) { CutTextAnim(); return; }

        if (!dialoguePaused) //Only if
        {
            VD.Next(); //We call the next node and populate nodeData with new data. Will fire OnNodeChange.
        }
        else
        {
            //Stuff we can do instead if dialogue is paused
        }
    }

    //If not using local input, then the UI buttons are going to call this method when you tap/click them!
    //They will send along the choice index
    public void SelectChoice(int choice)
    {
        VD.nodeData.commentIndex = choice;

        if (Input.GetMouseButtonUp(0))
        {
            Interact(VD.assigned);
        }
    }

    //Input related stuff (scroll through player choices and update highlight)
    void Update()
    {
        var data = VD.nodeData;

        if (VD.isActive) //If there is a dialogue active
        {
            if (!data.pausedAction && !animatingText && data.isPlayer && !useNavigation)
            {
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (data.commentIndex < availableChoices - 1)
                        data.commentIndex++;
                }
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (data.commentIndex > 0)
                        data.commentIndex--;
                }
                //Color the Player options. Blue for the selected one
                for (int i = 0; i < maxPlayerChoices.Count; i++)
                {
                    maxPlayerChoices[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.gray;
                    if (i == data.commentIndex) maxPlayerChoices[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                }
            }

            //Detect interact key
            if (Input.GetKeyDown(interactionKey))
            {
                Interact(VD.assigned);
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (animatingText)
                {
                    Interact(VD.assigned);
                }
                else if (!data.isPlayer)
                {
                    Interact(VD.assigned);
                }
            }
        }
        //Note you could also use Unity's Navi system, in which case you would tick the useNavigation flag.
    }

    //When we call VD.Next, nodeData will change. When it changes, OnNodeChange event will fire
    //We subscribed our UpdateUI method to the event in the Begin method
    //Here's where we update our UI
    void UpdateUI(VD.NodeData data)
    {
        //Reset some variables
        NPC_Text.text = "";
        foreach (Button b in maxPlayerChoices)
        {
            b.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            b.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
        NPC_Container.SetActive(false);
        playerContainer.SetActive(false);
        playerSprite.sprite = null;
        NPCSprite.sprite = null;

        //Look for dynamic text change in extraData
        PostConditions(data);

        if (data.isPlayer)
        {
            if (data.sprite != null)
                playerSprite.sprite = data.sprite;
            else if (VD.assigned.defaultPlayerSprite != null)
                playerSprite.sprite = VD.assigned.defaultPlayerSprite;

            SetChoices(data.comments);

            if (data.tag.Length > 0)
                playerLabel.text = data.tag;
            else
                playerLabel.text = defaultPlayerName;

            playerContainer.SetActive(true);
        }
        else
        {
            if (data.sprite != null)
            {
                if (data.extraVars.ContainsKey("sprite"))
                {
                    if (data.commentIndex == (int)data.extraVars["sprite"])
                        NPCSprite.sprite = data.sprite;
                    else
                        NPCSprite.sprite = defaultNPCSprite;   // ★ 수정
                }
                else
                {
                    NPCSprite.sprite = data.sprite;
                }
            }
            else if (defaultNPCSprite != null)   // ★ 수정
                NPCSprite.sprite = defaultNPCSprite;

            if (NPC_animateText)
            {
                TextAnimator = AnimateNPCText(data.comments[data.commentIndex]);
                StartCoroutine(TextAnimator);
            }
            else
            {
                NPC_Text.text = data.comments[data.commentIndex];
            }

            if (data.audios[data.commentIndex] != null)
            {
                audioSource.clip = data.audios[data.commentIndex];
                audioSource.Play();
            }

            if (data.tag.Length > 0)
                NPC_label.text = data.tag;
            else if (!string.IsNullOrEmpty(VD.assigned.alias))
                NPC_label.text = VD.assigned.alias;
            else
                NPC_label.text = defaultNPCName;

            NPC_Container.SetActive(true);
        }
    }

    //This uses the returned string[] from nodeData.comments to create the UIs for each comment
    //It first cleans, then it instantiates new choices
    public void SetChoices(string[] choices)
    {
        availableChoices = 0;

        foreach (UnityEngine.UI.Button choice in maxPlayerChoices)
            choice.gameObject.SetActive(false);

        if (player_animateText)
        {
            TextAnimator = AnimatePlayerText(choices);
            StartCoroutine(TextAnimator);
        }
        else
        {
            for (int i = 0; i < choices.Length; i++)
            {
                maxPlayerChoices[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i];
                maxPlayerChoices[i].gameObject.SetActive(true);
                availableChoices++;
            }
            if (useNavigation)
                maxPlayerChoices[0].Select();
        }
    }

    //Unsuscribe from everything, disable UI, and end dialogue
    public void EndDialogue(VD.NodeData data)
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);
        VD.EndDialogue();
    }

    //To prevent errors
    void OnDisable()
    {
        EndDialogue(null);
    }

    #endregion

    #region DIALOGUE CONDITIONS 

    bool PreConditions(VIDE_Assign assigned)
    {
        var data = VD.nodeData;
        if (VD.isActive)
        {
            if (!data.isPlayer)
            {

            }
            else
            {

            }
        }
        else
        {

        }
        return false;
    }

    void PostConditions(VD.NodeData data)
    {
        if (data.pausedAction) return;

        ReplaceWord(data);

        if (!data.isPlayer) //For NPC nodes
        {
            if (data.extraData[data.commentIndex].Contains("fs"))
            {
                int fSize = 14;
                string[] fontSize = data.extraData[data.commentIndex].Split(","[0]);
                int.TryParse(fontSize[1], out fSize);
                NPC_Text.fontSize = fSize;
            }
            else
            {
                NPC_Text.fontSize = 14;
            }
        }
        else
        {

        }
    }

    void ReplaceWord(VD.NodeData data)
    {
        if (data.comments[data.commentIndex].Contains("[NAME]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[NAME]", VD.assigned.gameObject.name);

        if (data.comments[data.commentIndex].Contains("[WEAPON]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[WEAPON]", "sword");
    }

    #endregion

    #region EVENTS AND HANDLERS

    void OnLoadedAction()
    {
        VD.OnLoaded -= OnLoadedAction;
    }

    void ActionHandler(int actionNodeID)
    {
        //Debug.Log("ACTION TRIGGERED: " + actionNodeID.ToString());
    }

    // 공통 타이핑 SFX
    void PlayTypingSFX()
    {
        if (audioSource == null || typingSFX == null) return;

        _typedCharCount++;
        if (_typedCharCount % Mathf.Max(1, sfxEveryNChars) != 0) return;

        audioSource.pitch = Random.Range(sfxPitchMin, sfxPitchMax);
        audioSource.PlayOneShot(typingSFX);
    }

    IEnumerator AnimatePlayerText(string[] choices)
    {
        animatingText = true;
        _typedCharCount = 0;

        int usableCount = Mathf.Min(choices.Length, maxPlayerChoices.Count);
        for (int c = 0; c < usableCount; c++)
        {
            var btn = maxPlayerChoices[c];
            var tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            btn.gameObject.SetActive(true);

            string line = choices[c];
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = false;
            tmp.text = "";

            float wait = Mathf.Max(0f, player_secsPerLetter);
            for (int i = 0; i < line.Length; i++)
            {
                tmp.text = line.Substring(0, i + 1);

                // 한 글자마다 SFX
                PlayTypingSFX();

                yield return new WaitForSeconds(wait);
            }

            availableChoices++;
            tmp.text = line;
            yield return new WaitForSeconds(choiceInterval);
        }

        if (useNavigation && maxPlayerChoices.Count > 0 && maxPlayerChoices[0] != null)
            maxPlayerChoices[0].Select();

        animatingText = false;

        if (audioSource) audioSource.pitch = 1f; // 원복
    }

    IEnumerator AnimateNPCText(string text)
    {
        animatingText = true;
        _typedCharCount = 0;

        if (NPC_Text == null)
        {
            Debug.LogError("[Dialogue] NPC_Text is null.");
            animatingText = false;
            yield break;
        }

        NPC_Text.supportRichText = true;
        NPC_Text.text = "";

        float wait = Mathf.Max(0f, NPC_secsPerLetter);

        // 높이 비교/개행 보정 로직 제거 → 글자만 누적
        for (int i = 0; i < text.Length; i++)
        {
            NPC_Text.text = text.Substring(0, i + 1);

            // 한 글자마다 SFX
            PlayTypingSFX();

            yield return new WaitForSeconds(wait);
        }

        NPC_Text.text = text;
        animatingText = false;

        if (audioSource) audioSource.pitch = 1f; // 원복
    }

    void CutTextAnim()
    {
        if (TextAnimator != null) StopCoroutine(TextAnimator);

        if (VD.nodeData.isPlayer)
        {
            availableChoices = 0;
            int usableCount = Mathf.Min(VD.nodeData.comments.Length, maxPlayerChoices.Count);
            for (int i = 0; i < usableCount; i++)
            {
                maxPlayerChoices[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = VD.nodeData.comments[i];
                maxPlayerChoices[i].gameObject.SetActive(true);
                availableChoices++;
            }
            if (useNavigation && usableCount > 0)
                maxPlayerChoices[0].Select();
        }
        else
        {
            NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex];
        }
        animatingText = false;

        if (audioSource) audioSource.pitch = 1f; // 원복
    }

    #endregion

    //Utility note: If you're on MonoDevelop. Go to Tools > Options > General and enable code folding.
    //That way you can exapnd and collapse the regions and methods

}
