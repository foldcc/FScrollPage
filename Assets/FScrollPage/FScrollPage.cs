using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FScrollPage {

    public enum FSlideType {
        Horizontal,
        Vertical
    }

    public class FScollObject {
        public RectTransform rectTransform;
        public int countID;
        public Vector2 postion;
        public Vector2 lastpostion;

        public FScollObject(RectTransform rt , int countid , Vector2 pos , Vector2 lpos)
        {
            rectTransform = rt;
            countID = countid;
            postion = pos;
            lastpostion = lpos;
        }
    }

    public class FScrollPage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler , IPointerExitHandler
    {

        [Header("自动初始化")]
        public bool IsAutoInit = true;

        [Header("滑动父对象")]
        public RectTransform Content;
        [Header("滑块边距")]
        public Vector2 Margin;
        [Header("滑块大小")]
        public Vector2 ScrollSize;
        [Header("滑动方向")]
        public FSlideType SlideType;
        [Header("滑动系数") ,Range(0 , 1.0f)]
        public float SlideCoefficient = 0;
        [Header("选中对象相对大小比例"), Range(-0.5f, 0.5f)]
        public float PicthScale = 0;

        private List<FScollObject> ScollList;

        private bool isScroll;
        //滑动方向
        private Vector2 moveVector;

        private Vector2 mouseVector;

        private int NowItemID;

        private void Start()
        {
            if(IsAutoInit)
                Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="defaultID">默认选中id</param>
        /// <param name="scrollList">自定义[id,item]</param>
        public void Init<T>(int defaultID = 0 , Dictionary<string , T> scrollList = null) where T : FScrollItem{
            
            if (scrollList != null)
            {
                ScollList = new List<FScollObject>();
                Content.pivot = Content.anchorMax = Content.anchorMin = new Vector2(0.5f, 0.5f);
                Content.anchoredPosition = new Vector3(0, 0, 0);
                int count = 0;

                foreach (var item2 in scrollList.Keys)
                {
                    RectTransform rt = scrollList[item2].GetComponent<RectTransform>();
                    IFScrollItem item = rt.GetComponent<IFScrollItem>();
                    int id = int.Parse(item2);

                    if (item != null)
                    {
                        item.OnScrollInit(this, id);
                    }

                    if (rt != null)
                    {
                        rt.pivot = rt.anchorMax = rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.sizeDelta = ScrollSize;
                        if (SlideType == FSlideType.Horizontal)
                        {
                            rt.anchoredPosition = new Vector2(count * (ScrollSize.x + Margin.x), 0);
                        }
                        else
                        {
                            rt.anchoredPosition = new Vector2(0, -count * (ScrollSize.y + Margin.y));
                        }
                        FScollObject fs = new FScollObject(rt, id, rt.anchoredPosition, rt.anchoredPosition);
                        sizeUpdata(fs);
                        ScollList.Add(fs);
                    }
                    count++;
                }

                if (ScollList.Count > 0)
                {
                    NowItemID = -1;
                    MoveToItemID(defaultID);
                }
            }
            else {
                Init(defaultID);
            }
            
            
        }


        public void Init(int defultID = 0) { 
            ScollList = new List<FScollObject>();
            Content.pivot = Content.anchorMax = Content.anchorMin = new Vector2(0.5f, 0.5f);
            Content.anchoredPosition = new Vector3(0, 0, 0);
            for (int i = 0; i < Content.childCount; i++)
            {
                RectTransform rt = Content.GetChild(i).GetComponent<RectTransform>();
                IFScrollItem item = rt.GetComponent<IFScrollItem>();
                if (item != null)
                {
                    item.OnScrollInit(this, i);
                }

                if (rt != null)
                {
                    rt.pivot = rt.anchorMax = rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = ScrollSize;
                    if (SlideType == FSlideType.Horizontal)
                    {
                        rt.anchoredPosition = new Vector2(i * (ScrollSize.x + Margin.x), 0);
                    }
                    else
                    {
                        rt.anchoredPosition = new Vector2(0, -i * (ScrollSize.y + Margin.y));
                    }
                    FScollObject fs = new FScollObject(rt, i, rt.anchoredPosition, rt.anchoredPosition);
                    sizeUpdata(fs);
                    ScollList.Add(fs);
                }
            }
            if (ScollList.Count > 0)
            {
                NowItemID = -1;
                MoveToItemID(defultID);
            }
        }

        private void Update()
        {
            if (isScroll)
            {
#if UNITY_EDITOR
                MouseMove();
#elif UNITY_IOS || UNITY_ANDROID
                if(Input.touchCount > 0)
                    moveVector = Input.GetTouch(0).deltaPosition;
#endif

                if (SlideType == FSlideType.Horizontal)
                {
                    moveVector.y = 0;
                }
                else
                {
                    moveVector.x = 0;
                }
                MoveScrollItem(moveVector);
            }
        }

        void MouseMove() {
            if (mouseVector == Vector2.zero)
            {
                mouseVector = Input.mousePosition;
            }
            else {
                Vector2 nowmousePos = Input.mousePosition;
                moveVector = nowmousePos - mouseVector;
                mouseVector = nowmousePos;
            }
        }

        void MoveScrollItem(Vector2 Delta) {
            if (Delta == Vector2.zero) {
                return;
            }
            if (Delta.x > 60)
                Delta = new Vector2(60, Delta.y);
            if(Delta.x < -60)
                Delta = new Vector2(-60, Delta.y);
            if (Delta.y < -60)
                Delta = new Vector2(Delta.x, -60);
            if (Delta.y > 60)
                Delta = new Vector2(Delta.x, 60);

            ScollList.ForEach(item => {
                item.rectTransform.anchoredPosition += Delta * (1- SlideCoefficient + 0.1f) * 2 + Time.deltaTime*Delta;
                sizeUpdata(item);
            });
        }

        void PostionScrollItem(Vector2 Delta)
        {
            ScollList.ForEach(item => {
                item.rectTransform.anchoredPosition = item.lastpostion + Delta;
                sizeUpdata(item);
            });
        }

        void UpdataLastPostion() {
            for (int i = 0; i < ScollList.Count; i++)
            {
                ScollList[i].lastpostion = ScollList[i].rectTransform.anchoredPosition;
                ScollList[i].postion = ScollList[i].rectTransform.anchoredPosition;
            }
        }

        IEnumerator moveBack() {
            float timenow = 0;
            while (Vector2.Distance(moveVector , Vector2.zero) >= 0.5f && timenow < 0.35f) {
                Vector2.Lerp(mouseVector , Vector2.zero , 0.15f + 0.15f * SlideCoefficient);
                MoveScrollItem(moveVector);
                moveVector = Vector2.Lerp(moveVector, Vector2.zero, 0.1f + 0.15f * SlideCoefficient);
                timenow += Time.deltaTime;
                yield return 0;
            }
            
            float length = Vector2.Distance(ScollList[0].rectTransform.anchoredPosition, Vector2.zero);
            int countID = ScollList[0].countID;
            ScollList.ForEach(item =>
            {
                float l = Vector2.Distance(item.rectTransform.anchoredPosition, Vector2.zero);
                if (l < length) {
                    length = l;
                    countID = item.countID;
                }
            });
            UpdataLastPostion();
            MoveToItemID(countID);

        }

        public void MoveToItemID(int itemCount) {
            StopCoroutine("moveToItem");
            StartCoroutine(moveToItem(itemCount));
        }

        IEnumerator moveToItem(int itemID) {
            int count = 0;
            int minCount = 0;
            ScollList.ForEach(item =>
            {
                if (item.countID == itemID)
                {
                    minCount = count;
                }
                count++;
            });
            Vector2 _moveVector = Vector2.zero - ScollList[minCount].lastpostion;

            ScollList.ForEach(item =>
            {
                item.postion += _moveVector;
            });

            float timeDetal = 0.35f;

            float nowTime = 0;

            

            while (nowTime <= timeDetal)
            {
                nowTime += Time.deltaTime;

                float scale = getProgress(nowTime / timeDetal);

                ScollList.ForEach(item =>
                {
                    item.rectTransform.anchoredPosition = item.lastpostion + _moveVector * scale;
                    sizeUpdata(item);
                });

                yield return 0;
            }
            
            ScollList.ForEach(item =>
            {
                item.lastpostion = item.postion;
                if (item.countID != NowItemID && item.countID == itemID && item.rectTransform.gameObject.GetComponent<IFScrollItem>() != null) {
                    item.rectTransform.gameObject.GetComponent<IFScrollItem>().OnScrollPitch();
                }
            });
            NowItemID = itemID;
            yield return 0;
        }

        /// <summary>
        /// 缓动函数执行器 
        /// </summary>
        /// <param name="timeScale">[0 , 1] 时间比例 </param>
        /// <returns></returns>
        float getProgress(float timeScale)
        {
            if (timeScale < 0)
                timeScale = 0;
            else if (timeScale > 1)
                timeScale = 1;
            return BackEaseOut(timeScale);
        }

        /// <summary>
        /// 缓动函数
        /// </summary>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        static float BackEaseOut(float t, float b = 0, float c = 1, float d = 1)
        {
            return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
        }

        /// <summary>
        /// 控制item在不同位置的大小
        /// </summary>
        void sizeUpdata(FScollObject fobject) {
            float fz = Vector2.Distance(fobject.rectTransform.anchoredPosition, Vector2.zero);
            float sizeOffset = 1;
            if (SlideType == FSlideType.Horizontal)
            {
                sizeOffset = fz / ((ScrollSize.x + Margin.x) * 1.0f);
            }
            else
            {
                sizeOffset = fz / ((ScrollSize.y + Margin.y) * 1.0f);
            }

            if (sizeOffset > 1)
            {
                sizeOffset = 1;
            }
            fobject.rectTransform.localScale = Vector2.one * (PicthScale * (1 - sizeOffset) + 1);
        }

        void OnScrollUp() {
            isScroll = false;
            mouseVector = Vector2.zero;
            StartCoroutine(moveBack());
        }

        void OnScrollDown() {
            isScroll = true;
            StopAllCoroutines();
            moveVector = Vector2.zero;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
           
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isScroll) {
                OnScrollDown();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isScroll)
            {
                OnScrollUp();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isScroll) {
                OnScrollUp();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("<b>进入滑动页面</b>");
        }
    }
}
