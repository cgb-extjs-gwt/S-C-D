import { CostBlockMeta, CostMetaData, CostElementMeta } from "../States/CostMetaStates";
import { NamedId } from "../States/CommonStates";

export const getDependency = (costBlock: CostBlockMeta, costElementId: string) => {
    const costElement = getCostElement(costBlock, costElementId);

    return costElement.dependency;
}

export const getCostElement = (costBlock: CostBlockMeta, costElementId: string) => findMeta(costBlock.costElements, costElementId);

export const getCostElementByAppMeta = (meta: CostMetaData, costBlockId: string, costElementId: string) => {
    const costBlock = getCostBlock(meta, costBlockId);

    return getCostElement(costBlock, costElementId);
}

export const getCostBlock = (meta: CostMetaData, costBlockId: string) => findMeta(meta.costBlocks, costBlockId);

export const findMeta = <T extends NamedId>(metas: T[], id: string) => metas.find(item => item.id == id)

export const getSortedInputLevels = (costElement: CostElementMeta) => 
    costElement.inputLevels.sort((inputLevel1, inputLevel2) => inputLevel1.levelNumer - inputLevel2.levelNumer);

export const getLastInputLevel = (costElement: CostElementMeta) => {
    const inputLevels = getSortedInputLevels(costElement);

    return inputLevels[inputLevels.length - 1];
}