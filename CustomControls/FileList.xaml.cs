using System.Windows.Controls;

namespace ModelPacker.CustomControls
{
    public partial class FileList : UserControl
    {
        public string[] files
        {
            get
            {
                ItemCollection items = FilesList.Items;
                string[] retVal = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    retVal[i] = items[i].ToString();
                }

                return retVal;
            }
        }

        public FileList()
        {
            InitializeComponent();
        }
    }
}