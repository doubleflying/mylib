using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Enums;
using Utility.Helper;

namespace Domain
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Disabled = 0;
            CreateTime = DateTime.Now;
            ModifyTime = DateTime.Now;
        }
        /// <summary>
        /// 主键Id
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// 是否无效
        /// </summary>
        [Base(ColumnTypeEnum.Delete)]
        public virtual int Disabled { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Base(ColumnTypeEnum.Insert)]
        public virtual DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Base(ColumnTypeEnum.Insert)]
        public virtual string CreateBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [Base(ColumnTypeEnum.Update)]
        public virtual DateTime ModifyTime { get; set; }

        /// <summary>
        /// 最后修改人
        /// </summary>
        [Base(ColumnTypeEnum.Update)]
        public virtual string ModifyBy { get; set; }

        #region 方法
        /// <summary>
        /// 设置新增属性
        /// </summary>
        /// <param name="userName"></param>
        public void SetInsertProperty(string userName = "")
        {
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
            userName = HttpContextHelper.GetOperator();
            if (!string.IsNullOrEmpty(userName))
            {
                this.CreateBy = userName;
            }
            this.CreateTime = DateTime.Now;
            this.Disabled = 0;

            this.SetModifyProperty(userName);
        }

        /// <summary>
        /// 设置修改属性
        /// </summary>
        /// <param name="userName"></param>
        public void SetModifyProperty(string userName = "")
        {
            userName = HttpContextHelper.GetOperator();

            if (!string.IsNullOrEmpty(userName))
            {
                this.ModifyBy = userName;
            }
            this.ModifyTime = DateTime.Now;
        }

        /// <summary>
        /// 获取默认排序字段
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, OrderByTypeEnum> GetDefaultOrderBy()
        {
            return new Dictionary<string, OrderByTypeEnum> { { "CreateTime", OrderByTypeEnum.Desc } };
        }
        #endregion
    }
}
