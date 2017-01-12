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

        public async Task<string> GetParking(string _name)
        {
            if (_data == null)
            {
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            string _result;

            Parking _parking = _data.FindParking(_name);

            if (_parking.Name.ToLower() == "unbekannt")
            {
                _result = "Ich kenne dieses Parkhaus leider nicht. Hier ist eine Liste von mir bekannten Parkhäusern in Zürich: \n\n " + _data.ListParking();
            }
            else
            {
                if (_parking.Status.ToLower() != "open")
                {
                    _result = _parking.Name + " ist zur Zeit geschlossen.";
                }
                else
                {
                    _result = "Hier sind Ihre Informationen zu " + _parking.Name;
                    this.Attachments.Add(_data.GetHeroCard(_parking).ToAttachment());
                }
            }

            return _result;
        }

        public async Task<string> GetTop5Parkings()
        {
            if (_data == null)
            {
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            string _result;

            List<Parking> _parkings = _data.FindTop5Parkings();

            _result = "Hier sind die 5 Parkhäuser in Zürich mit den meisten, freien Parkplätzen:";

            foreach (Parking _p in _parkings)
                this.Attachments.Add(_data.GetThumbnailCard(_p).ToAttachment());

            return _result;
        }

        public async Task<string> ListParkings()
        {
            if (_data == null)
            { 
                _data = new ParkleitsystemZHData();
                _data.LoadParking();
            }

            string _result = "Die verfügbaren Parkhäuser in Zürich sind: \n\n" + _data.ListParking();

            return _result;
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
            Parking _pResult = new Parking("http://www.pls-zh.ch", "http://pls-zh.ch/images/maps/stadtzuerich.gif", "Unbekannt", "geschlossen", "0");

            foreach (Parking _p in _parkingList)
            {
                if (_p.Name.ToLower().Contains(_name.ToLower()))
                    _pResult = _p;
            }

            return _pResult;
        }

        public List<Parking> FindTop5Parkings()
        {
            List<Parking> _parkingListSorted = _parkingList.OrderByDescending(o => o.Available).ToList();
            List<Parking> _parkingListTop5 = new List<Parking>();
            for (int _i = 0; _i < 5; _i++ )
            {
                _parkingListTop5.Add(_parkingListSorted[_i]);
            }

            return _parkingListTop5;


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
                Title = "Im Browser anzeigen"
            };
            _cardButtons.Add(_cardAction);

            HeroCard _heroCard = new HeroCard()
            {
                Title = _p.Name,
                Subtitle = "Das Parkhaus ist: " + _p.Status,
                Text = "Anzahl freie Parkplätze: " + _p.Available,
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
                Title = "Im Browser anzeigen"
            };
            _cardButtons.Add(_cardAction);

            ThumbnailCard _thumbCard = new ThumbnailCard()
            {
                Title = _p.Name,
                Subtitle = "Das Parkhaus ist: " + _p.Status,
                Text = "Anzahl freie Parkplätze: " + _p.Available,
                Images = _cardImages,
                Buttons = _cardButtons
            };

            return _thumbCard;
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
    }

    class LastParking
    {
        public string Name { get; set; }
    }
}