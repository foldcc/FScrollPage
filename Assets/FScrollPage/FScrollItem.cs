using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FScrollPage {

    public class FScrollItem : MonoBehaviour,IFScrollItem , IPointerUpHandler , IPointerDownHandler {

        private FScrollPage scrollPage;

        private int ItemID;

        private Vector2 lastPointer;

        public virtual void OnScrollClick()
        {
        }

        public virtual void OnScrollPitch()
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //传递事件
            scrollPage.OnPointerDown(eventData);
            //后续逻辑
            lastPointer = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            scrollPage.OnPointerUp(eventData);
            if (Vector2.Distance(lastPointer, eventData.position) <= 2f) {
                OnScrollClick();
                scrollPage.MoveToItemID(ItemID);
            }
        }

        public void OnScrollInit(FScrollPage page , int itemID)
        {
            scrollPage = page;
            ItemID = itemID;
        }
    }
}

