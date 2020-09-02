using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.DataTransferObjects.Interfaces
{
    interface IDtoMapper<M, Dto>
    {
        /// <summary>
        /// Map from model object to Dto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns>Data Transfer Object</returns>
        static Dto MapToDto(M model) { throw new NotImplementedException(); }
    }
}
