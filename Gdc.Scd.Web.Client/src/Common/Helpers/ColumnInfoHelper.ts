import { CostBlockMeta, CostMetaData } from "../States/CostMetaStates";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";


export const getDependecyColumns = (costBlock: CostBlockMeta) => {
    const dependencyColumnsMap = new Map<string, ColumnInfo>();

    for (const costElement of costBlock.costElements) {
        if (costElement.dependency && !dependencyColumnsMap.has(costElement.dependency.name)) {
            dependencyColumnsMap.set(
                costElement.dependency.name, 
                <ColumnInfo>{
                    title: costElement.dependency.name,
                    dataIndex: `${costElement.dependency.id}Name`,
                    type: ColumnType.Simple
                });
        }
    }

    return Array.from(dependencyColumnsMap.values());
}

export const getDependecyColumnsFromMeta = (meta: CostMetaData, costBlockId: string) => {
    const costBlock = meta.costBlocks.find(item => item.id === costBlockId);

    return getDependecyColumns(costBlock);
}