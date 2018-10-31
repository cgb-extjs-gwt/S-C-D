import { CostBlockMeta } from "../States/CostMetaStates";
import { NamedId } from "../States/CommonStates";

export const getDependency = (costBlock: CostBlockMeta, costElementId: string) => {
    const costElement = findMeta(costBlock.costElements, costElementId);

    return costElement.dependency;
}

export const findMeta = <T extends NamedId>(metas: T[], id: string) => metas.find(item => item.id == id)