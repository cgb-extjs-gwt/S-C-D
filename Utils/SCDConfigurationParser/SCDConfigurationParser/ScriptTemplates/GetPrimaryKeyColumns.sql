USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SELECT  
        COL_NAME(ic.OBJECT_ID,ic.column_id) AS ColumnName,
        case when CHARACTER_MAXIMUM_LENGTH is null then isc.DATA_TYPE 
        else isc.DATA_TYPE +'('+ Convert(nvarchar(5),CHARACTER_MAXIMUM_LENGTH) + ')'
        end as DataType
FROM    sys.indexes AS i 
		INNER JOIN sys.index_columns AS ic ON  i.OBJECT_ID = ic.OBJECT_ID AND i.index_id = ic.index_id       
        INNER JOIN INFORMATION_SCHEMA.COLUMNS AS isc on isc.Column_Name= COL_NAME(ic.OBJECT_ID,ic.column_id)
        and isc.TABLE_NAME='{TableName}'
WHERE   i.is_primary_key = 1 and   OBJECT_NAME(ic.OBJECT_ID)='{TableName}'