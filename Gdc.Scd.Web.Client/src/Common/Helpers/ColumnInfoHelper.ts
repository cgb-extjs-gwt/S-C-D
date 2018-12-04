import { CostBlockMeta, CostMetaData, InputLevelMeta } from "../States/CostMetaStates";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { NamedId } from "../States/CommonStates";
import { getDependency, findMeta, getSortedInputLevels } from "./MetaHelper";


export const getDependecyColumnFromCostBlock = (costBlock: CostBlockMeta, costElementId: string) => {
    const dependency = getDependency(costBlock, costElementId);

    return buildNameColumnInfo(dependency);
}

export const getDependecyColumnFromMeta = (meta: CostMetaData, costBlockId: string, costElementId: string) => {
    const costBlock = findMeta(meta.costBlocks, costBlockId);

    return getDependecyColumnFromCostBlock(costBlock, costElementId);
}

export const getInputLevelColumns = (costBlock: CostBlockMeta, costElementId: string) => {
    const costElement = findMeta(costBlock.costElements, costElementId);

    return getSortedInputLevels(costElement).map(buildNameColumnInfo);
}

export const buildNameColumnInfo = (metaItem: NamedId) => (<ColumnInfo>{
    title: metaItem.name,
    dataIndex: `${metaItem.id}Name`,
    type: ColumnType.Text
})