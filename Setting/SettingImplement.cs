using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setting
{
    public class SettingImplement : SettingRepository
    {
        // Fields
        private readonly string _connectionString = null;


        // Constructors
        public SettingImplement(string connectionString)
        {
            #region Contracts

            if (string.IsNullOrEmpty(connectionString) == true) throw new ArgumentException();

            #endregion

            // Default
            _connectionString = connectionString;
        }


        // Methods
        public void Add(Setting setting)
        {
           
        }

        public void Remove(string key)
        {
           
        }

        public Setting FindByKey(string key)
        {
            

            // Return
            return null;
        }

        public List<Setting> FindAll()
        {
            
            return null;
        }


    }

}
