﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class CreateGLDTO
    {
        public string Purpose { get; set; } = string.Empty;
        public string GLCurrency { get; set; } = string.Empty;
    }
}
