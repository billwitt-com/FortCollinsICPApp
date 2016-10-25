using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICPClientLinq


{
    /// <summary>
    ///  
    /// </summary>
    class Errors
    {
        private DateTime date = DateTime.Now;
        private string errorMessage;

        public void LogErrors(string msg)
        {
            errorMessage = string.Format("On {0} ICP Client reported this error: {1}",
                 date.ToShortDateString(), msg); 
            // write to data base

        }
    }
}
