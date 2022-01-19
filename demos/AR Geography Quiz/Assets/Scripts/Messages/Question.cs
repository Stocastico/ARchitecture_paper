using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Question : Message
        {
            public string content;

            public Question(string json) : base(json) { }

            public Question(string sender, string content) : 
                base(typeof(Question).Name, sender)
            {
                this.content = content;
            }

            public override string FriendlyName()
            {
                return typeof(Question).Name;
            }
        }
    }
}
