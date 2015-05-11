using System;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService.Core.Account
{
    public class AccountBus
    {
        public  static bool CheckAccountAsync(string uid ,string pwd)
        {
            AccountManage  m = AccountManage.GetInstance();
            return  m.CheckAccount(uid, pwd);
        }
        
    }


}