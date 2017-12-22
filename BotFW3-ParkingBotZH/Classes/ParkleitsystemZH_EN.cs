using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace BotFW3_ParkingBotZH
{
    class ParkleitsystemZH
    {
        private ParkleitsystemZHData _data;

        public List<Attachment> Attachments = new List<Attachment>();
        public string ResultChat = "I'm sorry, I can't understand you.";
        public string ResultSpeak = "I'm sorry, I can't understand you.";

        public void SayHi()
        {
            ResultChat = ResultSpeak = "Hello! You can ask me for a list of parkings in Zurich and for the number of available parking spaces in a parking.";
        }

        public void GetParking(string _name)
        {
            if (_data == null)
            {
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            Parking _parking = _data.FindParking(_name);

            if (_parking.Name.ToLower() == "unknown")
            {
                ResultChat = "I'm sorry but I don't know this parking. Here is a list of parkings that I know of in Zurich: \n\n " + _data.ListParking();
                ResultSpeak = "I'm sorry but I don't know this parking.";
            }
            else
            {
                if (_parking.Status.ToLower() != "open")
                {
                    ResultChat = ResultSpeak = _parking.Name + " is currently closed.";
                }
                else
                {
                    ResultChat = "Here is the information on " + _parking.Name;
                    this.Attachments.Add(_data.GetHeroCard(_parking).ToAttachment());

                    ResultSpeak = _parking.Name + " is currently " + _parking.Status + " and has " + _parking.Round(_parking.Available) + " available spaces.";
                }
            }
        }

        public void GetTop5Parkings()
        {
            if (_data == null)
            {
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            List<Parking> _parkings = _data.FindTop5Parkings();

            ResultChat = "Here are the 5 parkings in Zurich with the most available spaces: ";
            foreach (Parking _p in _parkings)
                this.Attachments.Add(_data.GetThumbnailCard(_p).ToAttachment());

            ResultSpeak = "Here are the 5 parkings in Zurich with the most available spaces: ";
            foreach (Parking _p in _parkings)
                ResultSpeak += _p.Name + " with " + _p.Round(_p.Available) + " spaces. ";
        }

        public void ListParkings()
        {
            if (_data == null)
            { 
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            ResultChat = ResultSpeak = "The available parkings in Zurich are: \n\n" + _data.ListParking();
        }
    }

    class ParkleitsystemZHData
    {
        List<Parking> _parkingList = new List<Parking>();

        public void LoadParking()
        {
            XmlDocument _rssXmlDoc = new XmlDocument();

            // Load the RSS file from the RSS URL
            _rssXmlDoc.Load("http://www.pls-zh.ch/plsFeed/rss");

            // Parse the Items in the RSS file
            XmlNodeList _rssNodes = _rssXmlDoc.SelectNodes("rss/channel/item");

            // Iterate through the items in the RSS file
            foreach (XmlNode _rssNode in _rssNodes)
            {
                XmlNode _rssSubNode = _rssNode.SelectSingleNode("title");
                string _title = _rssSubNode != null ? _rssSubNode.InnerText : "";

                _rssSubNode = _rssNode.SelectSingleNode("link");
                string _link = _rssSubNode != null ? _rssSubNode.InnerText : "";

                string _image = "http://www.plszh.ch/images/parkhaus/img_" +
                    _link.Substring(_link.IndexOf("pid=")+4) +".jpg";

                _rssSubNode = _rssNode.SelectSingleNode("description");
                string _description = _rssSubNode != null ? _rssSubNode.InnerText : "";

                _parkingList.Add(new Parking(
                    _link,
                    _image,
                    _title.Substring(0, _title.IndexOf(" / ", 0)).Trim(),
                    _description.Substring(0, _description.IndexOf(" / ", 0)).Trim(),
                    _description.Substring(_description.IndexOf(" / ", 0) + 3).Trim())
                );
            }
        }

        public Parking FindParking(string _name)
        {
            Parking _pResult = new Parking("http://www.pls-zh.ch", "http://pls-zh.ch/images/maps/stadtzuerich.gif", "unknown", "closed", "0");

            foreach (Parking _p in _parkingList)
            {
                if (_p.Name.ToLower().Contains(_name.ToLower()))
                    _pResult = _p;
            }

            return _pResult;
        }

        public List<Parking> FindTop5Parkings()
        {
            List<Parking> _parkingListSorted = new List<Parking>();
            int _count = 0;

            foreach (var _parking in _parkingList.OrderByDescending(o => o.Available, new SemiNumericComparer()))
            {
                if (_count < 5)
                    _parkingListSorted.Add(_parking);
                _count++;
            }

            return _parkingListSorted;
        }


        public string ListParking()
        {
            StringBuilder _list = new StringBuilder();
            foreach (Parking _p in _parkingList)
            {
                _list.Append(_p.Name + ", ");
            }

            _list.Remove(_list.Length - 2, 2).Append(".");
            return _list.ToString();
        }



        public HeroCard GetHeroCard (Parking _p)
        {
            List <CardImage> _cardImages = new List<CardImage>();
            _cardImages.Add(new CardImage(url: _p.Image));

            List <CardAction> _cardButtons = new List<CardAction>();
            CardAction _cardAction = new CardAction()
            {
                Value = _p.Url,
                Type = "openUrl",
                Title = "Show in browser"
            };
            _cardButtons.Add(_cardAction);

            HeroCard _heroCard = new HeroCard()
            {
                Title = _p.Name,
                Subtitle = "The parking is: " + _p.Status,
                Text = "Number of free spaces: " + _p.Available,
                Images = _cardImages,
                Buttons = _cardButtons
            };

            return _heroCard;
        }

        public ThumbnailCard GetThumbnailCard(Parking _p)
        {
            List<CardImage> _cardImages = new List<CardImage>();
            _cardImages.Add(new CardImage(url: _p.Image));

            List<CardAction> _cardButtons = new List<CardAction>();
            CardAction _cardAction = new CardAction()
            {
                Value = _p.Url,
                Type = "openUrl",
                Title = "Show in browser"
            };
            _cardButtons.Add(_cardAction);

            ThumbnailCard _thumbCard = new ThumbnailCard()
            {
                Title = _p.Name,
                Subtitle = "The parking is: " + _p.Status,
                Text = "Number of free spaces: " + _p.Available,
                Images = _cardImages,
                Buttons = _cardButtons
            };

            return _thumbCard;
        }
    }

    public class SemiNumericComparer : IComparer<string>
    {
        public int Compare(string _s1, string _s2)
        {
            if (IsNumeric(_s1) && IsNumeric(_s2))
            {
                if (Convert.ToInt32(_s1) > Convert.ToInt32(_s2)) return 1;
                if (Convert.ToInt32(_s1) < Convert.ToInt32(_s2)) return -1;
                if (Convert.ToInt32(_s1) == Convert.ToInt32(_s2)) return 0;
            }

            if (IsNumeric(_s1) && !IsNumeric(_s2))
                return 1;

            if (!IsNumeric(_s1) && IsNumeric(_s2))
                return -1;

            return string.Compare(_s1, _s2, true);
        }

        public static bool IsNumeric(object _value)
        {
            try
            {
                int _i = Convert.ToInt32(_value.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }

    class Parking
    {
        public string Url { get; set; }
        public string Image { get; set; } 
        public string Name { get; set; }
        public string Status { get; set; }
        public string Available { get; set; }

        public Parking(string _url, string _image, string _name, string _status, string _available)
        {
            Url = _url; Image = _image;  Name = _name; Status = _status; Available = _available;
        }

        public string Round(string _number)
        {
            string _result = "";
            int _num = 0;

            try { 
                _num = Convert.ToInt32(_number);
            }
            catch
            {
                //do nothing;
            }

            if (_num <= 10)
                _result = _num.ToString();

            if (10 < _num && _num <= 25)
                _result = "about 20";

            if (25 < _num && _num <= 50)
                _result = "about 50";

            if (50 < _num && _num <= 100)
                _result = "about 100";

            if (100 < _num && _num <= 200)
                _result = "about 200";

            if (200 < _num && _num <= 500)
                _result = "almost 500";

            if (500 < _num)
                _result = "more than 500";

            return _result;
        }
    }

    class LastParking
    {
        public string Name { get; set; }
    }
}