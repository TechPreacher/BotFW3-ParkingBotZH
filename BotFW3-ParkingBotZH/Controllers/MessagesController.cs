using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BotFW3_ParkingBotZH
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity _activity)
        {
            ParkleitsystemZH _plZH = new ParkleitsystemZH();
            bool _setParking = false;

            string _response = "Ich kann Sie leider nicht verstehen.";
            
            if (_activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(_activity.ServiceUrl));
                List<Attachment> _attachments = new List<Attachment>();

                // Get the stateClient to get/set Bot Data
                StateClient _stateClient = _activity.GetStateClient();
                BotData _botData = _stateClient.BotState.GetUserData(_activity.ChannelId, _activity.Conversation.Id);

                Luis _luis = await LuisClient.ParseUserInput(_activity.Text);

                if (_luis.intents.Count() > 0)
                {

                    string _userIntent = _luis.intents[0].intent;
                    string _userParking = "";

                    if (_luis.entities.Count() > 0)
                        _userParking = _luis.entities[0].entity;

                    switch (_userIntent)
                    {
                        case "hi":
                            _response = "Hallo!  Sie können mich um eine Liste von Parkhäusern in Zürich fragen und nach Anzahl freier Parkplätze in einem Parkhaus.";
                            break;

                        case "list":
                            _response = await _plZH.ListParkings();
                            break;

                        case "top5":
                            _response = await _plZH.GetTop5Parkings();
                            break;

                        case "again":
                            if (null == _botData.GetProperty<LastParking>("LastParking"))
                            {
                                _response = "Sie haben mich noch nicht nach einem Parkhaus gefragt, das ich nachschauen könnte!";
                            }
                            else
                            {
                                _response = await _plZH.GetParking(_botData.GetProperty<LastParking>("LastParking").Name);
                            }
                            break;

                        case "find":
                            _setParking = true;
                            _response = await _plZH.GetParking(_userParking);
                            break;

                        default:
                            break;
                    }

                    if (_setParking)
                    {
                        LastParking _lastParking = new LastParking();
                        _lastParking.Name = _userParking;
                        _botData.SetProperty<LastParking>("LastParking", _lastParking);
                        _stateClient.BotState.SetUserData(_activity.ChannelId, _activity.Conversation.Id, _botData);
                    }
                }

                // return our reply to the user
                Activity _reply = _activity.CreateReply(_response);
                if (_plZH.Attachments.Count > 0)
                {
                    _reply.Attachments = new List<Attachment>();
                    if (_plZH.Attachments.Count > 1)
                    {
                        _reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    }
                    foreach (Attachment _a in _plZH.Attachments)
                    {
                        _reply.Attachments.Add(_a);
                    }
                }
                await connector.Conversations.ReplyToActivityAsync(_reply);
                
            }
            else
            {
                HandleSystemMessage(_activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}