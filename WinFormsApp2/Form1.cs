using System.Runtime.InteropServices;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Margins
        {
            public int Left, Right, Top, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        public Form1()
        {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.K))
            {
                txtBarcode.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (DesignMode)
                return;
            ApplyFrostedGlass();
        }

        private void ApplyFrostedGlass()
        {
            int dark = 1;
            DwmSetWindowAttribute(Handle, 20, ref dark, sizeof(int));
            int backdrop = 1;
            DwmSetWindowAttribute(Handle, 38, ref backdrop, sizeof(int));
            var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
            DwmExtendFrameIntoClientArea(Handle, ref margins);

            var accent = new AccentPolicy
            {
                AccentState = 4,
                GradientColor = unchecked((int)0x28101010)
            };
            int size = Marshal.SizeOf(accent);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(accent, ptr, false);
                var data = new WindowCompositionAttributeData { Attribute = 19, Data = ptr, SizeOfData = size };
                SetWindowCompositionAttribute(Handle, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
