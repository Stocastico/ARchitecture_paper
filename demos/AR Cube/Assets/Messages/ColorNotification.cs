using System;

namespace OrkestraLib
{
    namespace Message
    {
        /// <summary>
        /// Message with the color information
        /// </summary>
        [Serializable]
        public class ColorNotification  : Message
        {
            public string content;

            public ColorNotification(string json) : base(json) { }

            public ColorNotification(string sender, string content) :
              base(typeof(ColorNotification).Name, sender)
            {
                this.content = content;
            }

            public string GetContent()
            {
                return content;
            }

            public override string FriendlyName()
            {
                return typeof(ColorNotification).Name;
            }
        }
    }
}
