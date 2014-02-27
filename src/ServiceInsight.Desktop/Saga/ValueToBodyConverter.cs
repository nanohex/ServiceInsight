using Caliburn.PresentationFramework;
using Caliburn.PresentationFramework.Screens;
using Caliburn.PresentationFramework.Views;
using Newtonsoft.Json;
using NServiceBus.Profiler.Desktop.MessageViewers;
using NServiceBus.Profiler.Desktop.MessageViewers.HexViewer;
using NServiceBus.Profiler.Desktop.MessageViewers.JsonViewer;
using NServiceBus.Profiler.Desktop.MessageViewers.XmlViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NServiceBus.Profiler.Desktop.Saga
{
    class ValueToBodyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new MessageBodyShim(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [View(typeof(MessageBodyView))]
    public class MessageBodyShim
    {
        string value;
        string jsonValue;
        string xmlValue;

        public bool JsonViewerVisibile
        {
            get
            {
                return jsonValue != null;
            }
        }

        public bool XmlViewerVisibile
        {
            get
            {
                return xmlValue != null;
            }
        }

        public IHexContentViewModel HexViewer { get; private set; }
        public IJsonMessageViewModel JsonViewer { get; private set; }
        public IXmlMessageViewModel XmlViewer { get; private set; }

        public MessageBodyShim(object value)
        {
            this.value = value.ToString();
            HexViewer = new HexContentShim(this.value);
            
            jsonValue = GetJson(this.value);
            if (jsonValue != null)
                JsonViewer = new JsonContentShim(jsonValue);
            
            xmlValue = GetXml(this.value);
            if (xmlValue != null)
                XmlViewer = new XmlContentShim(xmlValue);
        }

        private string GetXml(string value)
        {
            if (value.StartsWith("<?xml"))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        private string GetJson(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<object>(value).ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    [View(typeof(XmlMessageView))]
    public class XmlContentShim : Screen, IXmlMessageViewModel
    {
        private string value;

        public XmlContentShim(string xmlValue)
        {
            this.value = xmlValue;
        }
        public override void AttachView(object view, object context)
        {
            base.AttachView(view, context);
            ((IXmlMessageView)view).Display(value);
        }

        public Models.MessageBody SelectedMessage
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public void Handle(Events.SelectedMessageChanged message)
        {
        }


        public void CopyMessageXml()
        {
        }

        public bool CanCopyMessageXml()
        {
            return false;
        }
    }

    [View(typeof(JsonMessageView))]
    public class JsonContentShim : Screen, IJsonMessageViewModel
    {
        private string value;

        public JsonContentShim(string jsonValue)
        {
            this.value = jsonValue;
        }
        public override void AttachView(object view, object context)
        {
            base.AttachView(view, context);
            ((IJsonMessageView)view).Display(value);
        }

        public Models.MessageBody SelectedMessage
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public void Handle(Events.SelectedMessageChanged message)
        {
        }
    }


    [View(typeof(HexContentView))]
    public class HexContentShim : Screen, IHexContentViewModel
    {
        private IHexContentView _view;
        public IObservableCollection<HexPart> HexParts { get; private set; }

        public HexContentShim(string p)
        {
            CreateHexParts(p);
        }

        public Brush Background
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x26));
            }
        }

        public Brush Foreground
        {
            get
            {
                return Brushes.White;
            }
        }

        public override void AttachView(object view, object context)
        {
            base.AttachView(view, context);
            _view = (IHexContentView)view;
        }


        public byte[] SelectedMessage
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        public void Handle(Events.SelectedMessageChanged message)
        {
        }

        private void CreateHexParts(string value)
        {
            HexParts = new BindableCollection<HexPart>();
            var columnNumber = 0;
            var lineNumber = 1;
            var hexLine = new HexPart(lineNumber);
            HexParts.Add(hexLine);

            foreach (var currentByte in value)
            {
                var hexChar = new HexNumber();

                AppendHex(hexChar, currentByte);
                AppendText(hexChar, currentByte);

                hexLine.Numbers.Add(hexChar);
                columnNumber++;

                if (columnNumber == 8)
                {
                    lineNumber++;
                    columnNumber = 0;
                    hexLine = new HexPart(lineNumber);
                    HexParts.Add(hexLine);
                }
            }
        }

        private static void AppendText(HexNumber number, char b)
        {
            if (char.IsControl(b))
            {
                number.Text = ".";
            }
            else
            {
                number.Text = new string(b, 1);
            }
        }

        private static void AppendHex(HexNumber hexValue, char b)
        {
            hexValue.Hex = string.Format("{0:X2}", (int)b);
        }

    }
}
