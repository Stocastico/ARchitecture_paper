using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Answer : Message
        {
            public string name;
            public float x;
            public float y;
            public float z;

            public Answer(string json) : base(json) { }

            public Answer(string sender, GameObject go, Vector3 hitPoint) : 
                base(typeof(Answer).Name, sender)
            {
                name = go.name;
                x = hitPoint.x;
                y = hitPoint.y;
                z = hitPoint.z;
            }

            public override string FriendlyName()
            {
                return typeof(Answer).Name;
            }

            public Vector3 Location()
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
