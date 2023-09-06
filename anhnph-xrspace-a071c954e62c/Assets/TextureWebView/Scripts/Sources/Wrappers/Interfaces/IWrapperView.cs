using System;

namespace TWV.Wrappers
{
    interface IWrapperView
    {
        string LoadUrl { set; }
        string LoadData { set; }
        string PageUrl { get; }
        float DisplayDensity { get; }
        bool ShowKeyboard { set; }

        WebStates State { get; }
        object StateValue { get; }

        bool StartRender();
        void StopRender();
        void Release();

        bool MoveForward();
        bool MoveBack();
        void ChangeSize(int width, int height);
        void SetInputText(string text);
        void SetMotionEvent(MotionActions action, float x, float y);
    }
}
