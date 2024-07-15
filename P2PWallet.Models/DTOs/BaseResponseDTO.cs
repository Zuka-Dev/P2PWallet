using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class BaseResponseDTO
    {
  
            public bool Status { get; set; }

            public string StatusMessage { get; set; }

            public object Data { get; set; }

        
    }
}
