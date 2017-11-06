using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [Serializable]
    public class Message
    {
        public Guid Id { get; set; }

        public DateTime Moment { get; set; }

        public string Text { get; set; }

        public int TextLength
        {
            get
            {
                return this.Text.Length;
            }
        }
    }
}
