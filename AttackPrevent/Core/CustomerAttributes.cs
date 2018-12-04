using AttackPrevent.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Core
{
    public class CheckValidateCodeAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format("Verification code error.");
        }

        public CheckValidateCodeAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            bool bResult = false;
            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (text == configuration?.ValidateCode)
            {
                bResult = true;
            }

            return bResult;
        }
    }
    public class CheckIPAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format("Invalid IP exists.");
        }

        public CheckIPAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            bool bResult = false;
            if (!string.IsNullOrEmpty(text))
            {
                string[] ipList = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ip in ipList)
                {
                    bResult = Utils.IsValidIp(ip);
                    if (!bResult)
                    {
                        break;
                    }
                }
            }

            return bResult;
        }
    }
}
