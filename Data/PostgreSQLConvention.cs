using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Linq;

namespace ABC.Data
{
    public class PostgreSQLConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
            {
                // Thiết lập tên bảng là chữ thường
                var tableName = entity.GetTableName();
                if (tableName != null)
                {
                    entity.SetTableName(tableName.ToLower());
                }

                // Thiết lập tên schema là chữ thường nếu có
                var schema = entity.GetSchema();
                if (schema != null)
                {
                    entity.SetSchema(schema.ToLower());
                }

                // Thiết lập tất cả các cột là chữ thường
                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    if (columnName != null)
                    {
                        property.SetColumnName(columnName.ToLower());
                    }
                }

                // Thiết lập khóa chính là chữ thường
                foreach (var key in entity.GetKeys())
                {
                    var keyName = key.GetName();
                    if (keyName != null)
                    {
                        key.SetName(keyName.ToLower());
                    }
                }

                // Thiết lập các khóa ngoại là chữ thường
                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    var constraintName = foreignKey.GetConstraintName();
                    if (constraintName != null)
                    {
                        foreignKey.SetConstraintName(constraintName.ToLower());
                    }
                }

                // Thiết lập các chỉ mục là chữ thường
                foreach (var index in entity.GetIndexes())
                {
                    var indexName = index.GetDatabaseName();
                    if (indexName != null)
                    {
                        index.SetDatabaseName(indexName.ToLower());
                    }
                }
            }
        }
    }
}
