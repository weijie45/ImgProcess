using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public class SettingsImplement : SettingsRepository
    {
        // Fields
        private readonly string _connectionString = null;


        // Constructors
        public SettingsImplement(string connectionString)
        {
            #region Contracts

            if (string.IsNullOrEmpty(connectionString) == true) throw new ArgumentException();

            #endregion

            // Default
            _connectionString = connectionString;
        }


        // Methods
        public void Add(Settings setting)
        {
           
        }

        public void Remove(string key)
        {
           
        }

        public Settings FindByKey(string key)
        {
            

            // Return
            return null;
        }

        public List<Settings> FindAll()
        {
            
            return null;
        }


    }

}
