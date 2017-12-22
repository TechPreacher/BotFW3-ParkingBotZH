using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace BotFW3_ParkingBotZH.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            bool _setParking = false;
            var _activity = await result as Activity;

            ParkleitsystemZH _plZH = new ParkleitsystemZH();
            List<Attachment> _attachments = new List<Attachment>();

            LastParking _lastParking;

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
                        _plZH.SayHi();
                        break;

                    case "list":
                        _plZH.ListParkings();
                        break;

                    case "top5":
                        _plZH.GetTop5Parkings();
                        break;

                    case "again":
                        if (context.UserData.TryGetValue<LastParking>("LastParking", out _lastParking))
                        {
                            _plZH.GetParking(_lastParking.Name);
                        }
                        else
                        {
                            _plZH.ResultChat = _plZH.ResultSpeak = "I don't know which parking you are looking for.";
                        }
                        break;

                    case "find":
                        _plZH.GetParking(_userParking);
                        _setParking = true;
                        break;

                    default:
                        break;
                }

                if (_setParking)
                {
                    LastParking _newParking = new LastParking { Name = _userParking };
                    context.UserData.SetValue<LastParking>("LastParking", _newParking);
                }

                Activity _reply = _activity.CreateReply(_plZH.ResultChat);
                _reply.Speak = _plZH.ResultSpeak;
                _reply.InputHint = InputHints.AcceptingInput;

                if (_plZH.Attachments.Count > 0)
                {
                    _reply.Attachments = new List<Attachment>();
                    if (_plZH.Attachments.Count > 1)
                        _reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    foreach (Attachment _a in _plZH.Attachments)
                        _reply.Attachments.Add(_a);
                }

                await context.PostAsync(_reply);
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}