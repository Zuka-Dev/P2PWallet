﻿using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IGLSevice
    {
        Task<GeneralLedger> GetOrCreateGL(CreateGLDTO createGLDTO);
    }
}
