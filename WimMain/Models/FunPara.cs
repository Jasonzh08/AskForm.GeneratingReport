using System;
using System.Collections.Generic;

namespace WimMain.Models
{
    /// <summary>
    /// 方法参数列
    /// </summary>
    public class FunPara
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public List<FunParaContext> Context;

        /// <summary>
        /// 使用次数
        /// </summary>
        public int UseCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public DateTime LastUseDate { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public class FunParaContext
        {
            /// <summary>
            /// 方法参数
            /// </summary>
            public string Paras { get; set; }

            /// <summary>
            /// 使用次数
            /// </summary>
            public int UseCount { get; set; }

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime CreateDate { get; set; }

            /// <summary>
            /// 最后使用时间
            /// </summary>
            public DateTime LastUseDate { get; set; }
        }

    }
}
