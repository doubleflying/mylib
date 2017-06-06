using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Enums;
using SqlBuilder.Infrastructure;

namespace SqlBuilder.Resolve
{
    public class EntityResolve
    {
        /// <summary>
        /// 获取转换实体对象描述
        /// </summary>
        /// <typeparam name="TEntity">实体对象类型</typeparam>
        /// <returns></returns>
        internal EntityInfo GetEntityInfo<TEntity>()
        {
            var type = typeof(TEntity);

            var model = GetEntityInfo(type);
            return model;
        }

        internal EntityInfo GetEntityInfo(Type type)
        {
            EntityInfo model;

            if (Configuration.EntityInfos.TryGetValue(type.FullName, out model))
            {
                return model;
            }

            model = new EntityInfo { ClassType = type, ClassName = type.Name };

            #region 表描述

            var abs = type.GetCustomAttributes(true);

            var abLog = abs.FirstOrDefault(item => item is LogAttribute);
            var tbAttrObj = abs.FirstOrDefault(item => item is TableAttribute);

            model.IsLog = abLog != null;//是否需要记录日志

            if (tbAttrObj != null)
            {
                var tbAttr = tbAttrObj as TableAttribute;
                if (tbAttr != null && !string.IsNullOrEmpty(tbAttr.Name))
                {
                    model.TableName = tbAttr.Name;
                }
                else
                {
                    throw new ArgumentNullException("TableName不能为空");
                }
            }
            else
            {
                model.TableName = model.ClassName;
            }
            #endregion

            GetPropertyInfo(type, model);

            GetExecColumns(model, SqlType.Select);
            GetExecColumns(model, SqlType.Insert);
            GetExecColumns(model, SqlType.Update);
            GetExecColumns(model, SqlType.Delete);
            GetExecColumns(model, SqlType.Count);
            GetExecColumns(model, SqlType.Page);

            return model;
        }

        /// <summary>
        /// 获取属性信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        internal void GetPropertyInfo(Type type, EntityInfo model)
        {
            #region 属性描述
            foreach (var propertyInfo in type.GetProperties())
            {
                var pty = new PropertyDes
                {
                    Field = propertyInfo.Name,
                    Column = propertyInfo.Name,
                    PropertyType = propertyInfo.PropertyType,
                    PropertyInfo = propertyInfo
                };

                var attributesList = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);

                pty.CusAttribute = attributesList.Select(item => item as BaseAttribute).ToList();

                var isIgnore = false;

                var customAttribute = propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();

                if (customAttribute is ColumnAttribute)
                {
                    var columnAttribute = (customAttribute as ColumnAttribute) ?? new ColumnAttribute();

                    if (!string.IsNullOrEmpty(columnAttribute.Name))
                    {
                        pty.Column = columnAttribute.Name;
                    }
                }

                foreach (var attributes in attributesList)
                {
                    if (attributes is IgnoreAttribute)
                    {
                        isIgnore = true;
                        break;
                    }

                    if (attributes is KeyAttribute)
                    {
                        //pty.CusAttribute = attributes as KeyAttribute;
                        break;
                    }
                }

                if (!isIgnore)
                {
                    model.Properties.Add(pty);
                }
            }
            #endregion
        }

        /// <summary>
        /// 获取要执行SQL时的列,添加和修改数据时
        /// </summary>
        /// <param name="des"></param>
        /// <param name="getColumnType">列的获取类型</param>
        /// <returns></returns>
        internal void GetExecColumns(EntityInfo des, SqlType sqlType)
        {
            var columns = new List<ParamColumnModel>();

            if (des == null || des.Properties == null) return;

            foreach (var item in des.Properties)
            {
                if (CheckIsKey(item))
                {
                    if (sqlType == SqlType.Insert)
                    {
                        var attribute = item.CusAttribute.FirstOrDefault(ab => ab is KeyAttribute) as KeyAttribute ?? new KeyAttribute();

                        if (attribute.IsIdentity)
                        {
                            continue;
                        }
                    }

                    if (sqlType == SqlType.Update)
                    {
                        continue;
                    }
                }

                if (sqlType == SqlType.Update)
                {
                    if (item.CusAttribute.Any(ab => ab.ColumnType == ColumnTypeEnum.Insert))
                    {
                        continue;
                    }
                }

                if (sqlType == SqlType.Insert || sqlType == SqlType.Update)
                {
                    var attribute1 = item.CusAttribute.FirstOrDefault(ab => ab is IdentityAttribute) as IdentityAttribute;
                    if (attribute1 != null)
                    {
                        continue;
                    }
                }

                columns.Add(new ParamColumnModel() { ColumnName = item.Column ?? item.Field, FieldName = item.Field });
            }

            if (!des.ParamColumns.ContainsKey(sqlType))
            {
                des.ParamColumns.Add(sqlType, columns);
            }
        }

        /// <summary>
        /// 检测是否是主键
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CheckIsKey(PropertyDes property)
        {
            if (property.CusAttribute is KeyAttribute || property.Field == "Id")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取对象的主键标识列和属性
        /// </summary>
        /// <param name="des"></param>
        /// <returns></returns>
        internal PropertyDes GetPrimary(EntityInfo des)
        {
            var p = des.Properties.FirstOrDefault(CheckIsKey);

            if (p == null)
            {
                throw new Exception("没有任何列标记为主键特性");
            }
            return p;
        }
    }
}
