import { findMeta, getCostBlock, getCostElement } from "../../Common/Helpers/MetaHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, CostMetaData, FieldType } from "../../Common/States/CostMetaStates";
import { FieldInfo } from "../../Common/States/FieldInfo";
import { TableViewGridProps } from "../../TableView/Components/TableViewGrid";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";
import { TableViewInfo, QualityGateResultSet } from "../../TableView/States/TableViewState";
import { StoreDynamicGridProps } from "../../Common/Components/DynamicGrid";
import { QualtityGateTab } from "../../TableView/Components/QualtityGateSetView";

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

        const wgAdditionalColumns = [
            buildAdditionalColumns("WG Full name", "Wg.Description"),
            buildAdditionalColumns("PLA", "Wg.PLA"),
            buildAdditionalColumns("Responsible Person", "Wg.ResponsiblePerson")
        ]

        columns.push(...countColumns, ...coordinateColumns, ...wgAdditionalColumns, ...costElementColumns);

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

    public static buildCountDataIndex(dataIndex: string): string {
        return buildCountDataIndex(dataIndex);
    }

    public static isEqualCoordinates(
        { coordinates: coord1 }: TableViewRecord,
        { coordinates: coord2 }: TableViewRecord
    ): boolean {
        return Object.keys(coord1).every(key => coord1[key].id === coord2[key].id);
    }

    public static refreshEditRecords(
        oldRecords: TableViewRecord[],
        newRecords: TableViewRecord[],
        index: number
    ): TableViewRecord[] {

        let recs = oldRecords;

        newRecords.forEach(actionRecord => {
            const recordIndex = recs.findIndex(editRecord => TableViewGridHelper.isEqualCoordinates(editRecord, actionRecord));

            const changedData = {
                [index]: actionRecord.data[index]
            };

            if (recordIndex == -1) {
                recs = [
                    ...recs,
                    {
                        coordinates: actionRecord.coordinates,
                        data: changedData,
                        additionalData: actionRecord.additionalData
                    }
                ];
            }
            else {
                recs = recs.map(
                    (record, index) =>
                        index == recordIndex
                            ? {
                                coordinates: actionRecord.coordinates,
                                data: {
                                    ...record.data,
                                    ...changedData
                                },
                                additionalData: actionRecord.additionalData
                            }
                            : record
                );
            }
        });

        return recs;
    }

    public static buildErrorTabs(m: QualityGateResultSet, schema: TableViewInfo, meta: CostMetaData): QualtityGateTab[] {

        let tabs: QualtityGateTab[] = [];

        const { recordInfo } = schema;

        for (const item of m.items) {
            if (item.qualityGateResult.hasErrors) {
                const { applicationId, costBlockId, costElementId } = item.costElementIdentifier;
                const fieldInfos = recordInfo.data.filter(
                    fieldInfo =>
                        fieldInfo.metaId == costBlockId &&
                        fieldInfo.fieldName == costElementId
                );
                const costBlock = getCostBlock(meta, costBlockId);
                const costElement = getCostElement(costBlock, costElementId);
                tabs.push(...fieldInfos.map(fieldInfo => <QualtityGateTab>{
                    key: `${applicationId}_${costBlockId}_${costElementId}`,
                    title: `${costBlock.name} ${costElement.name}`,
                    costElement,
                    errors: item.qualityGateResult.errors
                }));
            }
        }

        return tabs;
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

const buildAdditionalColumns = (title, dataIndex) => {
    return <ColumnInfo<TableViewRecord>>{
        title: title,
        dataIndex: dataIndex,
        type: ColumnType.Text,
        mappingFn: record => record.additionalData[dataIndex]
    }
}
