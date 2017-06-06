using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public abstract class WorkUnit : IDisposable
    {
        private bool disposed;
        private bool isCommit = false;

        public IDbConnection connection;
        public IDbTransaction transaction;
        public bool IsTransaction = false;
        public abstract string ConnectionString { get; set; }

        public WorkUnit BeginTransaction()
        {
            IsTransaction = true;
            connection = new SqlConnection(ConnectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
            return this;
        }

        public void BeginTransaction(WorkUnit unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("WorkUnit不能为空");
            this.connection = unitOfWork.connection;
            this.transaction = unitOfWork.transaction;
            this.IsTransaction = unitOfWork.IsTransaction;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public void Commit()
        {
            isCommit = true;
            transaction.Commit();
            connection.Close();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            isCommit = true;
            transaction.Rollback();
            connection.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (!isCommit)
                    {
                        transaction.Rollback();
                    }
                    connection.Dispose();
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
