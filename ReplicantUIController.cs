using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnity;
using TMPro;
using UnityEngine.UI;
using QFSW.MOP2;
using System;
using Lasm.Bolt.UniversalSaver;
using System.IO;
using System.Threading.Tasks;

public class ReplicantUIController : ReplicantDriver
{

    //Summarize and Engels should have a 2k limit, we should let this known to the AI




    public ReplicantDriver RDriver;
    public GameObject ChatContainer;
    public TMP_InputField UserText;
    public Button EnterButton;
    public Image ButtonColor;
    public Sprite EnabledImg;
    public Sprite DisabledImg;
    public ObjectPool UserChat;
    public ObjectPool AIChat;
    public Button TTSButton;

    public bool IsAISpeaking;
    GameObject currentbubble;

    public bool IsRVCEnabled;

    private bool IsSavingHistory;
    public GameObject SavingHistory;

    private string ReferenceFile;

    string context;

    bool IsNewSession = true;


    private void SetChangingHistory()
    {
        IsSavingHistory = !IsSavingHistory;

        if (IsSavingHistory)
        {
            SavingHistory.SetActive (true);
        }

        else
        {
            SavingHistory.SetActive (false); 
        }
    }



    private void OnEnable()
    {

        InitializeSumSys();
        if (IsNewSession = true)
        {
            if (ChatContainer.transform.childCount <= 3)
            {
                var sessUUID = RDriver.StartNewSession();

                //Change colors red and blue to actual hexes

                EnterButton.onClick.AddListener(UserInputButtonClicked);
                TTSButton.onClick.AddListener(ChangeTTSStatus);

                UserText.interactable = false;
                EnterButton.interactable = false;

                currentbubble = AIChat.GetObject();
                currentbubble.transform.SetParent(ChatContainer.transform);

                ReferenceFile = RDriver.SaveLocation + @"/DataBunker" + RDriver.mainAI.AITruename + "GeneralSessions" + "/" + sessUUID;


                //Let's check this to look for main summarizers, if this is a new session and if context exists already


                if (IsNewSession)
                {

                    _ = Driver.Chat("Say Hello and introduce yourself to the user. If you do not know the user, you should give a generic introduction. However as you learn more about the user the " +
                        "introductions should become more customized.", HandleReply, IntroCompleted);
                }



                else
                {
                    //Get the context and use it
                }


            }
        }

        else
        {

        }

    }

    private void ChangeTTSStatus()
    {
        IsRVCEnabled = !IsRVCEnabled;
    }




    void LoadSession()
    {
        //create message

        IsAISpeaking = true;

        currentbubble = AIChat.GetObject();
        currentbubble.transform.SetParent(ChatContainer.transform);

        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();

        ButtonColor.color = Color.red;
        ButtonColor.sprite = DisabledImg;

        Debug.Log(ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.name);

        var TreeToRef = ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.GetComponent<ReplicantChatBubble>().MessageRef;

        Debug.Log(TreeToRef);

        var timenow = DateTime.Now.ToString();

        var currentusermessage = RDriver.AddUserMessage(UserText.text, "User", TreeToRef, timenow);


        var finalinput = RDriver.ConvertTreeToChatML(currentusermessage);

        string finalinputChatML = string.Join(" ", finalinput);


        // Update the MessageRef field directly
        chatBubble.SetMessageRef((currentusermessage));


        var CML = RDriver.ConvertChatToDicts(chatBubble.MessageRef);

        // Assuming you have dictionaries for AI, Humans, and Computer messages
        Dictionary<int, string> HumanMessages = CML[0];
        Dictionary<int, string> AIMessages = CML[1];
        Dictionary<int, string> ComputerMessages = CML[2];
        Dictionary<int, string> GrandmaMessages = CML[3];

        var sessid = RDriver.SessionUUID;

        RDriver.SaveSessionHistory(RDriver.mainAI, DateTime.Now, AIMessages, HumanMessages, ComputerMessages, GrandmaMessages, AIMessages.Count, sessid, true);



        _ = Driver.Chat("Continue The Conversation" + finalinputChatML, HandleReply, AIDoneSpeaking);

    }


    void IntroCompleted()
    {
        UserText.interactable = true;
        EnterButton.interactable = true;

        Debug.Log("Intro Finished!");
        Debug.Log(RDriver.CurrentMainSession.Data.ToString());

        var timenow = DateTime.Now.ToString();

        TreeNode<Message> currentmessage = RDriver.AddAssistantMessage(currentbubble.transform.GetChild(0).GetComponent<TMP_Text>().text, RDriver.mainAI.AIName, RDriver.CurrentMainSession, timenow);

        Debug.Log(currentmessage.ToString());

        // Get the ReplicantChatBubble component attached to currentbubble
        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();

        // Update the MessageRef field directly
        chatBubble.SetMessageRef((currentmessage));




    }



    void UserInputButtonClicked()
    {

        //We talk to the AI
        if (IsAISpeaking == false)
        {



            currentbubble = UserChat.GetObject();
            currentbubble.transform.SetParent(ChatContainer.transform);
            currentbubble.gameObject.GetComponent<ReplicantChatBubble>().Chat.text = UserText.text;

            var TreeHistory = ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 2).gameObject.GetComponent<ReplicantChatBubble>().MessageRef; //The bubble the user made

            Debug.Log(ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 2).gameObject.name);









            //create message

            IsAISpeaking = true;

            currentbubble = AIChat.GetObject();
            currentbubble.transform.SetParent(ChatContainer.transform);

            ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();

            ButtonColor.color = Color.red;
            ButtonColor.sprite = DisabledImg;

            Debug.Log(ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.name);
            
            var TreeToRef = ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.GetComponent<ReplicantChatBubble>().MessageRef;

            Debug.Log(TreeToRef);

            var timenow = DateTime.Now.ToString();

            var currentusermessage = RDriver.AddUserMessage(UserText.text, "User", TreeToRef, timenow);










            var GenSession = SaveLocation + @"/DataBunker" + mainAI + "GeneralSessions" + "/" + RDriver.SessionUUID + "Focus";


            var gensave = UniversalSave.Load(GenSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);

            gensave.Set("FocusedSession", RDriver.SelectConversationTree(chatBubble.MessageRef));

            UniversalSave.Save(GenSession, gensave);



            var finalinput = RDriver.ConvertTreeToChatML(currentusermessage);

            string finalinputChatML = string.Join(" ", finalinput);


            // Update the MessageRef field directly
            chatBubble.SetMessageRef((currentusermessage));


            var CML = RDriver.ConvertChatToDicts(chatBubble.MessageRef);

            // Assuming you have dictionaries for AI, Humans, and Computer messages
            Dictionary<int, string> HumanMessages = CML[0];
            Dictionary<int, string> AIMessages = CML[1];
            Dictionary<int, string> ComputerMessages = CML[2];
            Dictionary<int, string> GrandmaMessages = CML[3];

            var sessid = RDriver.SessionUUID;

            RDriver.SaveSessionHistory(RDriver.mainAI, DateTime.Now, AIMessages, HumanMessages, ComputerMessages, GrandmaMessages, AIMessages.Count, sessid, true);



            _ = Driver.Chat(finalinputChatML, HandleReply, AIDoneSpeaking);

        }

        else if (IsAISpeaking == true)
        {


            Driver.CancelRequests();
            ButtonColor.color = Color.white;
            ButtonColor.sprite = EnabledImg;
            IsAISpeaking = false;


            currentbubble = AIChat.GetObject();
            currentbubble.transform.SetParent(ChatContainer.transform);

            ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();


            Debug.Log(ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.name);

            var TreeToRef = ChatContainer.transform.GetChild(ChatContainer.transform.childCount - 1).gameObject.GetComponent<ReplicantChatBubble>().MessageRef;

            Debug.Log(TreeToRef);

            var timenow = DateTime.Now.ToString();

            var currentusermessage = RDriver.AddUserMessage(UserText.text, "User", TreeToRef, timenow);










            var GenSession = SaveLocation + @"/DataBunker" + mainAI + "GeneralSessions" + "/" + RDriver.SessionUUID + "Focus";


            var gensave = UniversalSave.Load(GenSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);

            gensave.Set("FocusedSession", RDriver.SelectConversationTree(chatBubble.MessageRef));

            UniversalSave.Save(GenSession, gensave);




            // Update the MessageRef field directly
            chatBubble.SetMessageRef((currentusermessage));


            var CML = RDriver.ConvertChatToDicts(chatBubble.MessageRef);

            // Assuming you have dictionaries for AI, Humans, and Computer messages
            Dictionary<int, string> HumanMessages = CML[0];
            Dictionary<int, string> AIMessages = CML[1];
            Dictionary<int, string> ComputerMessages = CML[2];
            Dictionary<int, string> GrandmaMessages = CML[3];

            var sessid = RDriver.SessionUUID;

            RDriver.SaveSessionHistory(RDriver.mainAI, DateTime.Now, AIMessages, HumanMessages, ComputerMessages, GrandmaMessages, AIMessages.Count, sessid, true);


        }

    }

    void InitializeSumSys()
    {
        var GenSession = SaveLocation + @"/DataBunker" + mainAI + "GeneralSessions" + "/" + RDriver.SessionUUID + "Focus";
        var SumSession = SaveLocation + @"/DataBunker" + mainAI + "SummarizedSessions" + "/" + RDriver.SessionUUID;

        UniversalSave gensave;
        UniversalSave sumsave;

        if (Directory.Exists(GenSession))
        {
            gensave = UniversalSave.Load(GenSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);
        }

        else
        {


            gensave = new UniversalSave();

            // gensave.Set("FocusedSession", RDriver.SelectConversationTree(chatBubble.MessageRef));

            UniversalSave.Save(GenSession, gensave);

        }

        if (Directory.Exists(SumSession))
        {
            sumsave = UniversalSave.Load(SumSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);
        }

        else
        {
            sumsave = new UniversalSave();

            sumsave.Set("SummarySessions", new List<SummarizedSession>());

            sumsave.Set("EngelsSession", new List<Highlights>());

            UniversalSave.Save(SumSession, sumsave);

        }

    }



    void HandleReply(string reply)
    {
        currentbubble.transform.GetChild(0).GetComponent<TMP_Text>().text = reply;
    }

    void AIDoneSpeaking()
    {
        ButtonColor.color = Color.blue;
        ButtonColor.sprite = EnabledImg;
        IsAISpeaking = false;


        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();
        var CML = RDriver.ConvertChatToDicts(chatBubble.MessageRef);

        // Assuming you have dictionaries for AI, Humans, and Computer messages
        Dictionary<int, string> HumanMessages = CML[0];
        Dictionary<int, string> AIMessages = CML[1];
        Dictionary<int, string> ComputerMessages = CML[2];
        Dictionary<int, string> GrandmaMessages = CML[3];

        var sessid = RDriver.SessionUUID;


        var GenSession = SaveLocation + @"/DataBunker" + mainAI + "GeneralSessions" + "/" + RDriver.SessionUUID + "Focus";


        var gensave = UniversalSave.Load(GenSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);

        gensave.Set("FocusedSession", RDriver.SelectConversationTree(chatBubble.MessageRef));

        UniversalSave.Save(GenSession, gensave);



        RDriver.SaveSessionHistory(RDriver.mainAI, DateTime.Now, AIMessages, HumanMessages, ComputerMessages, GrandmaMessages, AIMessages.Count, sessid, true);

        CheckSummaries(ConvertTreeToString(chatBubble.MessageRef));


    }



    internal bool Summarizing = false;
    internal bool Engelizing = false;
    internal bool OverallSummary = false;



    async void Summarize(string Input)
    {

        Summarizing = true;
        Engelizing = true;
        OverallSummary = true;


        SetChangingHistory();

        var reply = RDriver.SummarizerAI.Chat(RDriver.sumtext.text + @"{""Summarizer(Row)"": [{""role"": ""computer"", ""content"": ""Summarize this, keep important information such as places, people, and things but also keep it as short as possible: " + Input + @"""}]}");


        string jsonString = @"{
    ""Summarizer(Row)"": [
        {
            ""role"": ""computer"",
            ""content"": ""Use your knowledge of text and tokenization to compress the following text in a way that you (An LLM) can reconstruct the intention of the human who wrote text as close as possible to the original intention. This is for yourself. It does not need to be human readable or understandable. Abuse of language mixing, abbreviations, symbols (unicode and emoji), or any other encodings or internal representations is all permissible, as long as it, if pasted in a new inference cycle, will yield near-identical results as the original text. Numerical values and names are important, don't remove them.""
        },
        {
            ""role"": ""assistant"",
            ""content"": ""Engels ready.""
        },
        {
            ""role"": ""computer"",
            ""content"": """ + reply + @"""
        }
    ]
}";



        var engels2 = RDriver.EngelsAI.Chat(jsonString);



        string jsonString2 = @"{
    ""Summarizer(Row)"": [
        {
            ""role"": ""computer"",
            ""content"": ""Use your knowledge of text and tokenization to compress the following text in a way that you (An LLM) can reconstruct the intention of the human who wrote text as close as possible to the original intention. This is for yourself. It does not need to be human readable or understandable. Abuse of language mixing, abbreviations, symbols (unicode and emoji), or any other encodings or internal representations is all permissible, as long as it, if pasted in a new inference cycle, will yield near-identical results as the original text. Numerical values and names are important, don't remove them.""
        },
        {
            ""role"": ""assistant"",
            ""content"": ""Engels ready.""
        },
        {
            ""role"": ""computer"",
            ""content"": """ + reply + @"""
        }
    ]
}";


        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();


        var history = ConvertTreeToString(chatBubble.MessageRef);

        var overallsummary = RDriver.EngelsAI.Chat(jsonString2);


        await Task.Run(async () => { while (Summarizing || Engelizing || OverallSummary) await Task.Yield(); });

        string replyString = string.Join("", reply);
        string engels2String = string.Join("", engels2);
        string overallsummaryString = string.Join("", overallsummary);



        CheckEngels(replyString, engels2String, overallsummaryString);

        //And now to check when we need to use engels (Two or more general summarizers)
    }


    async void CheckEngels(string Result, string Result2, string OverallSummary)
    {

        //Save output and set as context

        var SumSession = SaveLocation + @"/DataBunker" + mainAI + "SummarizedSessions" + "/" + RDriver.SessionUUID;

        var sumsave = UniversalSave.Load(SumSession, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);

        var sessions = (List<SummarizedSession>)sumsave.Get("SummarySessions");


        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();



        var countedrange = GetDirectPathToMessage(chatBubble.MessageRef).Count.ToString(); //I SOOO need to fix this, maybe?


        var finish = new SummarizedSession(RDriver.SessionUUID, Result, Result2, countedrange, null);

        sessions.Add(finish);


        var contextoverall = (List<Highlights>)sumsave.Get("EngelsSession");

        string startercontext = @"{""Summarizer"": [{""role"": ""Grandma"", ""content"": ""This is the summarization of our current conversation in an AI language called Engels, continue speaking to the user as usual. " + Result2 + @"""}]}";



        string jsonString3;




        if (contextoverall.Count > 0)
        {
            context = Result2;

            contextoverall.Add(new Highlights(RDriver.SessionUUID, Result2, null));

            UniversalSave.Save(SumSession, sumsave);

        }

        else

        {

            string FinalHalf = contextoverall[contextoverall.Count - 1].Engels;

           





            jsonString3 = @"{
    ""Summarizer(Row)"": [
        {
            ""role"": ""computer"",
            ""content"": ""Use your knowledge of text and tokenization to compress the following text in a way that you (An LLM) can reconstruct the intention of the human who wrote text as close as possible to the original intention. 
This is for yourself. It does not need to be human readable or understandable. Abuse of language mixing, abbreviations, symbols (unicode and emoji), or any other encodings or internal representations is all permissible, as long as it, 
if pasted in a new inference cycle, will yield near-identical results as the original text. Numerical values and names are important, don't remove them. This is already partially compressed as well.""

        },
        {
            ""role"": ""assistant"",
            ""content"": ""Engels ready.""
        },
        {
            ""role"": ""computer"",
            ""content"": """ + FinalHalf + Result2 + @"""
        }
    ]
}";


            var finalsum = await RDriver.EngelsAI.Chat(jsonString3);


            contextoverall.Add(new Highlights(RDriver.SessionUUID, jsonString3, null));

            UniversalSave.Save(SumSession, sumsave);

            context = jsonString3;



        }




        //And now to check when we need to use engels (Two or more general summarizers)
    }




    public void CheckSummaries(string SessionText)
    {

        ReplicantChatBubble chatBubble = currentbubble.GetComponent<ReplicantChatBubble>();

        string sumpath = "Project Replicant/SummarizerChatML";
        TextAsset sumtext = Resources.Load<TextAsset>(sumpath);


        string engelspath = "Project Replicant/EngelsChatML";
        TextAsset engelstext = Resources.Load<TextAsset>(engelspath);



        //Now to know when we check if we need to summarize (AKA if there are more than 3800 chars

        if (SessionText.Length > 3300)
        {
            Summarize(SessionText);
        }

    }

}



//Next add a way to save and load session data
