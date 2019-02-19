using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
    public class PartnerRequestBase: IValidate
    {
        /// <summary>
        /// 请求的第三方AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 请求的第三方AppSecret
        /// </summary>
        public string AppSecret { get; set; }

        public virtual bool Validate()
        {
            return !string.IsNullOrEmpty(AppKey) && !string.IsNullOrEmpty(AppSecret);
        }

        public bool BaseValidate()
        {
            return !string.IsNullOrEmpty(AppKey) && !string.IsNullOrEmpty(AppSecret);
        }
    }
}
