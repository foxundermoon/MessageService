using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MessageService.Core.Data
{
    public class ColumnCollection : InternalDataCollectionBase
    {
        private ArrayList list;
        public Table DataTable { get; set; }
        public ColumnCollection(Table dataTable){
            list = new ArrayList();
            DataTable = dataTable;
        }
        public override int Count
        {
            get
            {
                return list.Count;
            }
        }
        protected override ArrayList List
        {
            get
            {
                return list;
            }
        }
        public ColumnCollection()
        {
            list = new ArrayList();
        }

        // 摘要: 
        //     从集合中获取位于指定索引位置的 System.Data.DataColumn。
        //
        // 参数: 
        //   index:
        //     要返回的列的从零开始的索引。
        //
        // 返回结果: 
        //     指定索引处的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.IndexOutOfRangeException:
        //     索引值大于集合中项的数目。
        public Column this[int index]
        {
            get
            {
                return (Column)List[index];
            }
        }
        //
        // 摘要: 
        //     从集合中获取具有指定名称的 System.Data.DataColumn。
        //
        // 参数: 
        //   name:
        //     要返回的列的 System.Data.DataColumn.ColumnName。
        //
        // 返回结果: 
        //     具有指定 System.Data.DataColumn.ColumnName 的集合中的 System.Data.DataColumn，否则，如果
        //     System.Data.DataColumn 不存在，则为空值。
        public Column this[string name]
        {
            get
            {
                foreach (Column c in list)
                {
                    if (string.Equals(name, c.ColumnName))
                    {
                        return c;
                    }
                }
                return null;
            }
        }

        // 摘要: 
        //     在由于添加或删除列而使列集合发生变化时发生。
        public event CollectionChangeEventHandler CollectionChanged;

        // 摘要: 
        //     创建 System.Data.DataColumn 对象并将其添加到 System.Data.DataColumnCollection 中。
        //
        // 返回结果: 
        //     新创建的 System.Data.DataColumn。
        public Column Add()
        {
            Column c = new Column();
            Add(c);
            DataTable.Columns.Add(c);
            return c;

        }
        //
        // 摘要: 
        //     创建指定的 System.Data.DataColumn 对象并将其添加到 System.Data.DataColumnCollection。
        //
        // 参数: 
        //   column:
        //     要相加的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.ArgumentNullException:
        //     column 参数为 null。
        //
        //   System.ArgumentException:
        //     该列已经属于此集合，或者属于另一个集合。
        //
        //   System.Data.DuplicateNameException:
        //     集合中已存在具有指定名称的列。 （该比较不区分大小写。）
        //
        //   System.Data.InvalidExpressionException:
        //     该表达式无效。 有关如何创建表达式的更多信息，请参见 System.Data.DataColumn.Expression 属性。
        public void Add(Column column)
        {
            list.Add(column);
            DataTable.Columns.Add(column);

        }
        //
        // 摘要: 
        //     创建一个具有指定名称的 System.Data.DataColumn 对象，并将其添加到 System.Data.DataColumnCollection
        //     中。
        //
        // 参数: 
        //   columnName:
        //     列的名称。
        //
        // 返回结果: 
        //     新创建的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.Data.DuplicateNameException:
        //     集合中已存在具有指定名称的列。 （该比较不区分大小写。）
        public Column Add(string columnName)
        {
            var c = Add();
            c.ColumnName = columnName;
            return c;
        }
        //
        // 摘要: 
        //     创建一个具有指定名称和类型的 System.Data.DataColumn 对象，并将其添加到 System.Data.DataColumnCollection
        //     中。
        //
        // 参数: 
        //   columnName:
        //     要在创建列时使用的 System.Data.DataColumn.ColumnName。
        //
        //   type:
        //     新列的 System.Data.DataColumn.DataType。
        //
        // 返回结果: 
        //     新创建的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.Data.DuplicateNameException:
        //     集合中已存在具有指定名称的列。 （该比较不区分大小写。）
        //
        //   System.Data.InvalidExpressionException:
        //     该表达式无效。 有关如何创建表达式的更多信息，请参见 System.Data.DataColumn.Expression 属性。
        public Column Add(string columnName, string dbType)
        {
            var c = Add(columnName);
            c.DbType = dbType;
            return c;

        }
        public Column Add(DataColumn dc)
        {
            var c = new Column(dc.ColumnName);
            c.AllowDBNull = dc.AllowDBNull;
            c.AutoIncrement = dc.AutoIncrement;
            c.AutoIncrementSeed = dc.AutoIncrementSeed;
            c.AutoIncrementStep = dc.AutoIncrementStep;
            c.Caption = dc.Caption;
            c.ColumnMapping = dc.ColumnMapping;
            c.DataType = dc.DataType;
            c.DefaultValue = dc.DefaultValue;
            Add(c);
            return c;
        }
        //
        // 摘要: 
        //     创建一个具有指定名称、类型和表达式的 System.Data.DataColumn 对象，并将其添加到 System.Data.DataColumnCollection
        //     中。
        //
        // 参数: 
        //   columnName:
        //     要在创建列时使用的名称。
        //
        //   type:
        //     新列的 System.Data.DataColumn.DataType。
        //
        //   expression:
        //     要分配给 System.Data.DataColumn.Expression 属性的表达式。
        //
        // 返回结果: 
        //     新创建的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.Data.DuplicateNameException:
        //     集合中已存在具有指定名称的列。 （该比较不区分大小写。）
        //
        //   System.Data.InvalidExpressionException:
        //     该表达式无效。 有关如何创建表达式的更多信息，请参见 System.Data.DataColumn.Expression 属性。
        //
        // 摘要: 
        //     将指定的 System.Data.DataColumn 数组的元素复制到集合末尾。
        //
        // 参数: 
        //   columns:
        //     要添加到集合中的 System.Data.DataColumn 对象的数组。
        public void AddRange(Column[] columns)
        {
            foreach (var c in columns)
            {
                Add(c);
            }

        }
        //
        // 摘要: 
        //     检查是否可从集合中移除特定列。
        //
        // 参数: 
        //   column:
        //     集合中的 System.Data.DataColumn。
        //
        // 返回结果: 
        //     如果可以移除该列，则为 true；否则为 false。
        //
        // 异常: 
        //   System.ArgumentNullException:
        //     column 参数为 null。
        //
        //   System.ArgumentException:
        //     该列不属于此集合。 - 或 - 该列是关系的一部分。 - 或 - 另一个列的表达式取决于该列。
        //
        // 摘要: 
        //     清除集合中的所有列。        //     检查集合是否包含具有指定名称的列。
        //
        // 参数: 
        //   name:
        //     要查找的列的 System.Data.DataColumn.ColumnName。
        //
        // 返回结果: 
        //     如果存在此名称的列，则为 true；否则为 false。
        public bool Contains(string name)
        {
            foreach (Column c in list)
            {
                if (string.Equals(name, c.ColumnName))
                    return true;
            }
            return false;
        }
        //
        // 摘要: 
        //     清除集合中的所有列。
        public void Clear()
        {
            list.Clear();
            
        }
        //
        // 摘要: 
        //     检查集合是否包含具有指定名称的列。
        //
        // 参数: 
        //   name:
        //     要查找的列的 System.Data.DataColumn.ColumnName。
        //
        // 返回结果: 
        //     如果存在此名称的列，则为 true；否则为 false。
        //
        // 摘要: 
        //     将整个集合复制到现有数组中，从该数组内的指定索引处开始复制。
        //
        // 参数: 
        //   array:
        //     将集合复制到其中的 System.Data.DataColumn 对象的数组。
        //
        //   index:

        //
        // 摘要: 
        //     获取按名称指定的列的索引。
        //
        // 参数: 
        //   column:
        //     要返回的列的名称。
        //
        // 返回结果: 
        //     如果找到由 column 指定的列的索引，则为这个索引；否则为 -1。
        public int IndexOf(Column column)
        {
            return list.IndexOf(column);
        }
        //
        // 摘要: 
        //     获取具有特定名称的列的索引（名称不区分大小写）。
        //
        // 参数: 
        //   columnName:
        //     要查找的列的名称。
        //
        // 返回结果: 
        //     具有指定名称的列的从零开始的索引，或者如果集合中不存在该列，则为 -1。
        public int IndexOf(string columnName)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if(string.Equals(((Column)list[i]).ColumnName , columnName)){
                    return i;
                }
            }
            return -1;
        }
        //
        // 摘要: 
        //     从集合中移除指定的 System.Data.DataColumn 对象。
        //
        // 参数: 
        //   column:
        //     要移除的 System.Data.DataColumn。
        //
        // 异常: 
        //   System.ArgumentNullException:
        //     column 参数为 null。
        //
        //   System.ArgumentException:
        //     该列不属于此集合。 - 或 - 该列是关系的一部分。 - 或 - 另一个列的表达式取决于该列。
        public void Remove(Column column)
        {
            list.Remove(column);
        }
        //
        // 摘要: 
        //     从集合中移除具有指定名称的 System.Data.DataColumn 对象。
        //
        // 参数: 
        //   name:
        //     要移除的列的名称。
        //
        // 异常: 
        //   System.ArgumentException:
        //     该集合中没有具有指定名称的列。
        public void Remove(string name)
        {
            foreach(Column c in list){
                if (string.Equals(c.ColumnName, name))
                {
                    list.Remove(c);
                }
            }
        }
        //
        // 摘要: 
        //     从集合中移除指定索引位置的列。
        //
        // 参数: 
        //   index:
        //     要移除的列的索引。
        //
        // 异常: 
        //   System.ArgumentException:
        //     该集合在指定的索引位置没有列。
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }
    }
}
