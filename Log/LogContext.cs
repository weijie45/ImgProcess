using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    public class LogContext 
    {
        // Fields     
        private readonly LogRepository _logRepoistory = null;

        // Constructors
        public LogContext(LogRepository logRepoistory)
        {
            if (logRepoistory == null) throw new ArgumentException();

            // Default
            _logRepoistory = logRepoistory;

        }

        // Properties        
        public LogRepository LogRepoistory { get { return _logRepoistory; } }


    }
}
