import { CostBlockMeta, CostMetaData, InputLevelMeta } from "../States/CostMetaStates";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { NamedId } from "../States/CommonStates";
import { getDependencies } from "./MetaHelper";


export const getDependecyColumnsFromCostBlock = (costBlock: CostBlockMeta) => {
    const dependencies = getDependencies(costBlock);

    return dependencies.map(dependency => buildNameColumnInfo(dependency));
}

export const getDependecyColumns = (dependencies: NamedId[]) => dependencies.map(dependency => buildNameColumnInfo(dependency));

export const getDependecyColumnsFromMeta = (meta: CostMetaData, costBlockId: string) => {
    const costBlock = meta.costBlocks.find(item => item.id === costBlockId);

    return getDependecyColumnsFromCostBlock(costBlock);
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