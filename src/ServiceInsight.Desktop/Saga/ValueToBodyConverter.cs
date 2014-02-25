using Caliburn.PresentationFramework;
using Caliburn.PresentationFramework.Screens;
using Caliburn.PresentationFramework.Views;
using NServiceBus.Profiler.Desktop.MessageViewers.HexViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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

    public class MessageBodyShim
    {
        public bool JsonViewerVisibile
        {
            get
            {
                return false;
            }
        }

        public bool XmlViewerVisibile
        {
            get
            {
                return false;
            }
        }

        public IHexContentViewModel HexViewer { get; private set; }

        public MessageBodyShim(object value)
        {
            HexViewer = new HexContentShim(value.ToString());
        }
    }

    [View(typeof(HexContentView))]
    public class HexContentShim : Screen, IHexContentViewModel
    {
        public IObservableCollection<HexPart> HexParts { get; private set; }

        public HexContentShim(string p)
        {
            CreateHexParts(p);
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

            foreach (var currentByte in value)
            {
                if (!HexParts.Contains(hexLine))
                    HexParts.Add(hexLine);

                var hexChar = new HexNumber();

                AppendHex(hexChar, currentByte);
                AppendText(hexChar, currentByte);

                hexLine.Numbers.Add(hexChar);
                columnNumber++;

                if (columnNumber == 16)
                {
                    lineNumber++;
                    columnNumber = 0;
                    hexLine = new HexPart(lineNumber);
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
            hexValue.Hex = string.Format("{00:X000}", b);
        }

    }
}
