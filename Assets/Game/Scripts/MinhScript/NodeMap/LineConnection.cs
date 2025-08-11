using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Map
{
    [System.Serializable]
    public class LineConnection
    {
        public LineRenderer lr;
        public UILineRenderer uilr;
        public RoomView from;
        public RoomView to;

        public LineConnection(LineRenderer lr, UILineRenderer uilr, RoomView from, RoomView to)
        {
            this.lr = lr;
            this.uilr = uilr;
            this.from = from;
            this.to = to;
        }

        public void SetColor(Color color)
        {
            if (lr != null)
            {
                UnityEngine.Gradient gradient = lr.colorGradient;
                GradientColorKey[] colorKeys = gradient.colorKeys;
                for (int j = 0; j < colorKeys.Length; j++)
                {
                    colorKeys[j].color = color;
                }

                gradient.colorKeys = colorKeys;
                lr.colorGradient = gradient;
            }

            if (uilr != null) uilr.color = color;
        }
    }
}