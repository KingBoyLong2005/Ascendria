using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Map
{
    //For Canvas UI
    public class MapViewUI : MapView
    {
        protected override void AddLineConnection(RoomView from, RoomView to)
        {
            //UILineRenderer lineRenderer = Instantiate(uiLinePrefab, mapParent.transform);
            //lineRenderer.transform.SetAsFirstSibling();
            //RectTransform fromRT = from.transform as RectTransform;
            //RectTransform toRT = to.transform as RectTransform;
            //Vector2 fromPoint = fromRT.anchoredPosition +
            //                    (toRT.anchoredPosition - fromRT.anchoredPosition).normalized * offsetFromNodes;

            //Vector2 toPoint = toRT.anchoredPosition +
            //                  (fromRT.anchoredPosition - toRT.anchoredPosition).normalized * offsetFromNodes;

            //// drawing lines in local space:
            //lineRenderer.transform.position = from.transform.position +
            //                                  (Vector3)(toRT.anchoredPosition - fromRT.anchoredPosition).normalized *
            //                                  offsetFromNodes;

            //// line renderer with 2 points only does not handle transparency properly:
            //List<Vector2> list = new List<Vector2>();
            //for (int i = 0; i < linePointsCount; i++)
            //{
            //    list.Add(Vector3.Lerp(Vector3.zero, toPoint - fromPoint +
            //                                        2 * (fromRT.anchoredPosition - toRT.anchoredPosition).normalized *
            //                                        offsetFromNodes, (float)i / (linePointsCount - 1)));
            //}

            //Debug.Log("From: " + fromPoint + " to: " + toPoint + " last point: " + list[list.Count - 1]);

            //lineRenderer.Points = list.ToArray();

            //DottedLineRenderer dottedLine = lineRenderer.GetComponent<DottedLineRenderer>();
            //if (dottedLine != null) dottedLine.ScaleMaterial();

            //lineConnections.Add(new LineConnection(null, lineRenderer, from, to));
        }
    }
}