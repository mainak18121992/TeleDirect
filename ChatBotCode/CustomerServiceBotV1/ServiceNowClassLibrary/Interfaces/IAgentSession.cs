using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceNowClassLibrary.Models;
using ServiceNowClassLibrary.GlobalResources;

namespace ServiceNowClassLibrary.Interfaces
{
    /// <summary>
    /// This IAgentSession class manages a session with a queue/agent using the SNOW CHAT CONNECT platform.
    /// </summary>
    interface IAgentSession
    {
        void StartSession(CreateSessionRequest toast, string user);
        void StartSession(string uri, string initialMessage);

        void StartSession(string uri, List<string> initialMessageList);

        void SendMessage(string message);

        void SendMessage(PostMessageToGroupRquest message, string groupID);

        void CloseSession();

        void Terminate(Action a);

        event EventHandler MessageFromAgent;

        event EventHandler LogMessage;

        event EventHandler SessionEnded;

        event EventHandler SessionInitiated;

        event EventHandler PhraseFromSession;

        event EventHandler AgentConnected;

        
        event EventHandler ConnectionError;


    }
}
