# FScrollPage

#### 项目介绍
一个用于Unity5.x及以上的滑动展示控件。

## 说明

> 由于Unity自带的Scroll View控件不能灵活的满足自己实际开发，因此想到自己制作一个滑动展示页面，且该控件不依赖Unity的其他UI控件，但该控件存在局限性，仅供参考

> 实际效果预览：
>![横向滑动展示栏][2]
> 
> ![纵向滑动展示栏][3]

## 结构设计
该控件滑动周期由三个部分组成 :

- 1 **滑动时** ： 随触摸点（或鼠标）滑动 
- 2 **退出滑动时** ： 滑动衰减 
- 3 **滑动结束时** ： 位置校正 

 滑动展示栏由一个控制器和多个滑块组成，使用时需要设置控制器的参数配置，内部的滑动item需要有FScrollItem组件或者继承自FScrollItem的组件，其中FScrollItem开放了点击和被选中事件，可以在两个接口中写自定义逻辑。

- 滑动控制器 -- FScrollPage
    ![FScrollPage][4]
- 滑动对象 -- FScrollItem
    ![FScrollItem][5]

## FScrollPage

滑动控制器FScrollPage 负责整理FScrollitem的位置和大小，用户触摸交互，监听滑动事件，执行滑动特效以及传递选中事件给子物体FScrollitem。
考虑到易用性，在FScrollPage的初始化分为两种，第一种是自动检查指定节点下的子对象进行标记id和排序整理m默认选中第一个被标记的item，第二种方式是传递一个map<itemID , item> 以及默认选中id。

## FScrollItem

滑动对象FScrollItem 提供了**点击**、**被选中**回调事件，当被点击时自动将该Item滑动到中心并向FScrollPage通知并选中到该Item。

## 位置整理

 - 统一锚点布局方式。
 - 按指定配置设置item的位置、大小、间距。

 将获取到的子对象map<itemID，item>进行遍历，生成对应的FScroillObject对象并设置统一的布局方式（居中）,初始化lastPostion和Postion **(FScrollObject除了有RectTransfrom属性外还有一个Postion属性和lastPostion属性，分别用来纪录当前（或正在前往）的位置和上一次位置信息)**。用于后续的滑动和位置校正。

## 滑动监听
滑动监听原理时通过检测有触摸或按下事件到滑动条上时激活滑动检测，当移动速度大于阈值时则判定当前正在滑动中，当检测到手指或鼠标离开滑动条时结束滑动监听。
这里有一点需要注意，如果触摸滑动条触摸到了item这会被拦截点击事件，滑动条的OnPointerDown事件不会被触发，解决办法我开始参考的是一篇渗透UI点击事件文章 : [渗透UI点击事件][6] ，但是使用后发现多个UI重叠时会出现栈内存溢出，导致编辑器卡死，后来想了一个更简单的办法，在FScrollItem中同样实现点击事件接口，当被按下或抬起时直接将事件继续传递给控制器的对应事件。

        public void OnPointerDown(PointerEventData eventData)
        {
            //传递事件
            scrollPage.OnPointerDown(eventData);
            //后续逻辑
            lastPointer = eventData.position;
        }

## 滑动效果以及位置校正
滑动时根据滑动速度来控制item的移动，滑动结束后使用速度线性递减，当速度低于一定阈值时执行位置校正，其中弹动的效果使用的时一个缓动函数BackEaseOut。

    static float BackEaseOut(float t, float b = 0, float c = 1, float d = 1)
    {
        return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
    }

位置校正时先查询距离中心点最近的item，记录并传递该item的ID到滑动方法中，然后通过滑动方法使Item移动到正常位置。

## 源码已知存在的缺陷

如果你需要使用源码你需要了解以下的缺陷，你可以对缺陷进行修改或者修复

- 滑动到边界时没有约束
- 锚点固定为居中，可能会与你的项目有冲突，也可能导致自适应UI出现问题
- 点击判定过于僵硬（按下坐标和抬起坐标的Distance小于指定阈值则触发点击）
- 滑动衰弱的时间和位置校正时间是固定的

  [1]: https://gitee.com/Foldcc/FScrollPage.git
  [2]: https://fold.oss-cn-shanghai.aliyuncs.com/BlogImg/FScrollPage1.gif
  [3]: https://fold.oss-cn-shanghai.aliyuncs.com/BlogImg/FScrollPage2.gif
  [4]: https://fold.oss-cn-shanghai.aliyuncs.com/BlogImg/FScrollPage.png
  [5]: https://fold.oss-cn-shanghai.aliyuncs.com/BlogImg/FScrollPage%203.png
  [6]: http://www.xuanyusong.com/archives/4241