import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

export interface CostEditorState {
    // applications: Map<string, NamedId>
    // costBlockMetas: Map<string, CostBlockMeta>
    selectedApplicationId: string
    costBlocks: CostBlockState[]
    //visibleCostBlockIds: string[]
    selectedCostBlockId: string
    dataLossInfo: {
        isWarningDisplayed: boolean
        action: Action<string>
    }
}