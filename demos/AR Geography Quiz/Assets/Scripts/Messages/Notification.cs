using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Notification : Message
        {
            public string content;

            public Notification(string json) : base(json) { }

            public Notification(string sender, string content):
              base(typeof(Notification).Name, sender)
            {
                this.content = content;
            }

            public string GetContent()
            {
                return content;
            }
     
            public override string FriendlyName()
            {
                return typeof(Notification).Name;
            }
        }
    }
}
