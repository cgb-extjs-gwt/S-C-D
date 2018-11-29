import { findMeta } from "../../Common/Helpers/MetaHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, CostMetaData, FieldType } from "../../Common/States/CostMetaStates";
import { FieldInfo } from "../../Common/States/FieldInfo";
import { TableViewGridProps } from "../../TableView/Components/TableViewGrid";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";
import { TableViewInfo } from "../../TableView/States/TableViewState";
import { StoreDynamicGridProps } from "../../Common/Components/DynamicGrid";

export interface TableViewGridProps2 extends TableViewGridProps, StoreDynamicGridProps { }

export class TableViewGridHelper {
    public static buildGridProps(readUrl: string, schema: TableViewInfo, meta: CostMetaData): TableViewGridProps2 {

        let columns = [];
        let filterDataIndexes = [];

        const costBlockCache = new Map<string, CostBlockMeta>();
        const coordinateColumns = mapToColumnInfo(schema.recordInfo.coordinates, meta, costBlockCache, buildCoordinateColumn);
        const costElementColumns = mapToColumnInfo(
            schema.recordInfo.data,
            meta,
            costBlockCache,
            (costBlock, fieldInfo) => buildCostElementColumn(costBlock, fieldInfo, schema));

        const countColumns = mapToColumnInfo(schema.recordInfo.data, meta, costBlockCache, buildCountColumns);

        columns.push(...countColumns, ...coordinateColumns, ...costElementColumns);

        coordinateColumns.forEach(column => filterDataIndexes.push(column.dataIndex));

        return {
            columns,
            filterDataIndexes,
            apiUrls: {
                read: readUrl
            },
            store: null
        } as TableViewGridProps2;
    }
}

const mapToColumnInfo = (
    fieldIfnos: FieldInfo[],
    meta: CostMetaData,
    costBlockCache: Map<string, CostBlockMeta>,
    mapFn: (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => ColumnInfo<TableViewRecord>
) => fieldIfnos.map(fieldInfo => {
    let costBlockMeta = costBlockCache.get(fieldInfo.metaId);
    if (!costBlockMeta) {
        costBlockMeta = findMeta(meta.costBlocks, fieldInfo.metaId);

        costBlockCache.set(fieldInfo.metaId, costBlockMeta);
    }

    return mapFn(costBlockMeta, fieldInfo);
})

const buildColumn = (item: NamedId, fieldInfo: FieldInfo) => ({
    title: item.name,
    dataIndex: fieldInfo.dataIndex,
})

const buildCoordinateColumn = (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => {
    let item: NamedId;

    for (const { dependency, inputLevels } of costBlock.costElements) {
        const items = dependency ? [dependency, ...inputLevels] : inputLevels;

        item = items.find(x => x.id == fieldInfo.fieldName);

        if (item) {
            break;
        }
    }

    return <ColumnInfo<TableViewRecord>>{
        ...buildColumn(item, fieldInfo),
        type: ColumnType.Text,
        mappingFn: (record: TableViewRecord) => record.coordinates[fieldInfo.dataIndex].name
    };
}

const buildCostElementColumn = (costBlock: CostBlockMeta, fieldInfo: FieldInfo, state: TableViewInfo) => {
    let type: ColumnType;
    let referenceItems: Map<string, NamedId>;

    const costElement = findMeta(costBlock.costElements, fieldInfo.fieldName);
    const fieldType = costElement.typeOptions ? costElement.typeOptions.Type : FieldType.Double;

    switch (fieldType) {
        case FieldType.Double:
            type = ColumnType.Numeric;
            break;

        case FieldType.Flag:
            type = ColumnType.CheckBox;
            break;

        case FieldType.Reference:
            type = ColumnType.Reference;
            referenceItems = new Map<string, NamedId>();

            state.references[fieldInfo.dataIndex].forEach(item => referenceItems.set(item.id, item));
            break;
    }

    return <ColumnInfo<TableViewRecord>>{
        ...buildColumn(costElement, fieldInfo),
        isEditable: true,
        type,
        referenceItems,
        mappingFn: record => record.data[fieldInfo.dataIndex].value,
        editMappingFn: (record, dataIndex) => record.data.data[dataIndex].value = record.get(dataIndex),
        rendererFn: (value, record) => {
            const dataIndex = buildCountDataIndex(fieldInfo.dataIndex);
            const count = record.get(dataIndex);

            return count == 1 ? value : `(${count} values)`;
        }
    };
}

const buildCountColumns = (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => (<ColumnInfo<TableViewRecord>>{
    isInvisible: true,
    dataIndex: buildCountDataIndex(fieldInfo.dataIndex),
    mappingFn: record => record.data[fieldInfo.dataIndex].count
})

const buildCountDataIndex = (dataIndex: string) => `${dataIndex}_Count`
