using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Map
{
    public enum RoomStates
    {
        Locked,
        Visited,
        Attainable
    }

    public class RoomView : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("If using World Space")]
        public SpriteRenderer sr;
        public SpriteRenderer visitedCircle;

        [Header("If using UI Canvas")]
        public Image image;
        public Image circleImage;
        public Image visitedCircleImage;


        public Room room { get; private set; }
        public RoomNode roomNode { get; private set; }


        private float initialScale;
        private const float HoverScaleFactor = 1.2f;
        private float mouseDownTime;
        private const float MaxClickDuration = 0.5f;

        public void SetUp(Room r, RoomNode rn)
        {
            room = r;
            roomNode = rn;

            //2D world
            if (sr != null) sr.sprite = roomNode.sprite;
            if (sr != null) initialScale = sr.transform.localScale.x;

            //Canvas UI
            if (image != null) image.sprite = roomNode.sprite;
            if (image != null) initialScale = image.transform.localScale.x;

            //Scale boss room
            if (room.roomType == RoomType.Boss) transform.localScale *= 1.5f;

            //2D world
            if (visitedCircle != null)
            {
                visitedCircle.color = MapView.Instance.visitedColor;
                visitedCircle.gameObject.SetActive(false);
            }

            //Canvas UI
            if (circleImage != null)
            {
                circleImage.color = MapView.Instance.visitedColor;
                circleImage.gameObject.SetActive(false);
            }

            SetState(RoomStates.Locked);
        }

        public void SetState(RoomStates state)
        {
            //2D world
            if (visitedCircle != null) visitedCircle.gameObject.SetActive(false);
            //Canvas UI
            if (circleImage != null) circleImage.gameObject.SetActive(false);

            switch (state)
            {
                case RoomStates.Locked:
                    //2D world
                    if (sr != null)
                    {
                        sr.DOKill(); // Dừng mọi animation DOTween đang chạy
                        sr.color = MapView.Instance.lockedColor;
                    }
                    //Canvas UI
                    if (image != null)
                    {
                        image.DOKill();
                        image.color = MapView.Instance.lockedColor;
                    }

                    break;
                case RoomStates.Visited:
                    //2D world
                    if (sr != null)
                    {
                        sr.DOKill();
                        sr.color = MapView.Instance.visitedColor;
                    }
                    //Canvas UI
                    if (image != null)
                    {
                        image.DOKill();
                        image.color = MapView.Instance.visitedColor;
                    }
                    //2D world
                    if (visitedCircle != null) visitedCircle.gameObject.SetActive(true);
                    //Canvas UI
                    if (circleImage != null) circleImage.gameObject.SetActive(true);
                    break;
                case RoomStates.Attainable:
                    //2D world
                    // start pulsating from visited to locked color:
                    if (sr != null)
                    {
                        //Flashing effect
                        sr.color = MapView.Instance.lockedColor;
                        sr.DOKill();
                        sr.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                    //Canvas UI
                    if (image != null)
                    {
                        image.color = MapView.Instance.lockedColor;
                        image.DOKill();
                        image.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        //On hover effect
        public void OnPointerEnter(PointerEventData data)
        {
            if (sr != null)
            {
                sr.transform.DOKill();
                sr.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }

            if (image != null)
            {
                image.transform.DOKill();
                image.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }
        }

        //Exit hover effect
        public void OnPointerExit(PointerEventData data)
        {
            if (sr != null)
            {
                sr.transform.DOKill();
                sr.transform.DOScale(initialScale, 0.3f);
            }

            if (image != null)
            {
                image.transform.DOKill();
                image.transform.DOScale(initialScale, 0.3f);
            }
        }

        //Record the time the mouse is pressed down
        public void OnPointerDown(PointerEventData data)
        {
            mouseDownTime = Time.time;
        }

        //Avoids confusion between drag and click.
        public void OnPointerUp(PointerEventData data)
        {
            if (Time.time - mouseDownTime < MaxClickDuration)
            {
                // user clicked on this node:
                MapPlayerTracker.Instance.SelectNode(this);
            }
        }

        //Swirl effect on Canvas UI
        public void ShowSwirlAnimation()
        {
            if (visitedCircleImage == null)
                return;

            const float fillDuration = 0.3f;
            visitedCircleImage.fillAmount = 0;

            DOTween.To(() => visitedCircleImage.fillAmount, x => visitedCircleImage.fillAmount = x, 1f, fillDuration);
        }

        //Clean up DOTween
        private void OnDestroy()
        {
            if (image != null)
            {
                image.transform.DOKill();
                image.DOKill();
            }

            if (sr != null)
            {
                sr.transform.DOKill();
                sr.DOKill();
            }
        }
    }
}