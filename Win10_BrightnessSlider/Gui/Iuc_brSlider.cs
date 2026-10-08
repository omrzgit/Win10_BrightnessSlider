namespace Win10_BrightnessSlider
{
    public interface Iuc_brSlider
    {
        int Height { get;  }
        string NotifyIconText { get; }

        void UpdateSliderControl();


        RichInfoScreen richInfoScreen { get; set; }
        void Set_MonitorName(string name );

        int CurrentValue { get; }
        void SetSliderValue(int value, bool isMouseDown);
        event System.Action<Iuc_brSlider, int, bool> SliderValueChanged;
    }
}