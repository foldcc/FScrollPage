using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FScrollPage
{

    interface IFScrollItem
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void OnScrollInit(FScrollPage page , int ItemID);
        ///被点击时
        void OnScrollClick();
        ///被选中时
        void OnScrollPitch();
    }
}

