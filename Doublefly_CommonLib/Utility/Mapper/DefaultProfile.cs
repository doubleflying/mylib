using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Utility.Mapper
{
    public class DefaultProfile<TE, TD> : Profile
    {
        /// <summary>
        /// E2D自定义转化集合
        /// </summary>
        public Dictionary<Expression<Func<TD, object>>, Expression<Func<TE, object>>> E2DDict { get; set; }

        /// <summary>
        /// D2E自定义转化集合
        /// </summary>
        public Dictionary<Expression<Func<TE, object>>, Expression<Func<TD, object>>> D2EDict { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DefaultProfile()
        {
            E2DDict = new Dictionary<Expression<Func<TD, object>>, Expression<Func<TE, object>>>();
            D2EDict = new Dictionary<Expression<Func<TE, object>>, Expression<Func<TD, object>>>();
        }

        protected void AddE2DMember(Expression<Func<TD, object>> dtoExpression, Expression<Func<TE, object>> entityExpression)
        {
            E2DDict.Add(dtoExpression, entityExpression);
        }

        protected void AddD2EMember(Expression<Func<TE, object>> entityExpression, Expression<Func<TD, object>> dtoExpression)
        {
            D2EDict.Add(entityExpression, dtoExpression);
        }
        /// <summary>
        /// 配置
        /// </summary>
        //protected override void Configure()
        //{
        //    var e2D = CreateMap<TE, TD>();
        //    var d2E = CreateMap<TD, TE>();
        //    foreach (var item in E2DDict)
        //    {
        //        KeyValuePair<Expression<Func<TD, object>>, Expression<Func<TE, object>>> item1 = item;

        //        Action<IMemberConfigurationExpression<TE, TD, object>> memberOptions = opt => opt.MapFrom(item1.Value);
        //        e2D = e2D.ForMember(item.Key, memberOptions);
        //    }

        //    foreach (var item in D2EDict)
        //    {
        //        KeyValuePair<Expression<Func<TE, object>>, Expression<Func<TD, object>>> item1 = item;
        //        Action<IMemberConfigurationExpression<TD, TE, object>> memberOptions = opt => opt.MapFrom(item1.Value);
        //        d2E = d2E.ForMember(item.Key, memberOptions);
        //    }
        //}
    }
}
