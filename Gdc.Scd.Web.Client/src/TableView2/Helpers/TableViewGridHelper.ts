import { findMeta } from "../../Common/Helpers/MetaHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, CostMetaData, FieldType } from "../../Common/States/CostMetaStates";
import { FieldInfo } from "../../Common/States/FieldInfo";
import { TableViewGridProps } from "../../TableView/Components/TableViewGrid";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";
import { TableViewInfo } from "../../TableView/States/TableViewState";

export class TableViewGridHelper {
    public static buildGridProps(url: string, schema: TableViewInfo, meta: CostMetaData): TableViewGridProps {

        let readUrl: string;
        let columns = [];
        let filterDataIndexes = [];

        readUrl = url;

        const costBlockCache = new Map<string, CostBlockMeta>();
        const coordinateColumns = this.mapToColumnInfo(schema.recordInfo.coordinates, meta, costBlockCache, this.buildCoordinateColumn);
        const costElementColumns = this.mapToColumnInfo(
            schema.recordInfo.data,
            meta,
            costBlockCache,
            (costBlock, fieldInfo) => this.buildCostElementColumn(costBlock, fieldInfo, schema));

        const countColumns = this.mapToColumnInfo(schema.recordInfo.data, meta, costBlockCache, this.buildCountColumns);

        columns.push(...countColumns, ...coordinateColumns, ...costElementColumns);

        coordinateColumns.forEach(column => filterDataIndexes.push(column.dataIndex));

        return {
            columns,
            filterDataIndexes,
            apiUrls: {
                read: readUrl
            }
        } as TableViewGridProps;
    }

    public static buildCoordinateColumn(costBlock: CostBlockMeta, fieldInfo: FieldInfo): ColumnInfo<TableViewRecord> {
        let item: NamedId;

        for (const { dependency, inputLevels } of costBlock.costElements) {
            const items = dependency ? [dependency, ...inputLevels] : inputLevels;

            item = items.find(x => x.id == fieldInfo.fieldName);

            if (item) {
                break;
            }
        }

        return {
            ...this.buildColumn(item, fieldInfo),
            type: ColumnType.Text,
            mappingFn: (record: TableViewRecord) => record.coordinates[fieldInfo.dataIndex].name
        } as ColumnInfo<TableViewRecord>;
    }

    public static buildColumn(item: NamedId, fieldInfo: FieldInfo) {
        return {
            title: item.name,
            dataIndex: fieldInfo.dataIndex,
        };
    }

    public static buildCountColumns(costBlock: any, fieldInfo: FieldInfo) {
        return {
            isInvisible: true,
            dataIndex: this.buildCountDataIndex(fieldInfo.dataIndex),
            mappingFn: record => record.data[fieldInfo.dataIndex].count
        } as ColumnInfo<TableViewRecord>;
    }

    public static buildCountDataIndex(dataIndex: string) {
        return `${dataIndex}_Count`;
    }

    public static mapToColumnInfo(
        fieldIfnos: FieldInfo[],
        meta: CostMetaData,
        costBlockCache: Map<string, CostBlockMeta>,
        mapFn: (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => ColumnInfo<TableViewRecord>
    ): any {

        return fieldIfnos.map(fieldInfo => {

            let costBlockMeta = costBlockCache.get(fieldInfo.metaId);
            if (!costBlockMeta) {

                costBlockMeta = findMeta(meta.costBlocks, fieldInfo.metaId);
                costBlockCache.set(fieldInfo.metaId, costBlockMeta);
            }

            return mapFn(costBlockMeta, fieldInfo);
        });
    }

    public static buildCostElementColumn(costBlock: CostBlockMeta, fieldInfo: FieldInfo, state: TableViewInfo) {

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

        return {
            ...this.buildColumn(costElement, fieldInfo),
            isEditable: true,
            type,
            referenceItems,
            mappingFn: record => record.data[fieldInfo.dataIndex].value,
            editMappingFn: (record, dataIndex) => record.data.data[dataIndex].value = record.get(dataIndex),
            rendererFn: (value, record) => {
                const dataIndex = this.buildCountDataIndex(fieldInfo.dataIndex);
                const count = record.get(dataIndex);

                return count == 1 ? value : `(${count} values)`;
            }
        } as ColumnInfo<TableViewRecord>;
    }
}