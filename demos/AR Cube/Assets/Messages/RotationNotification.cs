using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        /// <summary>
        /// Message with the rotation info
        /// </summary>
        [Serializable]
        public class RotationNotification : Message
        {
            public Vector3 angle;
        
            public RotationNotification(string json) : base(json) { }

            public RotationNotification(string sender, Vector3 angle) :
              base(typeof(RotationNotification).Name, sender)
            {
                this.angle = angle;
              
            }

            public Vector3 GetContent()
            {
                return angle;
            }

            public override string FriendlyName()
            {
                return typeof(RotationNotification).Name;
            }
        }
    }
}
