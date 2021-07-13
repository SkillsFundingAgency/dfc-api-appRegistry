using System.ComponentModel;

namespace DFC.Api.AppRegistry.Enums
{
    public enum Layout
    {
        [Description("No layout")]
        None = 0,

        [Description("Uses a layout which is full width")]
        FullWidth = 1,

        [Description("Uses a layout where the sidebar is located on the right")]
        SidebarRight = 2,

        [Description("Uses a layout where the sidebar is located on the left")]
        SidebarLeft = 3,

        [Description("Uses a layout which is full width but has no main div tag")]
        FullWidthNoMain = 4,

        [Description("Uses a layout which is full browser width")]
        UseBrowserWidth = 5,
    }
}
