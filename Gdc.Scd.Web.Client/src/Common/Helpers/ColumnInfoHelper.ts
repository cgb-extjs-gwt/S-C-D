import { CostBlockMeta, CostMetaData, InputLevelMeta } from "../States/CostMetaStates";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { NamedId } from "../States/CommonStates";
import { getDependency, findMeta } from "./MetaHelper";


export const getDependecyColumnFromCostBlock = (costBlock: CostBlockMeta, costElementId: string) => {
    const dependency = getDependency(costBlock, costElementId);

    return buildNameColumnInfo(dependency);
}

//export const getDependecyColumns = (dependencies: NamedId[]) => dependencies.map(dependency => buildNameColumnInfo(dependency));

export const getDependecyColumnFromMeta = (meta: CostMetaData, costBlockId: string, costElementId: string) => {
    const costBlock = findMeta(meta.costBlocks, costBlockId);

    return getDependecyColumnFromCostBlock(costBlock, costElementId);
}

export const getInputLevelColumns = (costBlock: CostBlockMeta) => {
    const inputLevelColumnsMap = new Map<string, InputLevelMeta>();

    for (const costElement of costBlock.costElements) {
        for (const inputLevel of costElement.inputLevels) {
            inputLevelColumnsMap.set(inputLevel.id, inputLevel);
        }
    }

    return Array.from(inputLevelColumnsMap.values())
                .sort((inputLevel1, inputLevel2) => inputLevel1.levelNumer - inputLevel2.levelNumer)
                .map(buildNameColumnInfo);
}

export const buildNameColumnInfo = (metaItem: NamedId) => (<ColumnInfo>{
    title: metaItem.name,
    dataIndex: `${metaItem.id}Name`,
    type: ColumnType.Text
})