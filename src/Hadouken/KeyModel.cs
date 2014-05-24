using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hadouken
{
    public class KeyModel
    {

        public KeyModel(int key)
        {
            if (!Enum.IsDefined(typeof(Keys), (Keys)key))
                throw new ArgumentException("Invalid key.", "key");
            this.DisplayValue = Enum.GetName(typeof(Keys), key);
            this.KeyValue = (Keys)key;
        }

        public string DisplayValue { get; set; }
        public Keys KeyValue { get; set; }
    }
}
