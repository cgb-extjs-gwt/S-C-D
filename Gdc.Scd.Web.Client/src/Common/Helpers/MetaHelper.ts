import { CostBlockMeta } from "../States/CostMetaStates";
import { NamedId } from "../States/CommonStates";

export const getDependencies = (costBlock: CostBlockMeta) => {
    const dependencyColumnsMap = new Map<string, NamedId>();

    for (const costElement of costBlock.costElements) {
        if (costElement.dependency && !dependencyColumnsMap.has(costElement.dependency.name)) {
            dependencyColumnsMap.set(costElement.dependency.name, costElement.dependency);
        }
    }

    return Array.from(dependencyColumnsMap.values());
}