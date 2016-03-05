using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NewFuslog
{
    public enum Severity
    {
        Error = 0,
        Warning = 1
    };

    public class MessageViewModel : BaseViewModel
    {
        public MessageViewModel(string text, Severity severity)
        {
            this.Text = text;
            this.Severity = severity;
        }

        public string Text { get; }

        public Severity Severity { get; }
    }
}
