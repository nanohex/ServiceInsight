using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Profiler.Desktop.Saga
{
    public class SagaUpdatedValue
    {
        public string Name { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }

        public string Label
        {
            get
            {
                return string.Format("{0}{1}", Name, IsValueNew ? " (new)" : "");
            }
        }

        public bool IsValueChanged
        {
            get
            {
                return !string.IsNullOrEmpty(OldValue) && NewValue != OldValue && !IsValueLong;
            }
        }

        public bool IsValueNew
        {
            get
            {
                return string.IsNullOrEmpty(OldValue) && !IsValueLong;
            }
        }

        public bool IsValueNotUpdated
        {
            get
            {
                return !IsValueChanged && !IsValueNew && !IsValueLong;
            }
        }

        public bool IsValueLong
        {
            get
            {
                return NewValue.Length > 30 || NewValue.Contains('\n');
            }
        }
    }
}
