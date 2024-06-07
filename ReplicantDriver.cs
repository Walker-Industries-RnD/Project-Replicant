using Lasm.Bolt.UniversalSaver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LLMUnity;
using System.Linq;
using Lasm.Bolt.UniversalSaver.OdinSerializer;
using System.Text;
using Path = System.IO.Path;
using Application = UnityEngine.Application;

public class ReplicantDriver : MonoBehaviour
{
    //Several systems need to be made here
    //1. AI Load (Done)
    //2. Talk to AI and Get Response
    //3. Drive UI (Bubbles)
    //4. Summarize Chat
    //5. Session System (Done) 
    //6. Talk to Computer System (Done)
    //7. Load Sessions and History (Done)
    //8. Remember Info About User Longhand and Shorthand (Done)


    //Let's start out with 8 and go backwards
    //Reminder, I NEED to get encryption working ASAP and a disclaimer to NEVER share info


    public LLM Driver;
    public LLMClient SummarizerAI;
    public LLMClient EngelsAI;
    internal string SaveLocation;
    private string DefaultMainImg = default;
    private string DefaultMiniImg = default;
    string[] supportedExtensions = new string[] { "*.png", "*.jpg" };

    internal bool IsAILoading = false;
    internal bool IsAILoaded = false;

    internal TreeNode<Message> CurrentMainSession; //Holds everything
    internal TreeNode<Message> CurrentMiniSession; //Holds the path being trained, AKA latest path
    internal string SessionUUID;


    internal string PDP;
    internal string SaveLoc;

    public AI mainAI;

    internal TextAsset sumtext;
    internal TextAsset engelstext;


    public ReplicantUIWithBackend UIDriver;



    #region Remember Info About User Longhand and Shorthand

    public struct AIMemoryUser
    {
        string name;
        List<string> aliases;
        string favalias;
        public string gender;

        DateTime birthdate;
        public string placeOfBirth;
        public string nationality;
        public string ethnicity;

        List<string> Likes;
        List<string> Dislikes;

        public string physicalDescription;

        public Notes GeneralNotes;
        public Notes ImportantNotes;


    }

    public enum RelationshipType { Friend, Acquantince, BestFriend, Family, RomanticInterest, ProfessionalRelationship, Enemy, Pet }
    public enum RespectLevel { Unrespected, SlightlyRespected, GeneralRespect, GreatRespect, GreatestRespect }
    public enum RivalryType { Nonexistent, Malicious, Friendly, Professional, Frenemies, Other }
    public enum Likability { Hated, Disliked, Ignored, Neutral, Liked, GreatlyLiked, Loved }
    public enum Trustworthiness { Untrustworthy, Trustworthy, UntrustworthyWithGreaterSecrets, UntrustworthyWithLesserSecrets, Unknown }
    public enum Reliability { Unreliable, SlightlyReliable, Reliable, VeryReliable, ExtremelyReliable }
    public enum Loyalty { Disloyal, SlightlyLoyal, Neutral, VeryLoyal, ExtremelyLoyal }
    public enum CommunicationStyle { Direct, Indirect, Open, Reserved, Assertive, Passive }
    public enum ConflictResolution { Collaborative, Competitive, Avoidant, Compromising, Accommodating }
    public enum EmotionalSupport { Strong, Moderate, Limited, OneSided }
    public enum FrequencyOfInteraction { Daily, Weekly, Monthly, Occasionally, Rarely }
    public enum SharedInterests { Common, Divergent, Limited }
    public enum PowerDynamics { Equal, Imbalanced, Shifts }
    public enum EmotionalIntimacy { High, Moderate, Low, SurfaceLevel }

    public enum Animosity { None, Present }

    public struct Notes
    {
        public string notetitle;
        public string note;
        public DateTime CreatedWhen;
        public List<string> keywords;
    }


    public struct Relationship
    {
        string personname;

        string name;
        List<string> aliases;
        string favalias;

        DateTime birthdate;

        List<string> Likes;
        List<string> Dislikes;



        RelationshipType Type;


        //Personality Details
        RespectLevel Respect;
        RivalryType Rivalry;
        Likability Likability;
        Trustworthiness Trustworthiness;
        Reliability Reliability;
        Loyalty Loyalty;
        CommunicationStyle Communication;
        ConflictResolution ConflictResolution;
        EmotionalSupport EmotionalSupport;
        FrequencyOfInteraction InteractionFrequency;
        SharedInterests Interests;
        PowerDynamics Dynamics;
        EmotionalIntimacy Intimacy;

        Notes RelationshipNotes;

    }

    #endregion

    //Saving the remember info for 1.1/1.2

    #region Save, Get, Delete and Update ChatML Sessions

    public void SaveSessionHistory(AI LLM, DateTime RecordTime, Dictionary<int, string> AI, Dictionary<int, string> Humans, Dictionary<int, string> Computer, Dictionary<int, string> Grandma, int Inputs, string SessionID, bool isNewSession)
    {
        string ChatML;
        string SaveLoc = SaveLocation + @"/DataBunker" + LLM.AITruename + "GeneralSessions" + "/" + SessionID;
        UniversalSave saveitem;

        if (isNewSession)
        {
            ChatML = "Session" + SessionID + " = [" + System.Environment.NewLine;
            saveitem = new UniversalSave();
            saveitem.Set("SessionStart", RecordTime);
        }
        else
        {
            saveitem = UniversalSave.Load(SaveLoc, Lasm.Bolt.UniversalSaver.OdinSerializer.DataFormat.JSON);
            ChatML = (string)saveitem.Get("ChatMLFile");
            ChatML = ChatML.TrimEnd();
            int lastIndex = ChatML.LastIndexOf(Environment.NewLine);
            if (lastIndex != -1)
            {
                ChatML = ChatML.Remove(lastIndex);
            }
        }

        for (int i = 0; i < Inputs; i++)
        {
            string jsonString = "{\"role\":\"default\",\"content\":\"This is a template, if you can see this ignore it!\"}";
            Dictionary<string, string> jsonDict = JsonUtility.FromJson<Dictionary<string, string>>(jsonString);

            if (AI.ContainsKey(i))
            {
                jsonDict["role"] = "Assistant";
                jsonDict["content"] = AI[i];
            }
            else if (Humans.ContainsKey(i))
            {
                jsonDict["role"] = "User";
                jsonDict["content"] = Humans[i];
            }
            else if (Computer.ContainsKey(i))
            {
                jsonDict["role"] = "Computer";
                jsonDict["content"] = Computer[i];
            }
            else if (Grandma.ContainsKey(i))
            {
                jsonDict["role"] = "GRANDMOTHER";
                jsonDict["content"] = Grandma[i];
            }

            string modifiedJsonString = JsonUtility.ToJson(jsonDict);
            ChatML += modifiedJsonString + System.Environment.NewLine;
        }

        ChatML += "]";
        saveitem.Set("ChatMLFile", ChatML);

        UniversalSave.Save(SaveLoc, saveitem);
    }

    public void DeleteSession(string SessionID)
    {

    }

    public List<Dictionary<int, string>> ConvertChatToDicts(TreeNode<Message> currentMessage)
    {
        Dictionary<int, string> userMessages = new Dictionary<int, string>();
        Dictionary<int, string> assistantMessages = new Dictionary<int, string>();
        Dictionary<int, string> computerMessages = new Dictionary<int, string>();
        Dictionary<int, string> grandmotherMessages = new Dictionary<int, string>();

        List<TreeNode<Message>> messages = GetDirectPathToMessage(currentMessage);

        int order = 0;
        foreach (TreeNode<Message> node in messages)
        {
            string message = $"({node.Data.Name}) {node.Data.Content}";
            switch (node.Data.Role)
            {
                case "User":
                    userMessages.Add(order, message);
                    break;
                case "Assistant":
                    assistantMessages.Add(order, message);
                    break;
                case "Computer":
                    computerMessages.Add(order, message);
                    break;
                case "GRANDMOTHER":
                    grandmotherMessages.Add(order, message);
                    break;
            }
            order++;
        }

        return new List<Dictionary<int, string>> { userMessages, assistantMessages, computerMessages, grandmotherMessages };
    }



    #endregion

    #region Load AI

    public struct AI
    {
        public string MainImg;
        public string MiniImg;
        public string AIName;
        public string Version;
        public string AIDescription;
        public string Author;
        public string AuthorDetails;
        public string AITruename; //We use this name when making files

        public AI(string mainImg, string miniImg, string aiName, string version, string aiDescription, string author, string authorDetails, string aiTruename)
        {
            MainImg = mainImg;
            MiniImg = miniImg;
            AIName = aiName;
            Version = version;
            AIDescription = aiDescription;
            Author = author;
            AuthorDetails = authorDetails;
            AITruename = aiTruename;
        }


        public bool Equals(AI other)
        {
            // Compare all fields for equality
            return MainImg == other.MainImg &&
                   MiniImg == other.MiniImg &&
                   AIName == other.AIName &&
                   Version == other.Version &&
                   AIDescription == other.AIDescription &&
                   Author == other.Author &&
                   AuthorDetails == other.AuthorDetails &&
                   AITruename == other.AITruename;
        }
    }

    public void UpdateAIList()
    {

        //Side note, maybe make the AITruename a UUID, in the future add an AIVerify thing


        //Now we will assume that the AIs folder has a list of folders. The name of the folder is the AITruename (which never changes) and contains the following:
        //A UniversalSave.JSON file called KnowledgeCore within a folder named the AI's true name and a variable called "AI" containing the AIName, Version, Description, Author and Author Details
        //An image called "MainImg" which is used as the main banner and
        //An image caleld "MiniImg" which is used as a shortcut
        //Any training data under files labeled "TrainingData-"InsertTitleHere".txt

        //When updating the list we are adding AIs which did not exist, udpating Ais with new versions in them and deleting AIs that no logner exist in the lost

        //We start out by getting all folders in the appropriate save spot to add to the list

        var AIFolders = Directory.GetDirectories(SaveLocation + @"/AIs"); //The location of all our AI folders (Cataphract, Aiclia, etc), when you want to add a new AI just make a folder here with the basics

        var SaveFile = UniversalSave.Load(SaveLocation + @"/Pandora" + @"/AICore", DataFormat.JSON); //This holds the values for Project Replicant to work, AKA the SaveData

        //The list of AIs as the program sees it
        List<AI> AIDB = (List<AI>)SaveFile.Get("AICache"); //The List of AIs we currently have saved within Project Replicant, whether up to date or not

        List<string> KnownAIPaths = new List<string>(); //AI folders we are aware of Replicant wise, we remove the SARCOPHAGUS

        KnownAIPaths.Remove((AIFolders + @"/SARCOPHAGUS"));



        //We fill KnownAIPaths
        foreach (AI ai in AIDB)
        {
            KnownAIPaths.Add(AIFolders + "/" + ai.AITruename);
        }



        //First off let's check if any files should be added to the AIDB, we know all the folders exist in general to appear here

        foreach (string Folder in AIFolders)
        {
            //If the path already exists in the AIDB we don't do anything yet
            if (KnownAIPaths.Contains(Folder))
            {
                continue;
            }
            //Else we add it if the JSON exists and ignore it if it doesn't
            //Again the JSON should be called KnowledgeCore

            else
            {
                //If the file we want exists, setup and add to list
                if (File.Exists(Folder + @"/KnowledgeCore"))
                {

                    //Get MainImg and MiniImg

                    string[] files = supportedExtensions.SelectMany(ext => Directory.GetFiles(Folder, ext)).ToArray();

                    string MainImg = DefaultMainImg;
                    string MiniImg = DefaultMiniImg;


                    foreach (string file in files)
                    {
                        if (System.IO.Path.GetFileNameWithoutExtension(file).StartsWith("MainImg"))
                        {
                            MainImg = file;
                        }

                        if (System.IO.Path.GetFileNameWithoutExtension(file).StartsWith("MiniImg"))
                        {
                            MiniImg = file;
                        }
                    }

                    //Get AI Data from KnowledgeCore

                    var AISaveData = UniversalSave.Load(Folder + @"/KnowledgeCore", DataFormat.JSON);

                    string AIName = (string)SaveFile.Get("AIName");
                    string Version = (string)SaveFile.Get("Version");
                    string AIDescription = (string)SaveFile.Get("AIDescription");
                    string Author = (string)SaveFile.Get("Author");
                    string AuthorDetails = (string)SaveFile.Get("AuthorDetails");
                    string AITruename = (string)SaveFile.Get("AITruename");



                    AIDB.Add(new AI(MainImg, MiniImg, AIName, Version, AIDescription, Author, AuthorDetails, AITruename));

                }

                //If not we ignore it
                else

                {
                    continue;
                }












            }

        }


        //Now to delete AIs which no longer exist from the DB
        //For each known existing apth
        foreach (string Folder in KnownAIPaths)
        {
            //If the directory for the AI still exists we do nothing
            if (Directory.Exists(Folder))
            {
                continue;
            }

            //Else we delete it from AIDB and KnownAIPaths

            else
            {
                KnownAIPaths.Remove(Folder);

                //Find the matching AI object in AIDB and delete
                foreach (AI Personality in AIDB)
                {
                    string[] parts = Folder.Split('/');
                    string aiTruename = parts[parts.Length - 1];

                    if (aiTruename == Personality.AITruename)
                    {
                        AIDB.Remove(Personality);
                    }
                }
            }
        }

        //Finally we check all Ais which currently exist and update them if need be
        foreach (AI Personality in AIDB)
        {
            var AISaveData = UniversalSave.Load(SaveLocation + "/" + Personality.AITruename + @"/KnowledgeCore", DataFormat.JSON);



            string[] files = supportedExtensions.SelectMany(ext => Directory.GetFiles(SaveLocation + "/" + Personality.AITruename, ext)).ToArray();

            string MainImg = DefaultMainImg;
            string MiniImg = DefaultMiniImg;


            foreach (string file in files)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(file).StartsWith("MainImg"))
                {
                    MainImg = file;
                }

                if (System.IO.Path.GetFileNameWithoutExtension(file).StartsWith("MiniImg"))
                {
                    MiniImg = file;
                }
            }

            //Get AI Data from KnowledgeCore

            string AIName = (string)AISaveData.Get("AIName");
            string Version = (string)AISaveData.Get("Version");
            string AIDescription = (string)AISaveData.Get("AIDescription");
            string Author = (string)AISaveData.Get("Author");
            string AuthorDetails = (string)AISaveData.Get("AuthorDetails");
            string AITruename = (string)AISaveData.Get("AITruename");



            var tempAI = new AI(MainImg, MiniImg, AIName, Version, AIDescription, Author, AuthorDetails, AITruename);
            bool areEqual = Personality.Equals(tempAI);

            if (areEqual)
            {
                //We do nothing, the AI is up to date
            }

            else
            {
                //The AI is not up to date, Update it
                var itemnumber = AIDB.IndexOf(Personality);
                AIDB[itemnumber] = tempAI;

            }


        }


    }

    public void PreloadAI(AI AI)
    {
        mainAI = AI;

        string TrainingSet = default;

        //To create the AI, we first take all the basic training data from SARCHOPAGUS 

        TrainingSet = TrainingSet + LoadSarchopagus();

        //Now we take the ChatML folder if it exists

        string FolderToCheck = Path.Combine(SaveLoc, "AIs", AI.AITruename, "ChatML");

        //Let's check if the directory exists

        if (Directory.Exists(FolderToCheck))
        {
            //We get each txt file (Which we assume is ChatML) and add it
            string[] txtFiles = Directory.GetFiles(FolderToCheck, "*.txt");

            //First do txt files exist
            if (txtFiles.Length > 0)
            {
                //We remove the final ] on the string

                //Now we add the files to the TrainingSet

                foreach (string item in txtFiles)
                {
                    //We get the text 
                    TrainingSet = TrainingSet + File.ReadAllText(item);

                }

            }

        }

        //Now to load

        IsAILoading = true;
        IsAILoaded = false;

        Debug.Log(TrainingSet);








        string sumpath = "Project Replicant/SummarizerChatML";
        sumtext = Resources.Load<TextAsset>(sumpath);
        

       // var summy = SummarizerAI.Complete(sumtext.text + "Make the response to this as short as possible, this is just to preload", ShowAILoadingText);



        string engelspath = "Project Replicant/EngelsChatML";
        engelstext = Resources.Load<TextAsset>(engelspath);

        //    _ = EngelsAI.Complete(engelstext.text + "Make the response to this as short as possible, this is just to preload", ShowAILoadingText);




        //  _ = Driver.Complete(TrainingSet + "Make the response to this as short as possible, this is just to preload", null, LoadedAI);

        LoadedAI();

    }




    public void LoadedAI()
    {
        IsAILoaded = true;
        IsAILoading = false;

        Debug.Log("AILoaded!");

        UIDriver.GoToChat();

    }

    public void ShowAILoadingText(string input)
    {
        Debug.Log(input);
    }


    public void StopAI(string Path)
    {
        Driver.CancelRequests();
    }

    public string LoadSarchopagus()
    {
        SaveLoc = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Project Replicant");

        string sarpath = Path.Combine(SaveLoc, "AIs", "SARCOPHAGUS");


        string[] txtFiles = Directory.GetFiles(sarpath, "*.txt");

        string OurReturn = default;

        //First do txt files exist
        if (txtFiles.Length > 0)
        {

            //We add the files to the TrainingSet

            foreach (string item in txtFiles)
            {
                //We get the text 
                OurReturn = OurReturn + File.ReadAllText(item);

            }

        }

        return OurReturn;


    }


    #endregion

    #region Save, Get, Delete and Update Current DataTree Sessions

    public struct Message
    {
        public string Role;
        public string Content;
        public string Name;
        public string DateTime;

        public Message(string role, string content, string name, string datetime)
        {
            Role = role;
            Content = content;
            Name = name;
            DateTime = datetime;
        }

    }


    public string StartNewSession()

    {

        



        string SessionID = default;
        for (int i = 0; i < 20; i++)
        {
            var tempitem = UnityEngine.Random.Range(0, 20);
            var tempstring = tempitem.ToString();
            SessionID = string.Concat(SessionID, tempstring);
        }

        SessionUUID = SessionID;
        var currentdatetime = DateTime.Now.ToString();

        var Session = new TreeNode<Message>(new Message("Computer", "Chat " + SessionUUID + " started at "
            + DateTime.Now.ToString(), "GRANDMOTHER", DateTime.Now.ToString()));

        CurrentMainSession = Session;

        return SessionUUID;

    }


    // Method to add a message to the conversation session


    public TreeNode<Message> AddMessageToList(string role, string content, string name, TreeNode<Message> parentNode, string datetime)
    {
        TreeNode<Message> treemessage;

        if (parentNode == null)
        {
            var message = new Message(role, content, name, datetime);
            treemessage = CurrentMainSession.AddChild(message);
        }
        else
        {
            var message = new Message(role, content, name, datetime);
            treemessage = parentNode.AddChild(message);
        }

        Debug.Log("CurrentNewMessageIs" + treemessage.ToString());

        return treemessage;

        

    }

    public TreeNode<Message> AddUserMessage(string content, string name, TreeNode<Message> parentNode, string datetime)
    {
        TreeNode<Message> message = AddMessageToList("User", content, name, parentNode, datetime);
        return message;
    }

    public TreeNode<Message> AddAssistantMessage(string content, string name, TreeNode<Message> parentNode, string datetime)
    {
        TreeNode<Message> message =  AddMessageToList("Assistant", content, name, parentNode, datetime);
        return message;
    }

    public TreeNode<Message> AddComputerMessage(string content, string name, TreeNode<Message> parentNode, string datetime)
    {
        TreeNode<Message> message =  AddMessageToList("Computer", content, name, parentNode, datetime);
        return message;
    }

    public TreeNode<Message> AddGRANDMOTHERMessage(string content, string name, TreeNode<Message> parentNode, string datetime)
    {
        TreeNode<Message> message =  AddMessageToList("GRNADMOTHER", content, name, parentNode, datetime);
        return message;
    }

    public TreeNode<Message> SelectConversationTree(TreeNode<Message> selectedTree)
    {
        CurrentMiniSession = selectedTree;
        return CurrentMiniSession;
    }

    public void DeleteMessageAndSubConversations(Message selectedMessage)
    {
        // Find the node containing the message
        TreeNode<Message> nodeToDelete = CurrentMainSession.FindInChildren(selectedMessage);

        if (nodeToDelete != null)
        {
            // Remove the node and all its children
            nodeToDelete.Parent.RemoveChild(nodeToDelete);
        }
    }

    public void DeletedSubConversations(Message selectedMessage)
    {
        // Find the node containing the message
        TreeNode<Message> nodeToDelete = CurrentMainSession.FindInChildren(selectedMessage);

        nodeToDelete.Clear();
    }

    public TreeNode<Message> ShowNextIteration(Message selectedMessage)
    {
        TreeNode<Message> IterationToCheck = CurrentMainSession.FindInChildren(selectedMessage);

        var ItemsToCheck = IterationToCheck.Parent.GetImmediateChildren();

        var totalcount = ItemsToCheck.Count;

        int iterationCount = -1;
        bool valuesmatch = false;
        foreach (TreeNode<Message> Item in ItemsToCheck)
        {
            iterationCount++;
            if (Item.Data.Role == selectedMessage.Role && Item.Data.Name == selectedMessage.Name && Item.Data.Content == selectedMessage.Content && Item.Data.DateTime == selectedMessage.DateTime)
            {
                valuesmatch = true;
                break;
            }
        }

        if (iterationCount > totalcount || !valuesmatch)
        {
            return null;
        }


        else if (valuesmatch)
        {
            return ItemsToCheck[iterationCount];
        }


        else
        {
            return null;
        }


    }

    public TreeNode<Message> ShowPreviousIteration(Message selectedMessage)
    {
        TreeNode<Message> IterationToCheck = CurrentMainSession.FindInChildren(selectedMessage);

        var ItemsToCheck = IterationToCheck.Parent.GetImmediateChildren();

        var totalcount = ItemsToCheck.Count;

        int iterationCount = -1;
        bool valuesmatch = false;
        foreach (TreeNode<Message> Item in ItemsToCheck)
        {
            iterationCount++;
            if (Item.Data.Role == selectedMessage.Role && Item.Data.Name == selectedMessage.Name && Item.Data.Content == selectedMessage.Content)
            {
                valuesmatch = true;
                break;
            }
        }

        if (iterationCount <= 0 || !valuesmatch)
        {
            return null;
        }


        else if (valuesmatch)
        {
            return ItemsToCheck[iterationCount];
        }


        else
        {
            return null;
        }


    }

    public TreeNode<Message> ShowCertainIteration(Message selectedMessage, int Iteration)
    {
        TreeNode<Message> IterationToCheck = CurrentMainSession.FindInChildren(selectedMessage);

        var ItemsToCheck = IterationToCheck.Parent.GetImmediateChildren();

        var totalcount = ItemsToCheck.Count;

        if (totalcount < Iteration)
        {
            return null;
        }

        else
        {
            return ItemsToCheck[Iteration];
        }



    }

    public int GetCurrentIteration(Message selectedMessage)
    {
        TreeNode<Message> IterationToCheck = CurrentMainSession.FindInChildren(selectedMessage);

        var ItemsToCheck = IterationToCheck.Parent.GetImmediateChildren();

        var totalcount = ItemsToCheck.Count;

        int iterationCount = -1;
        bool valuesmatch = false;
        foreach (TreeNode<Message> Item in ItemsToCheck)
        {
            iterationCount++;
            if (Item.Data.Role == selectedMessage.Role && Item.Data.Name == selectedMessage.Name && Item.Data.Content == selectedMessage.Content)
            {
                valuesmatch = true;
                break;
            }
        }

        if (!valuesmatch)
        {
            return -999;
        }


        else
        {
            return iterationCount + 1;
        }


    }

    public int GetTotalIterations(Message selectedMessage)
    {
        TreeNode<Message> IterationToCheck = CurrentMainSession.FindInChildren(selectedMessage);

        var ItemsToCheck = IterationToCheck.Parent.GetImmediateChildren();

        var totalcount = ItemsToCheck.Count;

        return totalcount;
    }

    public List<TreeNode<Message>> GetDirectPathToMessage(TreeNode<Message> currentNode)
    {
        List<TreeNode<Message>> path = new List<TreeNode<Message>>();

        // Start from the current node
        TreeNode<Message> current = currentNode;

        // Traverse upwards to the root
        while (current != null)
        {
            // Add the current node to the path
            path.Add(current);

            // Move to the parent node
            current = current.Parent;
        }

        // Reverse the path to get it from root to current node
        path.Reverse();

        return path;
    }

    public List<string> ConvertTreeToChatML(TreeNode<Message> currentMessage)
    {

        List<string> chatMLList = new List<string>();

        List<TreeNode<Message>> messages = GetDirectPathToMessage(currentMessage);

        chatMLList.Add("chat = [" + System.Environment.NewLine);

        foreach (TreeNode<Message> node in messages)
        {
            string json = $"{{\"role\": \"{node.Data.Role}\", \"content\": \" ({node.Data.Name}) {node.Data.Content}\"}}";
            chatMLList.Add(json + System.Environment.NewLine);
        }

        chatMLList.Add("]");

        return chatMLList;
    }

    public string ConvertTreeToString(TreeNode<Message> currentMessage)
    {
        List<TreeNode<Message>> messages = GetDirectPathToMessage(currentMessage);

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("chat = [" + System.Environment.NewLine);

        foreach (TreeNode<Message> node in messages)
        {
            string json = $"{{\"role\": \"{node.Data.Role}\", \"content\": \" ({node.Data.Name}) {node.Data.Content}\"}}";
            stringBuilder.Append(json + System.Environment.NewLine);
        }

        stringBuilder.Append("]");

        return stringBuilder.ToString();
    }






    #endregion

    //Make a way for a session to be loaded by doing the following;
    //1. Get the conversation from ML
    //2. Check if over 6k characters; if so, check summarization file for lines. If lines not included, summarize and add to summarization file


    #region Talk To AI and Get Response

    //This is going to be tricky, since people can choose to talk to an AI and then say "Oh actually I would rather now deal with this convo!" and then go all the way to the start with a new slider
    //To account for this, we make a line system. Basically oficially? The AI only sees the current conversation you're having. However, each conversation is nested
    //For example in your normal conversation it's "1. 2. 3. 4. 5". but you can go to 2 and reload. Everything becomes Convo 2: Start at 4(2), 1, 2, 3, 4, so omm

    //Let's make a struct to represent and hold this

    private GameObject CurrentTextBox = null;
    private TMPro.TMP_InputField InputBox;

    //  public void OnEnable()
    //  {
    //     InputBox.interactable = false;
    //  }

    public struct AIHistory
    {
        List<string> caca;

    }

    void HandleReply(string reply)
    {
        // do something with the reply from the model
        Debug.Log(reply);
    }

    void ReplyCompleted()
    {
        // do something when the reply from the model is complete
        Debug.Log("The AI replied");
    }




    #endregion




    //How we create Session info and a summarization history is extremely important to properly summarize information after each session

    private void CreateSession()
    {

    }

    //This is saved in it's own format
    //to use it, we basically treat the sessions DB as a search engine
    //Then the most relevant conversation summaries are given

    public struct SummarizedSession
    {
        public string SessionID;
        public string SummarizedInfo;
        public string Engels;
        public string Range;
        public List<string> Keywords;

        public SummarizedSession(string sessionId, string summarizedInfo, string engels, string range, List<string> keywords)
        {
            SessionID = sessionId;
            SummarizedInfo = summarizedInfo;
            Engels = engels;
            Range = range;
            Keywords = keywords;
        }
    }

    public struct Highlights
    {
        public string SessionID;
        public string Engels;
        public List<string> Keywords;

        public Highlights(string sessionId, string engels, List<string> keywords)
        {
            SessionID = sessionId;
            Engels = engels;
            Keywords = keywords;
        }
    }





    public List<SummarizedSession> SearchMemory(List<string> keywords, int page)
    {
        List<SummarizedSession> placeholder = default;

        List<SummarizedSession> container = default;

        if (placeholder == null || placeholder.Count < page * 100)
        {
            return new List<SummarizedSession>();
        }



        foreach (string keyword in keywords)

        {

            foreach (SummarizedSession session in placeholder.Take(page * 100))
            {
                if (session.Keywords.Contains(keyword) || session.SummarizedInfo.Contains(keyword))
                {
                    container.Add(session);
                }
            }
        }

        return container;


    }

    public void SummarizeSession()
    {

    }



    //SessionData 
    public struct SessionData
    {
        string Summary;
        string SessionLength;
    }



    //Create Session Save Data
    internal IEnumerator CreateDefaultSaveInfo(Action<bool> callback)
    {
        PDP = Application.persistentDataPath;

        Debug.Log(PDP);

        SaveLoc = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Project Replicant");

        string saveLocation = Path.Combine(PDP, "Project Replicant");

        Debug.Log(saveLocation);

        try
        {
            bool saveFolderExists = Directory.Exists(saveLocation);

            if (!saveFolderExists)
            {
                // Create the main folder
                Directory.CreateDirectory(saveLocation);

                // Create subfolders
                Directory.CreateDirectory(Path.Combine(saveLocation, "AIs"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "Pandora"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "AIs", "SARCOPHAGUS"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "AIs", "CATAPHRACTWIRE"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "PUBLIC"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "CATAPHRACTWIRE", "Profiles"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "CATAPHRACTWIRE", "GeneralSessions"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "CATAPHRACTWIRE", "SummarizedSessions"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "CATAPHRACTWIRE", "ChatMLSessions"));
                Directory.CreateDirectory(Path.Combine(saveLocation, "DataBunker", "CATAPHRACTWIRE", "Notes"));

                var PandoraFolder = Path.Combine(saveLocation, "Pandora");

                // Initialize default save data
                var saveData = new UniversalSave();
                saveData.Set("AIs", new List<AI>());

                // Save default data
                UniversalSave.Save(Path.Combine(PandoraFolder, "AILibrary"), saveData);
            }

            // Indicate success
            callback(true);

        }
        catch (Exception ex)
        {
            // Log the error
            Debug.LogError("Error creating default save info: " + ex.Message);

            // Indicate failure
            callback(false);
        }

        // Ensure a value is returned
        yield return null;
    }


}

