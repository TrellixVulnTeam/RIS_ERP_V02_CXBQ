﻿ 
using Auth.Model.PIMS.Model;
using Auth.Repository.PIMS;
using Auth.Utility.Attendance.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Auth.Controllers.PIMS
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeLeaveLedgerController : ControllerBase
    {
        //Intialize
        #region Constructor
        private IEmployeeLeaveLedgerRepository _repository;

        public EmployeeLeaveLedgerController(IEmployeeLeaveLedgerRepository repository)
        {
            _repository = repository;
        }

        #endregion

        [HttpGet]
        public async Task<dynamic> Gets(long nEmployeeId)
        {
            return await _repository.Gets(nEmployeeId);
        }
   
    }
}
