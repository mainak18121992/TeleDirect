//using ServiceNowClassLibrary.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;

//namespace ServiceNowClassLibrary
//{
//    /// <summary>
//    /// This IAgentSession class manages a session with a queue/agent using the Avaya AIC platform.
//    /// </summary>
//    class AICSession : IAgentSession
//    {

//        #region Private Variables

//        /// <summary>
//        /// The display name of the responding agent.
//        /// </summary>
//        private string agentName = string.Empty;

//        /// <summary>
//        /// An instance of the XmppSession class, which handles all XMPP interactions.
//        /// </summary>
//        private XmppSession xmppSession;

//        /// <summary>
//        /// Timer for cancelling an unresponsive session.
//        /// </summary>
//        private System.Threading.Timer messageTimeoutTimer;

//        /// <summary>
//        /// Timer for alerting the user the connection is still being established.
//        /// </summary>
//        private System.Threading.Timer messageWaitingTimer;

//        /// <summary>
//        /// A reference to the Controller instance which owns this AICSession instance.
//        /// </summary>
//        private IServiceController controller;

//        /// <summary>
//        /// Indicates if the queue has responded to the session request.
//        /// </summary>
//        private bool connected = false;

//        /// <summary>
//        /// A list of messages to be forwarded to the live agent once the connection has been established.
//        /// </summary>
//        private List<string> initialMessageList = new List<string>();

//        /// <summary>
//        /// Callback Action for module termination.
//        /// </summary>
//        private Action terminationCallback;

//        #endregion Private Variables

//        #region Constants and Structures

//        /// <summary>
//        /// Enumerates the Ids for the AIC bridge messages.
//        /// </summary>
//        private enum BridgeMessage
//        {
//            CONNECTION_CLOSED,
//            CONNECTION_CLOSED_TIMEOUT,
//            CONNECTION_CLOSED_GLOBAL_TIMEOUT,
//            CONNECTION_CLOSED_INTERNAL_ERROR,
//            CONNECTION_CLOSED_NO_AGENTS,
//            CONNECTION_CLOSED_SERVICE_STOPPED,
//            WAITING_FOR_AGENT,
//            WAIT_TO_SEND_MESSAGES,
//            CONNECTION_ESTABLISHED,
//            SESSION_TIMEOUT_WARNING_CUSTOMER,
//            SESSION_TIMEOUT_WARNING_AGENT,
//            ERROR_SENDING_MESSAGE,
//            OPERATION_NOT_SUPPORTED,
//            ACCOUNT_DISABLED,
//            NONE
//        }

//        /// <summary>
//        /// Enumerates a series of categories for AIC bridge messages.
//        /// These categories negate the need to use long boolean expressions.
//        /// </summary>
//        private enum BridgeMessageType
//        {
//            Closed,
//            Established,
//            Error,
//            TimeoutWarning,
//            Waiting,
//            None
//        }

//        #endregion Constants and Structures

//        #region Event Handlers

//        /// <summary>
//        /// Informs the Service Controller that a message from the XMPP session has been received.
//        /// </summary>
//        public event EventHandler MessageFromAgent;

//        /// <summary>
//        /// Informs the Service Controller of a log message.
//        /// </summary>
//        public event EventHandler LogMessage;

//        /// <summary>
//        /// Informs the Service Controller that a connection to the queue has been established.
//        /// This generally indicates that messages can now be forwarded to the agent.
//        /// </summary>
//        public event EventHandler SessionInitiated;

//        /// <summary>
//        /// Informs the Service Controller that the XMPP session has been closed.
//        /// </summary>
//        public event EventHandler SessionEnded;

//        /// <summary>
//        /// Informs the Service Controller that a message should be sent to the user.
//        /// The message is described by a PhraseId, so localization does not need to be handled in this module.
//        /// </summary>
//        public event EventHandler PhraseFromSession;

//        /// <summary>
//        /// Informs the Service Controller that an agent has responded.
//        /// Passes the agent's handle.
//        /// </summary>
//        public event EventHandler AgentConnected;

//        /// <summary>
//        /// Informs the service controller that a connection error has occured.
//        /// </summary>
//        public event EventHandler ConnectionError;

//        #endregion Event Handlers

//        #region Constructor

//        /// <summary>
//        /// Constructor for a new instance of the AICSession class.
//        /// </summary>
//        /// <param name="controller"> A reference to the calling Controller object. </param>
//        public AICSession(IServiceController controller)
//        {
//            this.controller = controller;
//        }

//        #endregion Constructor

//        #region Public Methods

//        /// <summary>
//        /// Attempts to initiate a new XMPP session with the specified queue.
//        /// Registers for events with the XMPP session.
//        /// </summary>
//        /// <param name="invitationTargetUri"> The SIP of the queue to be contacted. </param>
//        /// <param name="_initialMessageList"> A list of messages to be forwarded to the live agent. </param>
//        public void StartSession(string invitationTargetUri, List<string> _initialMessageList)
//        {
//            this.initialMessageList = _initialMessageList;
//            SendConnectingMessage();
//            if (!string.IsNullOrEmpty(invitationTargetUri))
//            {
//                // Create an XMPP session.
//                invitationTargetUri = invitationTargetUri.ToLower().Replace("sip:", "");
//                LogMessage("Attempting to open XMPP connection to " + invitationTargetUri, null);
//                xmppSession = new XmppSession(invitationTargetUri);

//                // Register for events.
//                xmppSession.RegisterForIncomingMessage(XmppSessionMessageReceived);
//                xmppSession.RegisterForMessagingErrors(XmppSessionMessagingError);
//                xmppSession.RegisterForLogMessages(XmppSessionLogMessageReceived);

//                // Open the session.
//                xmppSession.Open(XmppSessionEstablished);
//            }
//            else
//            {
//                LogMessage("No SIP provided for the queue.", null);
//                SendErrorMessage();
//                ConnectionError(1, null);
//                XmppSessionClose(true);
//            }
//        }

//        /// <summary>
//        /// Overloaded constructor takes a single string as an argument for initial message.
//        /// </summary>
//        /// <param name="invitationTargetUri"> The SIP of the queue to be contacted. </param>
//        /// <param name="_initialMessage"> The message to be forwarded to the live agent. </param>
//        public void StartSession(string invitationTargetUri, string _initialMessage)
//        {
//            StartSession(invitationTargetUri, new List<string> { _initialMessage });
//        }

//        /// <summary>
//        /// Public method to close the XMPP session from the Controller.
//        /// </summary>
//        public void CloseSession()
//        {
//            XmppSessionClose(false);
//        }

//        /// <summary>
//        /// This method is called to signal that the call is being terminated.
//        /// Close the XMPP session and return control to the specified Action.
//        /// </summary>
//        /// <param name="a"></param>
//        public void Terminate(Action a)
//        {
//            LogMessage("Terminating AIC Session Controller.", null);
//            terminationCallback = a;

//            // Perform any shutdown tasks here.
//            // Async tasks may call Terminated() when complete instead.
//            CloseSession();

//            Terminated();
//        }

//        #endregion Public Methods

//        #region Private Methods

//        /// <summary>
//        /// Callback function for any async termination tasks.
//        /// Returns control to the Action specified in the termination signal.
//        /// </summary>
//        private void Terminated()
//        {
//            this.xmppSession = null;
//            terminationCallback();
//        }

//        /// <summary>
//        /// Callback function for the XMPP "Open" function.
//        /// Starts timers to wait for the queue's initial response.
//        /// Closes the session if there was an error opening the connection.
//        /// </summary>
//        private void XmppSessionEstablished(string infoMessage)
//        {
//            if (xmppSession.ConnectionStatus)
//            {
//                LogMessage("XMPP Session Established.", null);
//                // Send an initial message(s) to the queue.
//                // Create a thread to check if a message has been recieved within a time limit.
//                messageWaitingTimer = new System.Threading.Timer(obj => { XMPPSessionCheckResponse(); }, null, 20000, System.Threading.Timeout.Infinite);
//                messageTimeoutTimer = new System.Threading.Timer(obj => { XMPPSessionTimeoutResponse(); }, null, 40000, System.Threading.Timeout.Infinite);
//                if (this.initialMessageList != null)
//                {
//                    // If no initial messages were provided, send this error symbol.
//                    if (this.initialMessageList.Count() == 0) this.initialMessageList = new List<string> { "!" };
//                    foreach (string s in this.initialMessageList)
//                    {
//                        xmppSession.SendMessage(s);
//                    }
//                }
//                else
//                {
//                    // If no initial messages were provided, send this error symbol.
//                    xmppSession.SendMessage("!");
//                }
//            }
//            else
//            {
//                LogMessage("Could not open XMPP session: [ " + infoMessage + " ]", null);
//                SendErrorMessage();
//                ConnectionError(1, null);
//                XmppSessionClose(true);
//            }
//        }

//        /// <summary>
//        /// This function is called if the queue has not responded by the time the first timer ends.
//        /// The users should be alert that the connection is still establishing.
//        /// </summary>
//        private void XMPPSessionCheckResponse()
//        {
//            if (connected) return;
//            SendStillWaitingMessage();
//            if (messageWaitingTimer != null) messageWaitingTimer.Dispose();
//        }

//        /// <summary>
//        /// This funciton is called if the queue has not responded by the time the second timer ends.
//        /// The connection should be closed, the user alerted, and the Controller notified.
//        /// </summary>
//        private void XMPPSessionTimeoutResponse()
//        {
//            if (connected) return;
//            LogMessage("Queue did not respond to user, closing XMPP session.", null);
//            SendErrorMessage();
//            ConnectionError(0, null);
//            XmppSessionClose(true);
//            if (messageTimeoutTimer != null) messageTimeoutTimer.Dispose();
//        }

//        /// <summary>
//        /// Closes out an XMPP session.
//        /// </summary>
//        /// <param name="isError"> If "true", sends an error message to the user. </param>
//        private void XmppSessionClose(bool isError)
//        {
//            // Stop any running timers.
//            if (messageTimeoutTimer != null) messageTimeoutTimer.Dispose();
//            if (messageWaitingTimer != null) messageWaitingTimer.Dispose();

//            if (xmppSession == null) return;

//            if (isError)
//            {
//                LogMessage("Attempting to close XMPP session due to error...", null);
//            }
//            else
//            {
//                LogMessage("Attempting to close XMPP session...", null);
//            }

//            try
//            {
//                xmppSession.Close();
//                xmppSession = null;
//                LogMessage("XMPP session closed.", null);
//                this.connected = false;
//            }
//            catch (Exception e)
//            {
//                LogMessage("Error closing XMPP session: " + e.Message, null);
//            }

//            // Raise the session ended event.
//            SessionEnded(null, null);
//        }

//        /// <summary>
//        /// Event handler registered with the XmppSession for incoming messages from an agent/queue.
//        /// Messages are forwarded to the user.
//        /// Extracts the agent's name from the message (first time only).
//        /// </summary>
//        /// <param name="message"> The text of the message. </param>
//        /// <param name="sender"> The SIP of the agent/queue. </param>
//        private void XmppSessionMessageReceived(string message, string sender)
//        {
//            // Only consider the session connected once the first message is reviceved from the queue.
//            if (!connected)
//            {
//                if (messageWaitingTimer != null) messageWaitingTimer.Dispose();
//                if (messageTimeoutTimer != null) messageTimeoutTimer.Dispose();
//                connected = true;
//            }

//            LogMessage("Message revieved from " + sender + " : " + message, null);

//            // Remove ID tags if they are present (old Enhanced Chat modification).
//            string regex = "(\\[#.*\\])";
//            message = Regex.Replace(message, regex, "").Trim();

//            Tuple<BridgeMessage, BridgeMessageType> bridgeMessage = CheckForBridgeMessage(message);

//            if (bridgeMessage.Item2 == BridgeMessageType.Closed)
//            {
//                SendXmppSessionClosedMessage();
//                XmppSessionClose(false);
//                return;
//            }

//            if (bridgeMessage.Item2 == BridgeMessageType.Established)
//            {
//                SessionInitiated(null, null);
//            }

//            // Store the name of the responding agent (if this hasn't already occured).
//            if (String.IsNullOrEmpty(this.agentName) && message.Length > 0)
//            {
//                if (message.Substring(0, 1) == "[" && message.IndexOf("]") != -1)
//                {
//                    string name = message.Substring(1, message.IndexOf("]") - 1).ToLower();
//                    if (name != "system")
//                    {
//                        this.agentName = name;
//                        AgentConnected(name, null);
//                        LogMessage("Responding agent is " + name + ".", null);
//                    }
//                }
//            }

//            // Add formatting to the message
//            string html = message;
//            if (message.Length > 0)
//            {
//                if (message.Substring(0, 1) == "[")
//                {
//                    int index = message.IndexOf("]");
//                    if (index != -1)
//                    {
//                        html = message.Substring(0, index + 1) + "</span></span>" + message.Substring(index + 1);
//                        html = "<span style=\"color:#800080;font-family:sans-serif;font-size:.8em\"><span>" + html;
//                    }
//                }
//                html = "<div>" + html;
//                html += "</div>";
//            }
//            Tuple<string, string> multipartMessage = new Tuple<string, string>(message, html);

//            // Relay the message from the agent to the end user.
//            //controller.MessageFromAgent(multipartMessage);
//            MessageFromAgent(multipartMessage, null);
//        }

//        /// <summary>
//        /// Event handler for a log message received from the XMPP session.
//        /// Pass this message to the Service Controller is debugging level is set to Verbose.
//        /// </summary>
//        /// <param name="message"> The text of the log message. </param>
//        private void XmppSessionLogMessageReceived(string message)
//        {
//            if (Debug.Verbose) LogMessage(message, null);
//        }

//        /// <summary>
//        /// Event handler registered with the XmppSession for errors.
//        /// </summary>
//        private void XmppSessionMessagingError()
//        {
//            LogMessage("Error sending XMPP message, closing session.", null);
//            SendErrorMessage();
//            XmppSessionClose(true);
//        }

//        /// <summary>
//        /// Passes the specified message to the XMPP session.
//        /// </summary>
//        /// <param name="message"> The text of the message. </param>
//        public void SendMessage(string message)
//        {
//            if (connected)
//            {
//                LogMessage("Sending message to agent/queue : " + message, null);
//                xmppSession.SendMessage(message);
//            }
//            else
//            {
//                SendStillWaitingMessage();
//            }
//        }

//        /// <summary>
//        /// Sends a generic error message to the user.
//        /// </summary>
//        private void SendErrorMessage()
//        {
//            PhraseFromSession("XmppConnectionError", null);
//        }

//        /// <summary>
//        /// Sends a messages to the user informing them a connection is being opened.
//        /// </summary>
//        private void SendConnectingMessage()
//        {
//            PhraseFromSession("XmppConnecting", null);
//        }

//        /// <summary>
//        /// Sends a message to the user indicating the connection is still being established.
//        /// </summary>
//        private void SendStillWaitingMessage()
//        {
//            PhraseFromSession("XmppStillWaiting", null);
//        }

//        /// <summary>
//        /// Sends a message to the user informing them the session has been closed.
//        /// </summary>
//        private void SendXmppSessionClosedMessage()
//        {
//            PhraseFromSession("XmppClosed", null);
//        }

//        /// <summary>
//        /// Determines if the message specified is an AIC bridge message.
//        /// </summary>
//        /// <param name="message"> The message to be matched. </param>
//        /// <returns> A Tuple containing the Id of the bridge message, and its general type. </returns>
//        private Tuple<BridgeMessage, BridgeMessageType> CheckForBridgeMessage(string message)
//        {
//            // Eliminate variables from messages in order to pattern match them.

//            // Remove numbers.
//            message = Regex.Replace(message, @"[\d-]", string.Empty);

//            // Remove text between quotation marks.
//            message = Regex.Replace(message, "\".*\"", string.Empty);

//            Tuple<BridgeMessage, BridgeMessageType> returnValue;
//            switch (message)
//            {
//                // Closed
//                case "Connection has been closed.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED, BridgeMessageType.Closed);
//                    break;
//                case "Connection has been closed due to inactivity timeout.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED_TIMEOUT, BridgeMessageType.Closed);
//                    break;
//                case "Connection has been closed due to a timeout.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED_GLOBAL_TIMEOUT, BridgeMessageType.Closed);
//                    break;
//                case "Connection has been closed due to an internal error. We apologize.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED_INTERNAL_ERROR, BridgeMessageType.Closed);
//                    break;
//                case "Connection has been interrupted due to loss of network connectivity, please wait while you are reconnected.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED_NO_AGENTS, BridgeMessageType.Closed);
//                    break;
//                case "Connection has been terminated. We are sorry, this service is unavailable.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_CLOSED_SERVICE_STOPPED, BridgeMessageType.Closed);
//                    break;

//                // Waiting
//                case "The system is looking for an agent. Please wait a moment.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.WAITING_FOR_AGENT, BridgeMessageType.Waiting);
//                    break;
//                case "Your message cannot be send to an agent until the connection is established. Please wait a moment.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.WAIT_TO_SEND_MESSAGES, BridgeMessageType.Waiting);
//                    break;

//                // Established
//                case "Session Established. Welcome.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.CONNECTION_ESTABLISHED, BridgeMessageType.Established);
//                    break;

//                // Timeout
//                case "Your session will be closed if no message is received in  seconds":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.SESSION_TIMEOUT_WARNING_CUSTOMER, BridgeMessageType.TimeoutWarning);
//                    break;
//                case "[System] The session will be closed if no message is received from customer in  seconds":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.SESSION_TIMEOUT_WARNING_AGENT, BridgeMessageType.TimeoutWarning);
//                    break;

//                // Error
//                case "This operation is not supported":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.OPERATION_NOT_SUPPORTED, BridgeMessageType.Error);
//                    break;
//                case "We are sorry for the inconvenience, this queue will shortly be moved out of service, your chat will not be started.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.ACCOUNT_DISABLED, BridgeMessageType.Error);
//                    break;
//                case "The message \"\" could not be sent.":
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.ERROR_SENDING_MESSAGE, BridgeMessageType.Error);
//                    break;

//                default:
//                    returnValue = new Tuple<BridgeMessage, BridgeMessageType>(BridgeMessage.NONE, BridgeMessageType.None);
//                    break;
//            }
//            return returnValue;

//            // Bridge Messages

//            ////CONNECTION_CLOSED=Connection has been closed.[#CC1]
//            ////CONNECTION_CLOSED_TIMEOUT=Connection has been closed due to inactivity timeout.[#CC2]
//            ////CONNECTION_CLOSED_GLOBAL_TIMEOUT=Connection has been closed due to a timeout.[#CC3]
//            ////CONNECTION_CLOSED_INTERNAL_ERROR=Connection has been closed due to an internal error. We apologize.[#CC4]
//            //CONNECTION_CLOSED_NO_AGENTS=Connection has been interrupted due to loss of network connectivity, please wait while you are reconnected.[#CC5]
//            //CONNECTION_CLOSED_SERVICE_STOPPED=Connection has been terminated. We are sorry, this service is unavailable.[#CC6]

//            //WAITING_FOR_AGENT=The system is looking for an agent. Please wait a moment.[#WA1]
//            //WAIT_TO_SEND_MESSAGES=Your message cannot be send to an agent until the connection is established. Please wait a moment.[#WS1]
//            //CONNECTION_ESTABLISHED=Session Established. Welcome.[#CE1]

//            //SESSION_TIMEOUT_WARNING_CUSTOMER=Your session will be closed if no message is received in {0} seconds[#TO1]
//            //SESSION_TIMEOUT_WARNING_AGENT=[System] The session will be closed if no message is received from customer in {0} seconds[#TO2]

//            //ERROR_SENDING_MESSAGE=The message "{0}" could not be sent.[#ER1]
//            //OPERATION_NOT_SUPPORTED=This operation is not supported[#ER2]
//            //ACCOUNT_DISABLED=We are sorry for the inconvenience, this queue will shortly be moved out of service, your chat will not be started.[#CC1]
//        }

//        #endregion Private Methods
//    }
//}
