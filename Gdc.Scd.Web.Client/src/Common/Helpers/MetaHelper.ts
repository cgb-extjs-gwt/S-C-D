import { CostBlockMeta } from "../States/CostMetaStates";
import { NamedId } from "../States/CommonStates";

export const getDependency = (costBlock: CostBlockMeta, costElementId: string) => {
    // const dependencyColumnsMap = new Map<string, NamedId>();

    // for (const costElement of costBlock.costElements) {
    //     if (costElement.dependency && !dependencyColumnsMap.has(costElement.dependency.name)) {
    //         dependencyColumnsMap.set(costElement.dependency.name, costElement.dependency);
    //     }
    // }

    // return Array.from(dependencyColumnsMap.values());

    const costElement = findMeta(costBlock.costElements, costElementId);

    return costElement.dependency;
}

export const findMeta = <T extends NamedId>(metas: T[], id: string) => metas.find(item => item.id == id)